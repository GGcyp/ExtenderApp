using System.Buffers;
using System.Reflection.PortableExecutable;
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
        /// 字节缓冲区工厂
        /// </summary>
        private readonly IByteBufferFactory _bufferFactory;

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

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver, SequencePool<byte> sequencePool, ByteBufferConvert convert, BinaryOptions options, IByteBufferFactory bufferFactory, IFileOperateProvider provider) : base(provider)
        {
            _resolver = binaryFormatterResolver;
            _pool = sequencePool;
            _options = options;
            _convert = convert;
            _bufferFactory = bufferFactory;

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
            buffer = _bufferFactory.Create();
            _resolver.GetFormatterWithVerify<T>().Serialize(ref buffer, value);
        }

        public void Serialize<T>(T value, out ByteBlock block)
        {
            ByteBuffer buffer = _bufferFactory.Create();
            _resolver.GetFormatterWithVerify<T>().Serialize(ref buffer, value);
            block = new((int)buffer.Length);
            buffer.TryCopyTo(ref block);
            buffer.Dispose();
        }

        public Task SerializeAsync<T>(T value, Stream stream, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                Serialize(value, out ByteBuffer buffer);
                buffer.TryCopyTo(stream);
                buffer.Dispose();
            }, token);
        }

        public Task<byte[]> SerializeAsync<T>(T value, CancellationToken token)
        {
            return Task.Run(() =>
            {
                var formatter = _resolver.GetFormatterWithVerify<T>();
                var buffer = _bufferFactory.Create(); ;
                formatter.Serialize(ref buffer, value);
                var result = buffer.ToArray();
                buffer.Dispose();
                return result;
            }, token);
        }

        #endregion Serialize

        #region Deserialize

        public T? Deserialize<T>(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                throw new ArgumentNullException(nameof(span));

            ByteBlock block = span;
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
            ByteBuffer buffer = block;
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

            ByteBuffer buffer = _bufferFactory.Create();
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

        public Task<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> memory, CancellationToken token)
        {
            if (memory.IsEmpty)
                throw new ArgumentNullException(nameof(memory));

            return Task.Run(() =>
            {
                ByteBuffer buffer = memory;
                var result = Deserialize<T>(ref buffer);
                buffer.Dispose();
                return result;
            }, token);
        }

        /// <summary>
        /// 异步从流中反序列化对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的对象的类型。</typeparam>
        /// <param name="stream">包含要反序列化的数据的流。</param>
        /// <returns>反序列化后的对象，如果反序列化失败则返回null。</returns>
        public Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken token)
        {
            return Task.Run<T?>(() =>
            {
                if (TryDeserializeFromMemoryStream<T>(stream, out var result))
                {
                    return result;
                }

                ByteBuffer buffer = _bufferFactory.Create();
                int bytesRead = 0;
                do
                {
                    Span<byte> span = buffer.GetSpan(stream.CanSeek ? (int)System.Math.Min(SuggestedContiguousMemorySize, stream.Length - stream.Position) : 0);
                    bytesRead = stream.Read(span);
                    buffer.WriteAdvance(bytesRead);
                } while (bytesRead > 0);

                result = DeserializeFromSequenceAndRewindStreamIfPossible<T>(stream, buffer);

                buffer.Dispose();
                return result;
            });
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

        protected override T? ExecuteRead<T>(IFileOperate fileOperate) where T : default
        {
            ByteBuffer buffer = _bufferFactory.Create();
            fileOperate.Read(ref buffer);
            T? result = Deserialize<T>(ref buffer);
            buffer.Dispose();
            return result;
        }

        protected override T? ExecuteRead<T>(IFileOperate fileOperate, long position, int length) where T : default
        {
            ByteBuffer buffer = _bufferFactory.Create();
            fileOperate.Read(position, length, ref buffer);
            T? result = Deserialize<T>(ref buffer);
            buffer.Dispose();
            return result;
        }

        protected override Task<T?> ExecuteReadAsync<T>(IFileOperate fileOperate, CancellationToken token) where T : default
        {
            return Task.Run(() =>
            {
                ByteBuffer buffer = _bufferFactory.Create();
                fileOperate.Read(ref buffer);
                T? result = Deserialize<T>(ref buffer);
                buffer.Dispose();
                return result;
            }, token);
        }

        protected override Task<T?> ExecuteReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token) where T : default
        {
            return Task.Run(() =>
            {
                ByteBuffer buffer = _bufferFactory.Create();
                fileOperate.Read(ref buffer);
                T? result = Deserialize<T>(ref buffer);
                buffer.Dispose();
                return result;
            }, token);
        }

        protected override void ExecuteWrite<T>(IFileOperate fileOperate, T value)
        {
            Serialize(value, out ByteBuffer buffer);
            fileOperate.Write(ref buffer);
            buffer.Dispose();
        }

        protected override void ExecuteWrite<T>(IFileOperate fileOperate, T value, long position)
        {
            Serialize(value, out ByteBuffer buffer);
            fileOperate.Write(position, ref buffer);
            buffer.Dispose();
        }

        protected override Task ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                Serialize(value, out ByteBuffer buffer);
                fileOperate.Write(ref buffer);
                buffer.Dispose();
            }, token);
        }

        protected override Task ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                Serialize(value, out ByteBuffer buffer);
                fileOperate.Write(position, ref buffer);
                buffer.Dispose();
            }, token);
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
        /// <param name="lz4Operation">要执行的 LZ4 变换（Encode/Decode）。</param>
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
        /// 将只读字节序列按指定压缩方式写入到 buffer（可选 LZ4 压缩）。
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
            var lz4Buffer = _bufferFactory.Create();

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
            outbuffer = _bufferFactory.Create();
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
            Serialize(value, out ByteBuffer buffer);

            var outBuffer = _bufferFactory.Create();
            ToLz4(buffer.Sequence, ref outBuffer, compression);

            byte[] result = outBuffer.ToArray();

            buffer.Dispose();
            outBuffer.Dispose();
            return result;
        }

        public void Write<T>(ExpectLocalFileInfo info, T value, CompressionType compression)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(FileExtension), value, compression);
        }

        public void Write<T>(FileOperateInfo info, T value, CompressionType compression)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(GetOperate(info), value, compression);
        }

        public void Write<T>(IFileOperate fileOperate, T value, CompressionType compression)
        {
            if (fileOperate == null)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }
            Serialize(value, out ByteBuffer buffer);

            var outBuffer = _bufferFactory.Create();
            ToLz4(buffer.Sequence, ref outBuffer, compression);
            fileOperate.Write(ref outBuffer);

            buffer.Dispose();
            outBuffer.Dispose();
        }

        public void Write(ExpectLocalFileInfo info, ref ByteBuffer buffer, CompressionType compression)
        {
            Write(info.CreateReadWriteOperate(FileExtension), ref buffer, compression);
        }

        public void Write(FileOperateInfo info, ref ByteBuffer buffer, CompressionType compression)
        {
            Write(GetOperate(info), ref buffer, compression);
        }

        public void Write(IFileOperate fileOperate, ref ByteBuffer buffer, CompressionType compression)
        {
            if (fileOperate == null)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }

            var outBuffer = _bufferFactory.Create();
            ToLz4(buffer.Sequence, ref outBuffer, compression);
            fileOperate.Write(ref outBuffer);

            outBuffer.Dispose();
        }

        public void Write(ExpectLocalFileInfo info, ref ByteBlock block, CompressionType compression)
        {
            Write(info.CreateReadWriteOperate(FileExtension), ref block, compression);
        }

        public void Write(FileOperateInfo info, ref ByteBlock block, CompressionType compression)
        {
            Write(GetOperate(info), ref block, compression);
        }

        public void Write(IFileOperate fileOperate, ref ByteBlock block, CompressionType compression)
        {
            if (fileOperate == null)
            {
                ErrorUtil.ArgumentNull(nameof(fileOperate));
                return;
            }

            var buffer = new ByteBuffer(block);
            var outBuffer = _bufferFactory.Create();
            ToLz4(buffer.Sequence, ref outBuffer, compression);
            fileOperate.Write(ref outBuffer);

            outBuffer.Dispose();
            buffer.Dispose();
        }

        public Task WriteAsync<T>(ExpectLocalFileInfo info, T value, CompressionType compression, CancellationToken token = default)
        {
            if (info.IsEmpty)
            {
                throw new ArgumentNullException(nameof(info));
            }
            return WriteAsync(info.CreateReadWriteOperate(FileExtension), value, compression, token);
        }

        public Task WriteAsync<T>(FileOperateInfo info, T value, CompressionType compression, CancellationToken token = default)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                throw new ArgumentNullException(nameof(info));
            }
            return WriteAsync(GetOperate(info), value, compression, token);
        }

        public Task WriteAsync<T>(IFileOperate fileOperate, T value, CompressionType compression, CancellationToken token = default)
        {
            if (fileOperate == null)
            {
                throw new ArgumentNullException(nameof(fileOperate));
            }
            return Task.Run(() =>
            {
                Serialize(value, out ByteBuffer buffer);

                var outBuffer = _bufferFactory.Create();
                ToLz4(buffer.Sequence, ref outBuffer, compression);

                fileOperate.Write(ref outBuffer);
                buffer.Dispose();
                outBuffer.Dispose();
            }, token);
        }

        public void Serialize<T>(T value, out ByteBuffer buffer, CompressionType compression)
        {
            Serialize(value, out buffer);

            var outBuffer = _bufferFactory.Create();
            ToLz4(buffer.Sequence, ref outBuffer, compression);

            outBuffer.Dispose();
        }

        public void Serialize<T>(T value, out ByteBlock block, CompressionType compression)
        {
            Serialize(value, out ByteBuffer buffer);

            var outBuffer = _bufferFactory.Create();
            ToLz4(buffer.Sequence, ref outBuffer, compression);

            block = outBuffer;
            outBuffer.Dispose();
        }

        public void Serialize(ref ByteBuffer inputBuffer, out ByteBuffer outBuffer, CompressionType compression)
        {
            if (inputBuffer.IsEmpty)
                throw new ArgumentNullException(nameof(inputBuffer));
            inputBuffer.ThrowIfCannotWrite();

            outBuffer = new(_pool);
            ToLz4(inputBuffer.Sequence, ref outBuffer, compression);
        }

        public void Serialize(ref ByteBlock inputBlock, out ByteBlock outBlock, CompressionType compression)
        {
            if (inputBlock.IsEmpty)
                throw new ArgumentNullException(nameof(inputBlock));

            ByteBuffer inputBuffer = inputBlock;
            ByteBuffer outBuffer = new(_pool);
            ToLz4(inputBuffer.Sequence, ref outBuffer, compression);
            outBlock = outBuffer;
        }

        public Task<byte[]> SerializeAsync<T>(T value, CompressionType compression)
        {
            return Task.Run(() =>
            {
                return Serialize(value, compression);
            });
        }

        #endregion LZ4
    }
}