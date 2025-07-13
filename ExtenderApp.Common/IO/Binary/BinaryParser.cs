using System.Buffers;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.Serialization;
using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.IO.Binaries.LZ4;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries
{
    /// <summary>
    /// 二进制解析器类
    /// </summary>
    internal class BinaryParser : FileParser, IBinaryParser
    {
        /// <summary>
        /// 最大字节数组长度
        /// </summary>
        private const int MaxByteArrayLength = 65536;

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
        private const sbyte Lz4BlockArray = 98;

        /// <summary>
        /// 二进制格式化器解析器
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        /// <summary>
        /// 字节序列池
        /// </summary>
        private readonly SequencePool<byte> _sequencePool;

        /// <summary>
        /// 二进制选项
        /// </summary>
        private readonly BinaryOptions _binaryOptions;

        /// <summary>
        /// 二进制写入转换器
        /// </summary>
        private readonly ExtenderBinaryWriterConvert _writerConvert;

        /// <summary>
        /// 二进制读取转换器
        /// </summary>
        private readonly ExtenderBinaryReaderConvert _readerConvert;

        /// <summary>
        /// 二进制文件扩展名
        /// </summary>
        private readonly string _binaryFileExtensions;

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver, SequencePool<byte> sequencePool, ExtenderBinaryReaderConvert readerConvert, ExtenderBinaryWriterConvert writerConvert, BinaryOptions options, IFileOperateProvider provider) : base(provider)
        {
            _resolver = binaryFormatterResolver;
            _sequencePool = sequencePool;

            _binaryFileExtensions = FileExtensions.BinaryFileExtensions;
            _readerConvert = readerConvert;
            _writerConvert = writerConvert;
            _binaryOptions = options;
        }

        #region Get

        public IBinaryFormatter<T> GetFormatter<T>()
        {
            return _resolver.GetFormatterWithVerify<T>();
        }

        #endregion

        #region Read

        public override T? Read<T>(ExpectLocalFileInfo info) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read<T>(info.CreateReadWriteOperate(_binaryFileExtensions));
        }

        public override T? Read<T>(FileOperateInfo info) where T : default
        {
            if (info.IsEmpty || !info.LocalFileInfo.Exists || !info.IsRead())
            {
                //var formatter = _resolver.GetFormatter<T>();
                //if (formatter == null)
                //    return default;

                //return formatter.Default;
                return default;
            }

            return PrivateRead<T>(GetOperate(info));
        }

        public override T? Read<T>(IFileOperate fileOperate) where T : default
        {
            return PrivateRead<T>(fileOperate);
        }

        public override T? Read<T>(ExpectLocalFileInfo info, long position, int length) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read<T>(info.CreateReadWriteOperate(_binaryFileExtensions), position, length);
        }

        public override T? Read<T>(FileOperateInfo info, long position, int length) where T : default
        {
            if (info.IsEmpty || !info.LocalFileInfo.Exists || !info.IsRead())
            {
                //var formatter = _resolver.GetFormatter<T>();
                //if (formatter == null)
                //    return default;

                //return formatter.Default;
                return default;
            }

            return PrivateRead<T>(GetOperate(info), position, length);
        }

        public override T? Read<T>(IFileOperate fileOperate, long position, int length) where T : default
        {
            return PrivateRead<T>(fileOperate, position, length);
        }

        /// <summary>
        /// 私有方法，用于从指定的并发操作接口中读取指定类型的数据。
        /// </summary>
        /// <typeparam name="T">需要读取的数据类型。</typeparam>
        /// <param name="operate">并发操作接口。</param>
        /// <param name="position">开始读取的位置。</param>
        /// <param name="length">需要读取的长度，默认为-1，表示读取到文件末尾。</param>
        /// <returns>读取到的数据。</returns>
        private T? PrivateRead<T>(IFileOperate operate, long position = 0, int length = -1)
        {
            length = length == -1 ? (int)operate.Info.FileInfo.Length : length;
            var bytes = operate.ReadForArrayPool(position, length);

            var reader = new ExtenderBinaryReader(new Memory<byte>(bytes, 0, length));
            var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
            if (TryDecompress(ref reader, ref writer))
            {
                writer.Commit();
                reader = writer;
            }

            var result = Deserialize<T>(ref reader);
            ArrayPool<byte>.Shared.Return(bytes);
            writer.Dispose();
            return result;
        }

        #endregion

        #region ReadAsync

        public override void ReadAsync<T>(ExpectLocalFileInfo info, Action<T?> callback) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            ReadAsync(info.CreateReadWriteOperate(_binaryFileExtensions), callback);
        }

        public override void ReadAsync<T>(FileOperateInfo info, Action<T?> callback) where T : default
        {
            if (info.IsEmpty || !info.IsRead())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            var operate = GetOperate(info);
            PrivateReadAsync(operate, callback);
        }

        public override void ReadAsync<T>(IFileOperate fileOperate, Action<T?> callback) where T : default
        {
            PrivateReadAsync(fileOperate, callback);
        }

        public override void ReadAsync<T>(ExpectLocalFileInfo info, long position, int length, Action<T?> callback) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            ReadAsync(info.CreateReadWriteOperate(_binaryFileExtensions), position, length, callback);
        }

        public override void ReadAsync<T>(FileOperateInfo info, long position, int length, Action<T?> callback) where T : default
        {
            if (info.IsEmpty || !info.IsRead())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            PrivateReadAsync(GetOperate(info), callback, position, length);
        }

        public override void ReadAsync<T>(IFileOperate fileOperate, long position, int length, Action<T?> callback) where T : default
        {
            PrivateReadAsync(fileOperate, callback, position, length);
        }

        /// <summary>
        /// 异步读取数据，将数据反序列化为泛型类型T，并调用回调函数处理结果。
        /// </summary>
        /// <typeparam name="T">要反序列化的类型</typeparam>
        /// <param name="operate">并发操作接口</param>
        /// <param name="callback">处理结果的回调函数</param>
        /// <param name="position">开始读取的位置</param>
        /// <param name="length">要读取的长度</param>
        private void PrivateReadAsync<T>(IFileOperate operate, Action<T?> callback, long position = 0, int length = -1)
        {
            DataBuffer<Delegate> buffer = DataBuffer<Delegate>.GetDataBuffer();
            buffer.Item1 = callback;
            var action = buffer.GetProcessAction<byte[]>((d, b) =>
            {
                var callback = d.Item1 as Action<T>;
                var reader = new ExtenderBinaryReader(b);
                var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
                if (TryDecompress(ref reader, ref writer))
                {
                    writer.Commit();
                    reader = writer;
                }

                T? result = Deserialize<T>(ref reader);
                writer.Dispose();

                var reslut = Deserialize<T>(b);
                callback?.Invoke(reslut);
            });
            operate.ReadAsync(position, length, action);
        }

        #endregion

        #region Write

        public override void Write<T>(ExpectLocalFileInfo info, T value)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(_binaryFileExtensions), value);
        }

        public override void Write<T>(FileOperateInfo info, T value)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWrite(GetOperate(info), value);
        }

        public override void Write<T>(IFileOperate fileOperate, T value)
        {
            PrivateWrite(fileOperate, value);
        }

        public override void Write<T>(ExpectLocalFileInfo info, T value, long position)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(_binaryFileExtensions), value, position);
        }

        public override void Write<T>(FileOperateInfo info, T value, long position)
        {
            if (!info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWrite(GetOperate(info), value, position);
        }

        public override void Write<T>(IFileOperate fileOperate, T value, long position)
        {
            PrivateWrite(fileOperate, value, position);
        }

        private void PrivateWrite<T>(IFileOperate operate, T value, long filePosition = 0)
        {
            var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
            Serialize(ref writer, value);
            operate.Write(writer, filePosition);
            writer.Dispose();
        }


        #endregion

        #region WriteAsync

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(_binaryFileExtensions), value, callback);
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            PrivateWriteAsync(GetOperate(info), value, 0, callback);
        }

        public override void WriteAsync<T>(IFileOperate fileOperate, T value, Action? callback = null)
        {
            PrivateWriteAsync(fileOperate, value, 0, callback);
        }

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, long position, Action? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(_binaryFileExtensions), value, position, callback);
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, long position, Action? callback = null)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            PrivateWriteAsync(GetOperate(info), value, position, callback);
        }

        public override void WriteAsync<T>(IFileOperate fileOperate, T value, long position, Action? callback = null)
        {
            PrivateWriteAsync(fileOperate, value, position, callback);
        }

        private void PrivateWriteAsync<T>(IFileOperate operate, T value, long filePosition = 0, Action? callback = null)
        {
            Task.Run(() =>
            {
                var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
                var formatter = _resolver.GetFormatterWithVerify<T>();
                formatter.Serialize(ref writer, value);
                operate.WriteAsync(writer, filePosition, callback);
            }).ConfigureAwait(false);
        }

        #endregion

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

            var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
            Serialize(ref writer, value);
            //writer.Flush();
            writer.Dispose();
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
            byte[] bytes = ArrayPool<byte>.Shared.Rent(MaxByteArrayLength);
            var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
            Serialize(ref writer, value);

            var result = writer.FlushAndGetArray();
            ArrayPool<byte>.Shared.Return(bytes);
            return result;
        }

        public void Serialize<T>(ref ExtenderBinaryWriter writer, T value)
        {
            try
            {
                _resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value);
                writer.Commit();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Serialize<T>(Stream stream, T value)
        {
            var rent = _sequencePool.Rent();
            var writer = new ExtenderBinaryWriter(rent.Value);
            Serialize(ref writer, value);
            writer.Commit();

            foreach (ReadOnlyMemory<byte> segment in rent.Value.AsReadOnlySequence)
            {
                var sharedBuffer = ArrayPool<byte>.Shared.Rent(segment.Length);
                segment.CopyTo(sharedBuffer);
                stream.Write(sharedBuffer, 0, sharedBuffer.Length);
            }
            rent.Dispose();
        }

        public async Task<byte[]> SerializeAsync<T>(T value, CancellationToken token)
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent(MaxByteArrayLength);

            var formatter = _resolver.GetFormatterWithVerify<T>();

            return await Task.Run(() =>
            {
                var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
                formatter.Serialize(ref writer, value);
                var result = writer.FlushAndGetArray();
                ArrayPool<byte>.Shared.Return(bytes);
                return result;
            }, token);
        }

        public Task SerializeAsync<T>(Stream stream, T value, CancellationToken token)
        {
            var rent = _sequencePool.Rent();
            var writer = new ExtenderBinaryWriter(rent.Value);
            Serialize(ref writer, value);
            writer.Commit();
            return Task.Run(() =>
            {
                foreach (ReadOnlyMemory<byte> segment in rent.Value.AsReadOnlySequence)
                {
                    var sharedBuffer = ArrayPool<byte>.Shared.Rent(segment.Length);
                    segment.CopyTo(sharedBuffer);
                    stream.Write(sharedBuffer, 0, sharedBuffer.Length);
                }
                rent.Dispose();
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
            length = GetLength(value);
            byte[] bytes = ArrayPool<byte>.Shared.Rent((int)length);
            var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
            Serialize(ref writer, value);

            writer.Dispose();
            return bytes;
        }

        public byte[] SerializeForArrayPool<T>(T value, out int length)
        {
            var bytes = SerializeForArrayPool(value, out long longLength);
            length = (int)longLength;
            return bytes;
        }

        #endregion

        #region Deserialize

        public T? Deserialize<T>(ReadOnlyMemory<byte> span)
        {
            if (span.IsEmpty)
                throw new ArgumentNullException(nameof(span));


            ExtenderBinaryReader reader = new ExtenderBinaryReader(span);
            return Deserialize<T>(ref reader);
        }

        /// <summary>
        /// 反序列化数据。
        /// </summary>
        /// <typeparam name="T">反序列化后的数据类型</typeparam>
        /// <param name="reader">用于读取数据的<see cref="ExtenderBinaryReader"/>对象</param>
        /// <returns>反序列化后的对象，如果剩余数据为0且没有默认格式化器，则返回默认值</returns>
        private T? Deserialize<T>(ref ExtenderBinaryReader reader)
        {
            if (reader.Remaining == 0)
            {
                var formatter = _resolver.GetFormatter<T>();
                if (formatter == null)
                    return default;

                return formatter.Default;
            }

            var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
            if (TryDecompress(ref reader, ref writer))
            {
                writer.Commit();
                reader = writer;
            }

            T? result = _resolver.GetFormatterWithVerify<T>().Deserialize(ref reader);
            writer.Dispose();
            return result;
        }

        public T? Deserialize<T>(Stream stream)
        {
            if (TryDeserializeFromMemoryStream<T>(stream, out var result))
            {
                return result;
            }

            var rent = _sequencePool.Rent();
            var sequence = rent.Value;
            int bytesRead = 0;
            do
            {
                Span<byte> span = sequence.GetSpan(stream.CanSeek ? (int)System.Math.Min(SuggestedContiguousMemorySize, stream.Length - stream.Position) : 0);
                bytesRead = stream.Read(span);
                sequence.Advance(bytesRead);
            }
            while (bytesRead > 0);

            result = DeserializeFromSequenceAndRewindStreamIfPossible<T>(stream, sequence);

            rent.Dispose();
            return result;
        }

        public async Task<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> span, CancellationToken token)
        {
            if (span.IsEmpty)
                throw new ArgumentNullException(nameof(span));


            return await Task.Run(() =>
            {
                ExtenderBinaryReader reader = new ExtenderBinaryReader(span);
                return Deserialize<T>(ref reader);
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

            var rent = _sequencePool.Rent();
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

            ExtenderBinaryReader reader = new ExtenderBinaryReader(sequence);

            T? result = Deserialize<T>(ref reader);

            //读取完成后返回到未读取的第一个字节
            if (stream.CanSeek && !reader.End)
            {
                long bytesNotRead = reader.Sequence.Slice(reader.Position).Length;
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
                var buffer = streamBuffer.AsMemory(checked((int)ms.Position));
                var reader = new ExtenderBinaryReader(buffer);

                result = Deserialize<T>(ref reader);

                var bytesRead = buffer.Slice(0, (int)reader.Consumed).Length;
                ms.Seek(bytesRead, SeekOrigin.Current);
                return true;
            }

            result = default;
            return false;
        }

        #endregion

        #region Delete

        public override void Delete(ExpectLocalFileInfo info)
        {
            var binaryFileInfo = info.CreatLocalFileInfo(FileExtensions.BinaryFileExtensions);
            Delete(binaryFileInfo);
        }

        #endregion

        #region Count

        public long GetLength<T>(T value)
        {
            return _resolver.GetFormatterWithVerify<T>().GetLength(value);
        }

        public long GetDefaulLength<T>()
        {
            return _resolver.GetFormatterWithVerify<T>().Length;
        }

        #endregion

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
            var scratchWriter = new ExtenderBinaryWriter(_sequencePool.Rent());
            Serialize(ref scratchWriter, value);


            var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
            ToLz4(scratchWriter.Rental.Value, ref writer, compression);

            byte[] result = writer.FlushAndGetArray();

            scratchWriter.Dispose();
            writer.Dispose();
            return result;
        }

        public byte[] Compression(ReadOnlySpan<byte> input, CompressionType compression)
        {
            var scratchWriter = new ExtenderBinaryWriter(_sequencePool.Rent());
            scratchWriter.Write(input);
            var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
            ToLz4(scratchWriter.Rental.Value, ref writer, compression);
            byte[] result = writer.FlushAndGetArray();
            scratchWriter.Dispose();
            writer.Dispose();
            return result;
        }

        public byte[] Compression(in ReadOnlySequence<byte> readOnlyMemories, CompressionType compression)
        {
            ExtenderBinaryWriter writer = new ExtenderBinaryWriter(_sequencePool.Rent());
            ToLz4(readOnlyMemories, ref writer, CompressionType.Lz4BlockArray);
            var result = writer.FlushAndGetArray();
            writer.Dispose();
            return result;
        }

        public void Write<T>(ExpectLocalFileInfo info, T value, CompressionType compression)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            Write(info.CreateReadWriteOperate(_binaryFileExtensions), value, compression);
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

            var scratchWriter = new ExtenderBinaryWriter(_sequencePool.Rent());
            Serialize(ref scratchWriter, value);


            var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
            ToLz4(scratchWriter.Rental.Value, ref writer, compression);
            writer.Commit();

            fileOperate.Write(writer, 0);

            scratchWriter.Dispose();
            writer.Dispose();
        }

        public void ToLz4(in ReadOnlySequence<byte> readOnlyMemories, ref ExtenderBinaryWriter writer, CompressionType compression)
        {
            if (readOnlyMemories.Length < CompressionMinLength || compression == CompressionType.None)
            {
                writer.Write(readOnlyMemories);
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
                        _resolver.GetFormatterWithVerify<ExtensionHeader>().Serialize(ref writer, new ExtensionHeader(Lz4Block, (uint)lz4Length));
                        _resolver.GetFormatterWithVerify<int>().Serialize(ref writer, (int)readOnlyMemories.Length);
                        // 将LZ4压缩后的数据写入
                        writer.Write(lz4Bytes.AsSpan(0, lz4Length));
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

                    _writerConvert.WriteArrayHeader(ref writer, sequenceCount + 1);
                    Serialize(ref writer, new ExtensionHeader(Lz4BlockArray, (uint)extHeaderSize));
                    IBinaryFormatter<int> intFormatter = _resolver.GetFormatterWithVerify<int>();
                    foreach (var item in readOnlyMemories)
                    {
                        intFormatter.Serialize(ref writer, item.Length);
                    }

                    foreach (var item in readOnlyMemories)
                    {
                        maxCompressedLength = LZ4Codec.MaximumOutputLength(item.Length);
                        var lz4Span = writer.GetSpan(maxCompressedLength + 5);
                        int lz4Length = LZ4Codec.Encode(item.Span, lz4Span.Slice(5, lz4Span.Length - 5));
                        WriteBin32Header((uint)lz4Length, lz4Span);
                        writer.Advance(lz4Length + 5);
                    }

                    break;
            }
        }

        public void WriteAsync<T>(ExpectLocalFileInfo info, T value, CompressionType compression, Action? callback = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(info.CreateReadWriteOperate(_binaryFileExtensions), value, compression, callback);
        }

        public void WriteAsync<T>(FileOperateInfo info, T value, CompressionType compression, Action? callback = null)
        {
            if (info.IsEmpty || !info.IsWrite())
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }
            WriteAsync(GetOperate(info), value, compression, callback);
        }

        public void WriteAsync<T>(IFileOperate fileOperate, T value, CompressionType compression, Action? callback = null)
        {
            Task.Run(() =>
           {
               if (fileOperate == null)
               {
                   ErrorUtil.ArgumentNull(nameof(fileOperate));
                   return;
               }

               var scratchWriter = new ExtenderBinaryWriter(_sequencePool.Rent());
               Serialize(ref scratchWriter, value);


               var writer = new ExtenderBinaryWriter(_sequencePool.Rent());
               ToLz4(scratchWriter.Rental.Value, ref writer, compression);
               writer.Commit();

               fileOperate.WriteAsync(writer, 0, () =>
               {
                   callback?.Invoke();
               });
               scratchWriter.Dispose();
           }).ConfigureAwait(false);
        }

        private bool TryDecompress(ref ExtenderBinaryReader reader, ref ExtenderBinaryWriter writer)
        {
            if (reader.End)
                return false;

            // Try to find LZ4Block
            var header = Deserialize<ExtensionHeader>(ref reader);

            if (!header.IsEmpty && header.TypeCode == Lz4Block)
            {
                var extReader = reader.CreatePeekReader();

                //这个整数表示数据在解压缩之后的长度。这样的设计允许接收方在解压数据之前就知道最终数据的大小，
                //这对于数据处理的效率和正确性至关重要。
                int uncompressedLength = Deserialize<int>(ref extReader);

                //接下来开始解压
                ReadOnlySequence<byte> compressedData = extReader.Sequence.Slice(extReader.Position);

                Span<byte> uncompressedSpan = writer.GetSpan(uncompressedLength).Slice(0, uncompressedLength);
                int actualUncompressedLength = LZ4Operation(compressedData, uncompressedSpan, LZ4CodecDecode);
                writer.Advance(actualUncompressedLength);
                return true;
            }


            var peekReader = reader.CreatePeekReader();
            var arrayLength = _readerConvert.ReadArrayHeader(ref peekReader);
            if (arrayLength == 0) return false;
            header = Deserialize<ExtensionHeader>(ref reader);
            if (header.TypeCode != Lz4BlockArray)
                return false;

            // 切换成原读取器
            reader = peekReader;

            // 开始读取 [Ext(98:int,int...), bin,bin,bin...]
            var sequenceCount = arrayLength - 1;
            var uncompressedLengths = ArrayPool<int>.Shared.Rent(sequenceCount);
            try
            {
                IBinaryFormatter<int> intFormatter = _resolver.GetFormatterWithVerify<int>();
                for (int i = 0; i < sequenceCount; i++)
                {
                    uncompressedLengths[i] = intFormatter.Deserialize(ref reader);
                }

                for (int i = 0; i < sequenceCount; i++)
                {
                    var uncompressedLength = uncompressedLengths[i];
                    ReadOnlySequence<byte> lz4Block = _readerConvert.ReadBytes(ref reader);
                    Span<byte> uncompressedSpan = writer.GetSpan(uncompressedLength);
                    var actualUncompressedLength = LZ4Operation(lz4Block, uncompressedSpan, LZ4CodecDecode);
                    writer.Advance(actualUncompressedLength);
                }
                return true;
            }
            finally
            {
                ArrayPool<int>.Shared.Return(uncompressedLengths);
            }
            return false;
        }

        private void WriteBin32Header(uint value, Span<byte> span)
        {
            unchecked
            {
                span[0] = _binaryOptions.BinaryCode.Bin32;

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
            if (value <= _binaryOptions.BinaryRang.MaxFixPositiveInt)
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

        #endregion
    }
}
