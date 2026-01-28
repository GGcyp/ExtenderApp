using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.IO.Binary.LZ4;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary
{
    /// <summary>
    /// 二进制解析器类
    /// </summary>
    internal class BinaryParser : FileParser, IBinaryParser
    {
        /// <summary>
        /// 建议的连续内存大小。
        /// </summary>
        /// <remarks>
        /// 值为1MB（1024 * 1024字节）。
        /// </remarks>
        private const int SuggestedContiguousMemorySize = 1024 * 1024;

        /// <summary>
        /// 压缩文件的最小长度
        /// </summary>
        private const int CompressionMinLength = 64;

        /// <summary>
        /// LZ4块压缩的标识常量
        /// </summary>
        private const sbyte Lz4Block = 99;

        /// <summary>
        /// LZ4块数组压缩的标识常量
        /// </summary>
        private const sbyte Lz4bufferArray = 98;

        /// <summary>
        /// 二进制格式化器解析器
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        /// <summary>
        /// 字节序列池
        /// </summary>
        private readonly SequencePool<byte> _pool;

        /// <summary>
        /// 二进制选项
        /// </summary>
        private readonly BinaryOptions _options;

        /// <summary>
        /// 字节缓冲区转换器
        /// </summary>
        private readonly ByteBufferConvert _convert;

        private readonly IBinaryFormatter<ExtensionHeader> _extensionFormatter;

        private readonly IBinaryFormatter<int> _intFormatter;

        protected override string FileExtension { get; }

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver, SequencePool<byte> sequencePool, ByteBufferConvert convert, BinaryOptions options, IFileOperateProvider provider) : base(provider)
        {
            _resolver = binaryFormatterResolver;
            _pool = sequencePool;
            _options = options;
            _convert = convert;

            _extensionFormatter = _resolver.GetFormatterWithVerify<ExtensionHeader>();
            _intFormatter = _resolver.GetFormatterWithVerify<int>();

            FileExtension = FileExtensions.BinaryFileExtensions;
        }

        #region Get

        public IBinaryFormatter<T> GetFormatter<T>()
        {
            return _resolver.GetFormatterWithVerify<T>();
        }

        #endregion Get

        #region Serialize

        public void Serialize<T>(T value, Span<byte> span)
        {
            if (span.IsEmpty)
                throw new ArgumentNullException(nameof(span));

            Serialize(value, out ByteBuffer buffer);
            if (buffer.Remaining > span.Length)
                throw new ArgumentException("数组内存空间不足", nameof(span));

            buffer.TryCopyTo(span);
            buffer.Dispose();
        }

        public byte[] Serialize<T>(T value)
        {
            Serialize(value, out ByteBuffer buffer);
            var result = buffer.ToArray();
            buffer.Dispose();
            return result;
        }

        public void Serialize<T>(T value, Stream stream)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            Serialize(value, out ByteBuffer buffer);
            buffer.TryCopyTo(stream);
            buffer.Dispose();
        }

        public void Serialize<T>(T value, out ByteBuffer buffer)
        {
            buffer = ByteBuffer.CreateBuffer();
            _resolver.GetFormatterWithVerify<T>().Serialize(ref buffer, value);
        }

        public void Serialize<T>(T value, out ByteBlock block)
        {
            ByteBuffer buffer = ByteBuffer.CreateBuffer();
            _resolver.GetFormatterWithVerify<T>().Serialize(ref buffer, value);
            block = new((int)buffer.Length);
            block.Write(buffer);
            buffer.Dispose();
        }

        #endregion Serialize

        #region Deserialize

        public T? Deserialize<T>(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                throw new ArgumentNullException(nameof(span));

            ByteBlock block = new ByteBlock(span);
            var result = Deserialize<T>(ref block);
            block.Dispose();
            return result;
        }

        public T? Deserialize<T>(ReadOnlyMemory<byte> memory)
        {
            if (memory.IsEmpty)
                throw new ArgumentNullException(nameof(memory));

            ByteBuffer buffer = new ByteBuffer(memory);
            var result = Deserialize<T>(ref buffer);
            buffer.Dispose();
            return result;
        }

        /// <summary>
        /// 反序列化数据。
        /// </summary>
        /// <typeparam name="T">反序列化后的数据类型</typeparam>
        /// <param name="buffer">用于读取数据的<see cref="ByteBuffer"/>对象</param>
        /// <returns>反序列化后的对象，如果剩余数据为0且没有默认格式化器，则返回默认值</returns>
        public T? Deserialize<T>(ref ByteBuffer buffer)
        {
            if (buffer.Remaining == 0)
            {
                return default;
            }

            T? result;
            if (TryDecompress(ref buffer, out var outBuffer))
            {
                result = _resolver.GetFormatterWithVerify<T>().Deserialize(ref outBuffer);
            }
            else
            {
                result = _resolver.GetFormatterWithVerify<T>().Deserialize(ref buffer);
            }

            outBuffer.Dispose();
            return result;
        }

        public T? Deserialize<T>(ref ByteBlock block)
        {
            ByteBuffer buffer = (ByteBuffer)block;
            T? result = Deserialize<T>(ref buffer);
            buffer.Dispose();
            return result;
        }

        public T? Deserialize<T>(Stream stream)
        {
            if (TryDeserializeFromMemoryStream<T>(stream, out var result))
            {
                return result;
            }

            ByteBuffer buffer = ByteBuffer.CreateBuffer();
            int bytesRead = 0;
            do
            {
                Span<byte> span = buffer.GetSpan(stream.CanSeek ? (int)System.Math.Min(SuggestedContiguousMemorySize, stream.Length - stream.Position) : 0);
                bytesRead = stream.Read(span);
                buffer.WriteAdvance(bytesRead);
            }
            while (bytesRead > 0);

            result = DeserializeFromSequenceAndRewindStreamIfPossible<T>(stream, buffer);

            buffer.Dispose();
            return result;
        }

        /// <summary>
        /// 从只读序列中反序列化对象，并在可能的情况下将流重置到未读取的第一个字节。
        /// </summary>
        /// <typeparam name="T">要反序列化的对象的类型。</typeparam>
        /// <param name="stream">包含要反序列化的数据的流。</param>
        /// <param name="sequence">包含要反序列化的数据的只读序列。</param>
        /// <returns>反序列化后的对象。</returns>
        /// <exception cref="ArgumentNullException">如果stream为null。</exception>
        private T? DeserializeFromSequenceAndRewindStreamIfPossible<T>(Stream stream, ByteBuffer buffer)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            T? result = Deserialize<T>(ref buffer);

            //读取完成后返回到未读取的第一个字节
            if (stream.CanSeek && !buffer.End)
            {
                long bytesNotRead = buffer.Sequence.Slice(buffer.Position).Length;
                stream.Seek(-bytesNotRead, SeekOrigin.Current);
            }

            return result;
        }

        /// <summary>
        /// 尝试从内存流中反序列化对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的对象的类型。</typeparam>
        /// <param name="stream">包含要反序列化的数据的流。</param>
        /// <param name="result">反序列化后的对象。</param>
        /// <returns>如果成功反序列化，则返回true；否则返回false。</returns>
        private bool TryDeserializeFromMemoryStream<T>(Stream stream, out T? result)
        {
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> streamBuffer))
            {
                var mbuffer = streamBuffer.AsMemory(checked((int)ms.Position));
                var buffer = new ByteBuffer(mbuffer);

                result = Deserialize<T>(ref buffer);

                var pos = buffer.Length - buffer.Remaining;
                ms.Seek(pos, SeekOrigin.Current);
                return true;
            }

            result = default;
            return false;
        }

        #endregion Deserialize

        #region Execute

        protected override Result<T?> ExecuteRead<T>(IFileOperate fileOperate, long position, int length) where T : default
        {
            try
            {
                fileOperate.Read(position, length, out ByteBuffer buffer);
                T? result = Deserialize<T>(ref buffer);
                buffer.Dispose();
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.FromException<T?>(ex);
            }
        }

        protected override async ValueTask<Result<T?>> ExecuteReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token) where T : default
        {
            try
            {
                ByteBlock block = new(length);
                await fileOperate.ReadAsync(block.GetMemory(length), token);
                T? result = Deserialize<T>(block.UnreadMemory);
                block.Dispose();
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.FromException<T?>(ex);
            }
        }

        protected override Result ExecuteWrite<T>(IFileOperate fileOperate, T value, long position)
        {
            try
            {
                Serialize(value, out ByteBuffer buffer);
                fileOperate.Write(ref buffer);
                buffer.Dispose();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.FromException<T?>(ex);
            }
        }

        protected override ValueTask<Result> ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default)
        {
            try
            {
                Serialize(value, out ByteBlock block);
                return ExecuteWriteAsyncprivate(fileOperate, position, block, token);
            }
            catch (Exception ex)
            {
                return ValueTask.FromResult(Result.FromException(ex));
            }
        }

        private async ValueTask<Result> ExecuteWriteAsyncprivate(IFileOperate fileOperate, long position, ByteBlock block, CancellationToken token)
        {
            try
            {
                await fileOperate.WriteAsync(position, block.UnreadMemory, CancellationToken.None);
                block.Dispose();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        #endregion Execute

        #region Count

        public long GetLength<T>(T value)
        {
            return _resolver.GetFormatterWithVerify<T>().GetLength(value);
        }

        public long GetDefaulLength<T>()
        {
            return _resolver.GetFormatterWithVerify<T>().DefaultLength;
        }

        #endregion Count

        #region LZ4

        /// <summary>
        /// LZ4 变换委托签名。
        /// </summary>
        /// <param name="input">输入只读字节序列。</param>
        /// <param name="output">输出目标缓冲区（由调用方预先分配）。</param>
        /// <returns>实际写入到 <paramref name="output"/> 的字节数。</returns>
        private delegate int LZ4Transform(ReadOnlySpan<byte> input, Span<byte> output);

        /// <summary>
        /// LZ4 编码实现（压缩）。
        /// </summary>
        private static readonly LZ4Transform LZ4CodecEncode = LZ4Codec.Encode;

        /// <summary>
        /// LZ4 解码实现（解压）。
        /// </summary>
        private static readonly LZ4Transform LZ4CodecDecode = LZ4Codec.Decode;

        /// <summary>
        /// 使用 LZ4 算法对输入数据执行指定变换（压缩/解压）。
        /// </summary>
        /// <param name="input">输入数据（可能由多段组成）。</param>
        /// <param name="output">输出缓冲区（调用方保证容量足够）。</param>
        /// <param name="lz4Operation">要执行的 LZ4 变换（EncodeSequence/TryBERDecodeSequence）。</param>
        /// <returns>变换后的实际字节数。</returns>
        /// <remarks>
        /// - 若 <paramref name="input"/> 为多段，会临时从 <see cref="ArrayPool{T}"/> 租用缓冲并拷贝为连续内存；
        ///   用后归还以降低 GC 压力。
        /// - 实际变换工作由 <paramref name="lz4Operation"/> 完成。
        /// </remarks>
        private int LZ4Operation(in ReadOnlySequence<byte> input, Span<byte> output, LZ4Transform lz4Operation)
        {
            ReadOnlySpan<byte> inputSpan;
            byte[]? rentedInputArray = null;
            if (input.IsSingleSegment)
            {
                inputSpan = input.First.Span;
            }
            else
            {
                rentedInputArray = ArrayPool<byte>.Shared.Rent((int)input.Length);
                input.CopyTo(rentedInputArray);
                inputSpan = rentedInputArray.AsSpan(0, (int)input.Length);
            }

            try
            {
                return lz4Operation(inputSpan, output);
            }
            finally
            {
                if (rentedInputArray != null)
                {
                    ArrayPool<byte>.Shared.Return(rentedInputArray);
                }
            }
        }

        /// <summary>
        /// 将只读字节序列按指定压缩方式写入到 Buffer（可选 LZ4 压缩）。
        /// </summary>
        /// <param name="readOnlyMemories">输入只读序列。</param>
        /// <param name="buffer">输出目标 <see cref="ByteBuffer"/>（追加写）。</param>
        /// <param name="compression">压缩方式。</param>
        /// <remarks>
        /// - CompressionType.None 或数据长度小于阈值时，直接原样写入；
        /// - Lz4Block：写入 [Ext(99, compressedLength:int), uncompressedLength:int, compressedBytes]；
        /// - Lz4BlockArray：写入 [Array(sequenceCount+1), Ext(98, extHeaderSize), (length:int)xN, (bin32+compressed)xN]，
        ///   其中 extHeaderSize 为长度序列的编码开销总和。
        /// </remarks>
        private void ToLz4(in ReadOnlySequence<byte> readOnlyMemories, ref ByteBuffer buffer, CompressionType compression)
        {
            if (readOnlyMemories.Length < CompressionMinLength || compression == CompressionType.None)
            {
                buffer.Write(readOnlyMemories);
                return;
            }

            switch (compression)
            {
                case CompressionType.Lz4Block:
                    ToLz4Block(readOnlyMemories, ref buffer);
                    break;

                case CompressionType.Lz4BlockArray:
                    ToLz4BlockArray(readOnlyMemories, ref buffer);
                    break;
            }
        }

        private void ToLz4Block(in ReadOnlySequence<byte> readOnlyMemories, ref ByteBuffer buffer)
        {
            var maxCompressedLength = LZ4Codec.MaximumOutputLength((int)readOnlyMemories.Length);
            var lz4Buffer = ByteBuffer.CreateBuffer();

            int lz4BlockLength = LZ4Operation(readOnlyMemories, lz4Buffer.GetSpan(maxCompressedLength).Slice(0, maxCompressedLength), LZ4CodecEncode);
            lz4Buffer.WriteAdvance(lz4BlockLength);
            _extensionFormatter.Serialize(ref buffer, new ExtensionHeader(Lz4Block, (uint)lz4BlockLength));
            _intFormatter.Serialize(ref buffer, (int)readOnlyMemories.Length);
            // 将LZ4压缩后的数据写入
            buffer.Write(lz4Buffer);
            lz4Buffer.Dispose();
        }

        private void ToLz4BlockArray(in ReadOnlySequence<byte> readOnlyMemories, ref ByteBuffer buffer)
        {
            const int FixedArrayhead = 5; // Array头

            int sequenceCount = 0;
            int extHeaderSize = 0;
            foreach (var item in readOnlyMemories)
            {
                sequenceCount++;
                extHeaderSize += GetUInt32WriteSize((uint)item.Length);
            }

            Serialize(new ExtensionHeader(Lz4bufferArray, (uint)extHeaderSize), out ByteBuffer tempBuffer);
            tempBuffer.TryCopyTo(ref buffer);
            tempBuffer.Dispose();
            _convert.WriteArrayHeader(ref buffer, sequenceCount + 1);
            foreach (var item in readOnlyMemories)
            {
                _intFormatter.Serialize(ref buffer, item.Length);
            }

            int maxCompressedLength;
            foreach (var item in readOnlyMemories)
            {
                maxCompressedLength = LZ4Codec.MaximumOutputLength(item.Length);
                var lz4Span = buffer.GetSpan(maxCompressedLength + FixedArrayhead);
                int Lz4BlockArrayLength = LZ4Codec.Encode(item.Span, lz4Span.Slice(FixedArrayhead, lz4Span.Length - FixedArrayhead));
                WriteBin32Header((uint)Lz4BlockArrayLength, lz4Span);
                buffer.WriteAdvance(Lz4BlockArrayLength + FixedArrayhead);
            }
        }

        /// <summary>
        /// 尝试解析并解压 <paramref name="inputbuffer"/> 中按约定格式写入的 LZ4 负载。
        /// </summary>
        /// <param name="inputbuffer">输入缓冲（读取位置将前进）。</param>
        /// <param name="outbuffer">输出缓冲（成功时填充解压结果；失败时已 Dispose）。</param>
        /// <returns>若检测到 LZ4 帧并成功解压返回 true；否则返回 false。</returns>
        /// <remarks>
        /// 支持两种帧：
        /// - 单块：Ext(99, compressedLength:int) + uncompressedLength:int + compressedBytes
        /// - 多块数组：[Array(n+1), Ext(98, extHeaderSize), (length:int)xN, (bin32+compressed)xN]
        /// </remarks>
        private bool TryDecompress(ref ByteBuffer inputbuffer, out ByteBuffer outbuffer)
        {
            outbuffer = ByteBuffer.CreateBuffer();
            if (inputbuffer.End)
            {
                outbuffer.Dispose();
                return false;
            }

            // Try to find LZ4buffer
            var header = _extensionFormatter.Deserialize(ref inputbuffer);
            if (header.IsEmpty)
            {
                outbuffer.Dispose();
                return false;
            }

            switch (header.TypeCode)
            {
                case 0:
                    outbuffer.Dispose();
                    return false;

                case Lz4Block:
                    return FormLz4Block(ref inputbuffer, out outbuffer);

                case Lz4bufferArray:
                    return FormLz4bufferArray(ref inputbuffer, out outbuffer);

                default:
                    outbuffer.Dispose();
                    return false;
            }
        }

        private bool FormLz4Block(ref ByteBuffer inputbuffer, out ByteBuffer outBuffer)
        {
            outBuffer = new(_pool);
            var peekbuffer = inputbuffer.CreatePeekBuffer();

            //这个整数表示数据在解压缩之后的长度。这样的设计允许接收方在解压数据之前就知道最终数据的大小，
            //这对于数据处理的效率和正确性至关重要。
            int uncompressedLength = _intFormatter.Deserialize(ref peekbuffer);

            //接下来开始解压
            ReadOnlySequence<byte> compressedData = peekbuffer.Sequence.Slice(peekbuffer.Position);

            Span<byte> uncompressedSpan = outBuffer.GetSpan(uncompressedLength).Slice(0, uncompressedLength);
            int actualUncompressedLength = LZ4Operation(compressedData, uncompressedSpan, LZ4CodecDecode);
            outBuffer.WriteAdvance(actualUncompressedLength);

            return true;
        }

        private bool FormLz4bufferArray(ref ByteBuffer inputbuffer, out ByteBuffer outBuffer)
        {
            outBuffer = new(_pool);
            var peekbuffer = inputbuffer.CreatePeekBuffer();
            // 读取数组头
            var arrayLength = _convert.ReadArrayHeader(ref peekbuffer);
            if (arrayLength == 0)
            {
                outBuffer.Dispose();
                return false;
            }

            // 切换成原读取器
            inputbuffer = peekbuffer;

            // 开始读取 [Ext(98:int,int...), bin,bin,bin...]
            var sequenceCount = arrayLength - 1;
            var uncompressedLengths = ArrayPool<int>.Shared.Rent(sequenceCount);
            try
            {
                for (int i = 0; i < sequenceCount; i++)
                {
                    uncompressedLengths[i] = _intFormatter.Deserialize(ref inputbuffer);
                }

                for (int i = 0; i < sequenceCount; i++)
                {
                    var uncompressedLength = uncompressedLengths[i];
                    ReadOnlySequence<byte> lz4buffer = _convert.ReadBytes(ref inputbuffer);
                    Span<byte> uncompressedSpan = outBuffer.GetSpan(uncompressedLength).Slice(0, uncompressedLength);
                    var actualUncompressedLength = LZ4Operation(lz4buffer, uncompressedSpan, LZ4CodecDecode);
                    outBuffer.WriteAdvance(actualUncompressedLength);
                }
                return true;
            }
            finally
            {
                ArrayPool<int>.Shared.Return(uncompressedLengths);
            }
        }

        /// <summary>
        /// 写入 MessagePack bin32 头（1字节类型码 + 4字节大端长度）。
        /// </summary>
        /// <param name="value">负载长度（字节）。</param>
        /// <param name="span">目标缓冲（至少 5 字节）。</param>
        private void WriteBin32Header(uint value, Span<byte> span)
        {
            unchecked
            {
                span[0] = _options.BinaryCode.Bin32;

                // Write to highest index first so the JIT skips bounds checks on subsequent writes.
                //在进行数组写操作时，如果首先写入数组的最高索引位置，
                //那么在随后的写操作中，即时编译器（Just-In-Time, JIT）可能会跳过数组边界检查，从而提高性能。
                span[4] = (byte)value;
                span[3] = (byte)(value >> 8);
                span[2] = (byte)(value >> 16);
                span[1] = (byte)(value >> 24);
            }
        }

        /// <summary>
        /// 估算以 MessagePack 数值编码写入 <paramref name="value"/> 所需的字节数。
        /// </summary>
        /// <param name="value">待编码的无符号整数。</param>
        /// <returns>
        /// 编码所需总字节数：
        /// - 1 字节：FixPositiveInt；
        /// - 2 字节：UInt8；
        /// - 3 字节：UInt16；
        /// - 5 字节：UInt32。
        /// </returns>
        private int GetUInt32WriteSize(uint value)
        {
            if (value <= _options.BinaryRang.MaxFixPositiveInt)
            {
                return 1;
            }
            else if (value <= byte.MaxValue)
            {
                return 2;
            }
            else if (value <= ushort.MaxValue)
            {
                return 3;
            }
            else
            {
                return 5;
            }
        }

        public byte[] Serialize<T>(T value, CompressionType compression)
        {
            Serialize(value, out ByteBuffer buffer, compression);
            byte[] result = buffer.ToArray();
            buffer.Dispose();
            return result;
        }

        public void Serialize<T>(T value, out ByteBuffer buffer, CompressionType compression)
        {
            Serialize(value, out ByteBuffer inputBuffer);
            buffer = ByteBuffer.CreateBuffer();
            ToLz4(inputBuffer.Sequence, ref buffer, compression);
            inputBuffer.Dispose();
        }

        public void Serialize<T>(T value, out ByteBlock block, CompressionType compression)
        {
            Serialize(value, out ByteBuffer inputBuffer, compression);
            block = new((int)inputBuffer.Length);
            block.Write(inputBuffer);
            inputBuffer.Dispose();
        }

        public void Serialize(ReadOnlySequence<byte> sequence, out ByteBuffer outBuffer, CompressionType compression)
        {
            outBuffer = ByteBuffer.CreateBuffer();
            ToLz4(sequence, ref outBuffer, compression);
        }

        public void Serialize(ReadOnlyMemory<byte> memory, out ByteBlock outBlock, CompressionType compression)
        {
            Serialize(new ReadOnlySequence<byte>(memory), out ByteBuffer buffer, compression);
            outBlock = new((int)buffer.Length);
            outBlock.Write(buffer);
            buffer.Dispose();
        }

        #region Write

        public Result Write<T>(ExpectLocalFileInfo info, T value, CompressionType compression)
        {
            return Write(GetFileOperate(info), value, compression);
        }

        public Result Write<T>(FileOperateInfo info, T value, CompressionType compression)
        {
            return Write(GetFileOperate(info), value, compression);
        }

        public Result Write<T>(IFileOperate fileOperate, T value, CompressionType compression)
        {
            try
            {
                Serialize(value, out ByteBuffer buffer, compression);
                var result = fileOperate.Write(ref buffer);
                buffer.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        public Result Write(ExpectLocalFileInfo info, ReadOnlyMemory<byte> buffer, CompressionType compression)
        {
            return Write(GetFileOperate(info), buffer, compression);
        }

        public Result Write(FileOperateInfo info, ReadOnlyMemory<byte> memory, CompressionType compression)
        {
            return Write(GetFileOperate(info), memory, compression);
        }

        public Result Write(IFileOperate fileOperate, ReadOnlyMemory<byte> memory, CompressionType compression)
        {
            try
            {
                Serialize(memory, out ByteBlock block, compression);
                var result = fileOperate.Write(ref block);
                block.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        public Result Write(ExpectLocalFileInfo info, ReadOnlySequence<byte> sequence, CompressionType compression)
        {
            return Write(GetFileOperate(info), sequence, compression);
        }

        public Result Write(FileOperateInfo info, ReadOnlySequence<byte> sequence, CompressionType compression)
        {
            return Write(GetFileOperate(info), sequence, compression);
        }

        public Result Write(IFileOperate fileOperate, ReadOnlySequence<byte> sequence, CompressionType compression)
        {
            try
            {
                Serialize(sequence, out ByteBuffer buffer, compression);
                var result = fileOperate.Write(ref buffer);
                buffer.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        public ValueTask<Result> WriteAsync<T>(ExpectLocalFileInfo info, T value, CompressionType compression, CancellationToken token = default)
        {
            return WriteAsync(GetFileOperate(info), value, compression, token);
        }

        public ValueTask<Result> WriteAsync<T>(FileOperateInfo info, T value, CompressionType compression, CancellationToken token = default)
        {
            return WriteAsync(GetFileOperate(info), value, compression, token);
        }

        public ValueTask<Result> WriteAsync<T>(IFileOperate fileOperate, T value, CompressionType compression, CancellationToken token = default)
        {
            try
            {
                Serialize(value, out ByteBuffer buffer, compression);
                var writeTask = fileOperate.WriteAsync(buffer.UnreadSequence, token);
                if (writeTask.IsCompleted)
                {
                    return ValueTask.FromResult(Result.Success());
                }
                return WaitTask(writeTask);
            }
            catch (Exception ex)
            {
                return ValueTask.FromResult(Result.FromException(ex, "写入文件时发生异常"));
            }

            async ValueTask<Result> WaitTask(ValueTask<Result<int>> task)
            {
                try
                {
                    await task;
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    return Result.FromException(ex);
                }
            }
        }

        public ValueTask<Result> WriteAsync(ExpectLocalFileInfo info, ReadOnlyMemory<byte> memory, CompressionType compression, CancellationToken token = default)
        {
            return WriteAsync(GetFileOperate(info), memory, compression, token);
        }

        public ValueTask<Result> WriteAsync(FileOperateInfo info, ReadOnlyMemory<byte> memory, CompressionType compression, CancellationToken token = default)
        {
            return WriteAsync(GetFileOperate(info), memory, compression, token);
        }

        public async ValueTask<Result> WriteAsync(IFileOperate fileOperate, ReadOnlyMemory<byte> memory, CompressionType compression, CancellationToken token = default)
        {
            try
            {
                Serialize(memory, out ByteBlock block, compression);
                await fileOperate.WriteAsync(block.UnreadMemory, token);
                block.Dispose();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        public ValueTask<Result> WriteAsync(ExpectLocalFileInfo info, ReadOnlySequence<byte> sequence, CompressionType compression, CancellationToken token = default)
        {
            return WriteAsync(GetFileOperate(info), sequence, compression, token);
        }

        public ValueTask<Result> WriteAsync(FileOperateInfo info, ReadOnlySequence<byte> sequence, CompressionType compression, CancellationToken token = default)
        {
            return WriteAsync(GetFileOperate(info), sequence, compression, token);
        }

        public async ValueTask<Result> WriteAsync(IFileOperate fileOperate, ReadOnlySequence<byte> sequence, CompressionType compression, CancellationToken token = default)
        {
            try
            {
                foreach (var memory in sequence)
                {
                    await WriteAsync(fileOperate, memory, token);
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        #endregion Write

        #endregion LZ4
    }
}