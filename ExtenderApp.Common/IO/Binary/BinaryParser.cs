using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ObjectPools.Policy;
using ExtenderApp.Data;
using ExtenderApp.Data.File;


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
        /// 二进制格式化器解析器
        /// </summary>
        private readonly IBinaryFormatterResolver _resolver;

        /// <summary>
        /// 字节序列池
        /// </summary>
        private readonly SequencePool<byte> _sequencePool;

        private readonly ObjectPool<WriteOperation> _writeOperationPool;
        private readonly ObjectPool<BinaryReadOperation> _readOperationPool;

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver, FileStore store) : base(store)
        {
            _resolver = binaryFormatterResolver;
            _sequencePool = new(Environment.ProcessorCount * 2, ArrayPool<byte>.Shared);
            _writeOperationPool = ObjectPool.Create(new SelfResetPooledObjectPolicy<WriteOperation>());
            _readOperationPool = ObjectPool.Create(new SelfResetPooledObjectPolicy<BinaryReadOperation>());
        }

        #region Get

        public IBinaryFormatter<T> GetFormatter<T>()
        {
            return _resolver.GetFormatterWithVerify<T>();
        }

        #endregion

        #region Write

        public override void Write<T>(ExpectLocalFileInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            Write(info.CreateFileOperate(FileExtensions.BinaryFileExtensions, FileMode.OpenOrCreate, FileAccess.ReadWrite), value, fileOperate, options);
        }

        public override void Write<T>(FileOperateInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null)
        {
            byte[] bytes = SerializeForArrayPool(value, out long length);

            var operate = GetOperate(info, fileOperate, length);
            var operation = _writeOperationPool.Get();
            operation.Set(bytes, length, null);
            operate.ExecuteOperation(operation);
            operation.Release();
            ArrayPool<byte>.Shared.Return(bytes);
        }

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            var fileOperateInfo = info.CreateFileOperate(FileExtensions.BinaryFileExtensions, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            WriteAsync(fileOperateInfo, value, callback, fileOperate, options);
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null)
        {
            byte[] bytes = SerializeForArrayPool(value, out long length);

            var operate = GetOperate(info, fileOperate, length);
            var operation = _writeOperationPool.Get();
            operation.Set(bytes, length, b =>
            {
                callback?.Invoke();
                ArrayPool<byte>.Shared.Return(b);
            });
            operate.QueueOperation(operation);
        }

        #endregion

        #region Read

        public override T? Read<T>(ExpectLocalFileInfo info, IConcurrentOperate fileOperate = null, object? options = null) where T : default
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read<T>(info.CreateWriteOperate(FileExtensions.BinaryFileExtensions), fileOperate, options);
        }

        public override T? Read<T>(FileOperateInfo info, IConcurrentOperate fileOperate = null, object? options = null) where T : default
        {
            if (!info.LocalFileInfo.Exists)
            {
                var formatter = _resolver.GetFormatter<T>();
                if (formatter == null)
                    return default;

                return formatter.Default;
            }

            var operate = GetOperate(info, fileOperate);
            var operation = _readOperationPool.Get();
            operation.Set((m, p, l) =>
            {
                var dataBuffer = DataBuffer<T>.GetDataBuffer();
                var bytes = ArrayPool<byte>.Shared.Rent((int)l);

                for (long i = p; i < l; i++)
                {
                    bytes[i] = m.ReadByte(i);
                }

                dataBuffer.Item = Deserialize<T>(bytes);
                ArrayPool<byte>.Shared.Return(bytes);
                return dataBuffer;
            });

            operate.ExecuteOperation(operation);
            var dataBuffer = operation.Data as DataBuffer<T>;
            var result = dataBuffer.Item;
            dataBuffer.Release();
            operation.Release();
            return result;
            //var stream = operate.OpenFile();
            //T? result = Deserialize<T>(stream, options);
            //stream.Dispose();
            //return result;
        }

        public override void ReadAsync<T>(ExpectLocalFileInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            var operate = info.CreateFileOperate(FileExtensions.BinaryFileExtensions);
            ReadAsync(operate, callback, fileOperate, options);
            //var stream = operate.OpenFile();
            //T? result = await DeserializeAsync<T>(stream);
            //stream.Dispose();
            //callback?.Invoke(result);
        }

        public override void ReadAsync<T>(FileOperateInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null)
        {
            //T? result = await DeserializeAsync<T>(operate.OpenFile());
            //callback?.Invoke(result);
            var operate = GetOperate(info, fileOperate);
            var operation = _readOperationPool.Get();
            var dataBuffer = DataBuffer<Delegate>.GetDataBuffer();
            operation.Data = dataBuffer;

            operation.Set((m, p, l, d) =>
            {
                var dataBuffer = d as DataBuffer<Delegate>;
                var bytes = ArrayPool<byte>.Shared.Rent((int)l);

                for (long i = p; i < l; i++)
                {
                    bytes[i] = m.ReadByte(i);
                }

                T result = Deserialize<T>(bytes);
                var callback = dataBuffer.Item as Action<T>;
                callback?.Invoke(result);
                ArrayPool<byte>.Shared.Return(bytes);
                dataBuffer.Release();
            });

            operate.QueueOperation(operation);
        }

        public byte[] Read(ExpectLocalFileInfo info, long position, long length, IConcurrentOperate? fileOperate = null, byte[]? bytes = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            return Read(info.CreateWriteOperate(FileExtensions.BinaryFileExtensions), position, length, fileOperate, bytes);
        }

        public byte[] Read(FileOperateInfo info, long position, long length, IConcurrentOperate? fileOperate = null, byte[]? bytes = null)
        {
            if (info.IsEmpty)
            {
                ErrorUtil.ArgumentNull(nameof(info));
            }

            var operate = GetOperate(info, fileOperate);
            var operation = _readOperationPool.Get();
            var buffer = DataBuffer<byte[]>.GetDataBuffer();

            if (bytes == null || bytes.Length < length)
            {
                //throw new ArgumentOutOfRangeException(nameof(bytes));
                bytes = new byte[length];
            }
            buffer.Item = bytes;

            operation.Set((m, p, l, d) =>
            {
                var buffer = d as DataBuffer<byte[]>;
                for (long i = p; i < l; i++)
                {
                    buffer.Item[i] = m.ReadByte(i);
                }
            }, position, length, buffer);
            operate.ExecuteOperation(operation);
            var result = buffer.Item;
            operation.Release();
            buffer.Release();
            return result;
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
            writer.Flush();
        }

        public void Serialize<T>(T value, byte[] bytes)
        {
            bytes.ArgumentNull(nameof(bytes));

            var result = SerializeForArrayPool(value, out long length);
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
            }
            catch (Exception ex)
            {
                //throw new MessagePackSerializationException($"Failed to serialize {typeof(T).FullName} value.", ex);
                throw;
            }
        }

        public void Serialize<T>(Stream stream, T value, object? options = null)
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

            writer.Flush();
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

        public T? Deserialize<T>(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));


            ExtenderBinaryReader reader = new ExtenderBinaryReader(bytes);
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

            return _resolver.GetFormatterWithVerify<T>().Deserialize(ref reader);
        }

        public T? Deserialize<T>(Stream stream, object? options = null)
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

        public async Task<T?> DeserializeAsync<T>(Stream stream, object? options = null)
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
            _store.Delete(binaryFileInfo);
            binaryFileInfo.Delete();
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
    }
}
