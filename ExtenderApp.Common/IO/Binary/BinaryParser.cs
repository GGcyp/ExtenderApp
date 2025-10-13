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
        private const sbyte Lz4buffer = 99;

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

        protected override string FileExtension { get; }

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver, SequencePool<byte> sequencePool, ByteBufferConvert convert, BinaryOptions options, IFileOperateProvider provider) : base(provider)
        {
            _resolver = binaryFormatterResolver;
            _pool = sequencePool;
            _options = options;
            _convert = convert;

            FileExtension = FileExtensions.BinaryFileExtensions;
        }

        #region Get

        public IBinaryFormatter<T> GetFormatter<T>()
        {
            return _resolver.GetFormatterWithVerify<T>();
        }

        #endregion Get

        #region Serialize

        public void Serialize<T>(T value, byte[] bytes, out int length)
        {
            Serialize(value, bytes, out long longLength);
            length = (int)longLength;
        }

        public void Serialize<T>(T value, byte[] bytes, out long length)
        {
            bytes.ArgumentNull(nameof(bytes));

            length = GetLength(value);
            if (bytes.LongLength < length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            var buffer = new ByteBuffer(_pool);
            Serialize(ref buffer, value);
            buffer.TryCopyTo(bytes);
            buffer.Dispose();
        }

        public void Serialize<T>(T value, byte[] bytes)
        {
            bytes.ArgumentNull(nameof(bytes));

            var result = SerializeForArrayPool(value, out int length);
            if (bytes.LongLength < length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            result.CopyTo(bytes, 0);
            ArrayPool<byte>.Shared.Return(result);
        }

        public byte[] Serialize<T>(T value)
        {
            var buffer = new ByteBuffer(_pool);
            Serialize(ref buffer, value);
            var result = buffer.ToArray();
            buffer.Dispose();
            return result;
        }

        public void Serialize<T>(ref ByteBuffer buffer, T value)
        {
            try
            {
                _resolver.GetFormatterWithVerify<T>().Serialize(ref buffer, value);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Serialize<T>(Stream stream, T value)
        {
            var buffer = new ByteBuffer(_pool);
            Serialize(ref buffer, value);

            foreach (ReadOnlyMemory<byte> segment in buffer.Sequence)
            {
                var sharedBuffer = ArrayPool<byte>.Shared.Rent(segment.Length);
                segment.CopyTo(sharedBuffer);
                stream.Write(sharedBuffer, 0, sharedBuffer.Length);
            }
            buffer.Dispose();
        }

        public Task<byte[]> SerializeAsync<T>(T value, CancellationToken token)
        {
            return Task.Run(() =>
            {
                var formatter = _resolver.GetFormatterWithVerify<T>();
                var buffer = new ByteBuffer(_pool);
                formatter.Serialize(ref buffer, value);
                var result = buffer.ToArray();
                buffer.Dispose();
                return result;
            }, token);
        }

        public Task SerializeAsync<T>(Stream stream, T value, CancellationToken token)
        {
            return Task.Run(() =>
            {
                var buffer = new ByteBuffer(_pool);
                Serialize(ref buffer, value);
                foreach (ReadOnlyMemory<byte> segment in buffer.Sequence)
                {
                    var sharedBuffer = ArrayPool<byte>.Shared.Rent(segment.Length);
                    segment.CopyTo(sharedBuffer);
                    stream.Write(sharedBuffer, 0, sharedBuffer.Length);
                }
                buffer.Dispose();
            }, token);
        }

        /// <summary>
        /// 将指定对象序列化为字节数组，并存储在 ArrayPool 中。
        /// </summary>
        /// <typeparam name="T">要序列化的对象的类型。</typeparam>
        /// <param name="value">要序列化的对象。</param>
        /// <returns>包含序列化数据的字节数组。</returns>
        public byte[] SerializeForArrayPool<T>(T value, out long length)
        {
            var buffer = new ByteBuffer(_pool);
            Serialize(ref buffer, value);
            length = buffer.Length;
            byte[] bytes = ArrayPool<byte>.Shared.Rent((int)length);
            buffer.TryCopyTo(bytes);
            buffer.Dispose();
            return bytes;
        }

        public byte[] SerializeForArrayPool<T>(T value, out int length)
        {
            var bytes = SerializeForArrayPool(value, out long longLength);
            length = (int)longLength;
            return bytes;
        }

        #endregion Serialize

        #region Deserialize

        public T? Deserialize<T>(ReadOnlyMemory<byte> memory)
        {
            if (memory.IsEmpty)
                throw new ArgumentNullException(nameof(memory));

            ByteBuffer buffer = new ByteBuffer(memory);
            return Deserialize<T>(ref buffer);
        }

        /// <summary>
        /// 反序列化数据。
        /// </summary>
        /// <typeparam name="T">反序列化后的数据类型</typeparam>
        /// <param name="rbuffer">用于读取数据的<see cref="ByteBuffer"/>对象</param>
        /// <returns>反序列化后的对象，如果剩余数据为0且没有默认格式化器，则返回默认值</returns>
        private T? Deserialize<T>(ref ByteBuffer rbuffer)
        {
            if (rbuffer.Remaining == 0)
            {
                var formatter = _resolver.GetFormatter<T>();
                return default;
            }

            if (TryDecompress(ref rbuffer, out var buffer))
            {
                rbuffer.Dispose();
                rbuffer = new ByteBuffer(buffer.Sequence);
            }

            T? result = _resolver.GetFormatterWithVerify<T>().Deserialize(ref rbuffer);
            rbuffer.Dispose();
            return result;
        }

        public T? Deserialize<T>(Stream stream)
        {
            if (TryDeserializeFromMemoryStream<T>(stream, out var result))
            {
                return result;
            }

            ByteBuffer buffer = new ByteBuffer(_pool);
            int bytesRead = 0;
            do
            {
                Span<byte> span = buffer.GetSpan(stream.CanSeek ? (int)System.Math.Min(SuggestedContiguousMemorySize, stream.Length - stream.Position) : 0);
                bytesRead = stream.Read(span);
                buffer.WriteAdvance(bytesRead);
            }
            while (bytesRead > 0);

            result = DeserializeFromSequenceAndRewindStreamIfPossible<T>(stream, buffer.Sequence);

            buffer.Dispose();
            return result;
        }

        public async Task<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> span, CancellationToken token)
        {
            if (span.IsEmpty)
                throw new ArgumentNullException(nameof(span));

            return await Task.Run(() =>
            {
                ByteBuffer buffer = new ByteBuffer(span);
                return Deserialize<T>(ref buffer);
            }, token);
        }

        /// <summary>
        /// 异步从流中反序列化对象。
        /// </summary>
        /// <typeparam name="T">要反序列化的对象的类型。</typeparam>
        /// <param name="stream">包含要反序列化的数据的流。</param>
        /// <returns>反序列化后的对象，如果反序列化失败则返回null。</returns>
        public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken token)
        {
            if (TryDeserializeFromMemoryStream<T>(stream, out var result))
            {
                return result;
            }

            var rent = _pool.Rent();
            var sequence = rent.Value;
            int bytesRead = 0;
            while (bytesRead > 0)
            {
                Memory<byte> memory = sequence.GetMemory(stream.CanSeek ? (int)System.Math.Min(SuggestedContiguousMemorySize, stream.Length - stream.Position) : 0);
                bytesRead = await stream.ReadAsync(memory);
                sequence.Advance(bytesRead);
            }

            result = DeserializeFromSequenceAndRewindStreamIfPossible<T>(stream, sequence);

            rent.Dispose();
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
        private T? DeserializeFromSequenceAndRewindStreamIfPossible<T>(Stream stream, ReadOnlySequence<byte> sequence)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            ByteBuffer buffer = new ByteBuffer(sequence);

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
            ByteBuffer buffer = new ByteBuffer(_pool);
            fileOperate.Read(ref buffer);
            T? result = Deserialize<T>(ref buffer);
            buffer.Dispose();
            return result;
        }

        protected override T? ExecuteRead<T>(IFileOperate fileOperate, long position, int length) where T : default
        {
            ByteBuffer buffer = new ByteBuffer(_pool);
            fileOperate.Read(position, length, ref buffer);
            T? result = Deserialize<T>(ref buffer);
            buffer.Dispose();
            return result;
        }

        protected override Task<T?> ExecuteReadAsync<T>(IFileOperate fileOperate, CancellationToken token) where T : default
        {
            return Task.Run(() =>
            {
                ByteBuffer buffer = new ByteBuffer(_pool);
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
                ByteBuffer buffer = new ByteBuffer(_pool);
                fileOperate.Read(ref buffer);
                T? result = Deserialize<T>(ref buffer);
                buffer.Dispose();
                return result;
            }, token);
        }

        protected override void ExecuteWrite<T>(IFileOperate fileOperate, T value)
        {
            ByteBuffer buffer = new ByteBuffer(_pool);
            Serialize(ref buffer, value);
            fileOperate.Write(buffer);
            buffer.Dispose();
        }

        protected override void ExecuteWrite<T>(IFileOperate fileOperate, T value, long position)
        {
            ByteBuffer buffer = new ByteBuffer(_pool);
            Serialize(ref buffer, value);
            fileOperate.Write(position, buffer);
            buffer.Dispose();
        }

        protected override Task ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                ByteBuffer buffer = new ByteBuffer(_pool);
                Serialize(ref buffer, value);
                fileOperate.Write(buffer);
                buffer.Dispose();
            }, token);
        }

        protected override Task ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                ByteBuffer buffer = new ByteBuffer(_pool);
                Serialize(ref buffer, value);
                fileOperate.Write(position, buffer);
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

        private delegate int LZ4Transform(ReadOnlySpan<byte> input, Span<byte> output);

        private static readonly LZ4Transform LZ4CodecEncode = LZ4Codec.Encode;
        private static readonly LZ4Transform LZ4CodecDecode = LZ4Codec.Decode;

        /// <summary>
        /// 使用LZ4算法对输入数据进行操作。
        /// </summary>
        /// <param name="input">输入数据，类型为ReadOnlySequence<byte>。</param>
        /// <param name="output">输出缓冲区，类型为Span<byte>。</param>
        /// <param name="lz4Operation">LZ4操作类型，类型为LZ4Transform。</param>
        /// <returns>操作结果，返回值为int类型。</returns>
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

        public byte[] Compression<T>(T value, CompressionType compression)
        {
            var scratchbuffer = new ByteBuffer(_pool);
            Serialize(ref scratchbuffer, value);

            var buffer = new ByteBuffer(_pool);
            ToLz4(scratchbuffer.Sequence, ref buffer, compression);

            byte[] result = buffer.ToArray();

            scratchbuffer.Dispose();
            buffer.Dispose();
            return result;
        }

        public byte[] Compression(ReadOnlySpan<byte> input, CompressionType compression)
        {
            var scratchbuffer = new ByteBuffer(_pool);
            scratchbuffer.Write(input);
            var buffer = new ByteBuffer(_pool);
            ToLz4(scratchbuffer.Sequence, ref buffer, compression);
            byte[] result = buffer.ToArray();
            scratchbuffer.Dispose();
            buffer.Dispose();
            return result;
        }

        public byte[] Compression(in ReadOnlySequence<byte> readOnlyMemories, CompressionType compression)
        {
            ByteBuffer buffer = new ByteBuffer(_pool);
            ToLz4(readOnlyMemories, ref buffer, CompressionType.Lz4BlockArray);
            var result = buffer.ToArray();
            buffer.Dispose();
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

            var scratchbuffer = new ByteBuffer(_pool);
            Serialize(ref scratchbuffer, value);

            var buffer = new ByteBuffer(_pool);
            ToLz4(scratchbuffer.Sequence, ref buffer, compression);

            fileOperate.Write(buffer);

            scratchbuffer.Dispose();
            buffer.Dispose();
        }

        public void ToLz4(in ReadOnlySequence<byte> readOnlyMemories, ref ByteBuffer buffer, CompressionType compression)
        {
            if (readOnlyMemories.Length < CompressionMinLength || compression == CompressionType.None)
            {
                buffer.Write(readOnlyMemories);
                return;
            }

            switch (compression)
            {
                case CompressionType.Lz4Block:
                    // 如果是LZ4压缩，则需要在前面添加LZ4块头
                    var maxCompressedLength = LZ4Codec.MaximumOutputLength((int)readOnlyMemories.Length);
                    var lz4Bytes = ArrayPool<byte>.Shared.Rent(maxCompressedLength);
                    try
                    {
                        int lz4Length = LZ4Operation(readOnlyMemories, lz4Bytes, LZ4CodecEncode);
                        _resolver.GetFormatterWithVerify<ExtensionHeader>().Serialize(ref buffer, new ExtensionHeader(Lz4buffer, (uint)lz4Length));
                        _resolver.GetFormatterWithVerify<int>().Serialize(ref buffer, (int)readOnlyMemories.Length);
                        // 将LZ4压缩后的数据写入
                        buffer.Write(lz4Bytes.AsSpan(0, lz4Length));
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(lz4Bytes);
                    }
                    break;

                case CompressionType.Lz4BlockArray:
                    var sequenceCount = 0;
                    var extHeaderSize = 0;
                    foreach (var item in readOnlyMemories)
                    {
                        sequenceCount++;
                        extHeaderSize += GetUInt32WriteSize((uint)item.Length);
                    }

                    _convert.WriteArrayHeader(ref buffer, sequenceCount + 1);
                    Serialize(ref buffer, new ExtensionHeader(Lz4bufferArray, (uint)extHeaderSize));
                    IBinaryFormatter<int> intFormatter = _resolver.GetFormatterWithVerify<int>();
                    foreach (var item in readOnlyMemories)
                    {
                        intFormatter.Serialize(ref buffer, item.Length);
                    }

                    foreach (var item in readOnlyMemories)
                    {
                        maxCompressedLength = LZ4Codec.MaximumOutputLength(item.Length);
                        var lz4Span = buffer.GetSpan(maxCompressedLength + 5);
                        int lz4Length = LZ4Codec.Encode(item.Span, lz4Span.Slice(5, lz4Span.Length - 5));
                        WriteBin32Header((uint)lz4Length, lz4Span);
                        buffer.WriteAdvance(lz4Length + 5);
                    }

                    break;
            }
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
                var scratchbuffer = new ByteBuffer(_pool);
                Serialize(ref scratchbuffer, value);

                var buffer = new ByteBuffer(_pool);
                ToLz4(scratchbuffer.Sequence, ref buffer, compression);

                fileOperate.Write(buffer);
                scratchbuffer.Dispose();
                buffer.Dispose();
            }, token);
        }

        private bool TryDecompress(ref ByteBuffer inputbuffer, out ByteBuffer outbuffer)
        {
            outbuffer = new ByteBuffer(_pool);
            if (inputbuffer.End)
            {
                inputbuffer.Dispose();
                return false;
            }

            // Try to find LZ4buffer
            var header = _resolver.GetFormatterWithVerify<ExtensionHeader>().Deserialize(ref inputbuffer);
            if (header.IsEmpty)
            {
                outbuffer.Dispose();
                return false;
            }

            if (header.TypeCode == Lz4buffer)
            {
                var extbuffer = inputbuffer.CreatePeekBuffer();

                //这个整数表示数据在解压缩之后的长度。这样的设计允许接收方在解压数据之前就知道最终数据的大小，
                //这对于数据处理的效率和正确性至关重要。
                int uncompressedLength = _resolver.GetFormatterWithVerify<int>().Deserialize(ref extbuffer);

                //接下来开始解压
                ReadOnlySequence<byte> compressedData = extbuffer.Sequence.Slice(extbuffer.Position);

                Span<byte> uncompressedSpan = outbuffer.GetSpan(uncompressedLength).Slice(0, uncompressedLength);
                int actualUncompressedLength = LZ4Operation(compressedData, uncompressedSpan, LZ4CodecDecode);
                outbuffer.WriteAdvance(actualUncompressedLength);

                return true;
            }

            var peekbuffer = inputbuffer.CreatePeekBuffer();
            var arrayLength = _convert.ReadArrayHeader(ref peekbuffer);
            if (arrayLength == 0) return false;
            header = Deserialize<ExtensionHeader>(ref inputbuffer);
            if (header.TypeCode != Lz4bufferArray)
            {
                outbuffer.Dispose();
                peekbuffer.Dispose();
                return false;
            }

            // 切换成原读取器
            inputbuffer = peekbuffer;

            // 开始读取 [Ext(98:int,int...), bin,bin,bin...]
            var sequenceCount = arrayLength - 1;
            var uncompressedLengths = ArrayPool<int>.Shared.Rent(sequenceCount);
            try
            {
                IBinaryFormatter<int> intFormatter = _resolver.GetFormatterWithVerify<int>();
                for (int i = 0; i < sequenceCount; i++)
                {
                    uncompressedLengths[i] = intFormatter.Deserialize(ref inputbuffer);
                }

                for (int i = 0; i < sequenceCount; i++)
                {
                    var uncompressedLength = uncompressedLengths[i];
                    ReadOnlySequence<byte> lz4buffer = _convert.ReadBytes(ref inputbuffer);
                    Span<byte> uncompressedSpan = outbuffer.GetSpan(uncompressedLength);
                    var actualUncompressedLength = LZ4Operation(lz4buffer, uncompressedSpan, LZ4CodecDecode);
                    outbuffer.WriteAdvance(actualUncompressedLength);
                }
                return true;
            }
            finally
            {
                ArrayPool<int>.Shared.Return(uncompressedLengths);
            }
        }

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

        #endregion LZ4
    }
}