﻿using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Common.ObjectPools;
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
        private const int MaxByteArrayLenght = 65536;

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

        /// <summary>
        /// 字节数组池
        /// </summary>
        private readonly ArrayPool<byte> _arrayPool;

        private readonly ObjectPool<WriteOperation> _writeOperationPool;
        private readonly ObjectPool<BinaryReadOperation> _readOperationPool;

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver, FileStore store) : base(store)
        {
            _resolver = binaryFormatterResolver;
            _arrayPool = ArrayPool<byte>.Shared;
            _sequencePool = new(Environment.ProcessorCount * 2, _arrayPool);
            _writeOperationPool = ObjectPool.Create(new ConcurrentOperationPoolPolicy<WriteOperation>(o => new WriteOperation(o)));
            _readOperationPool = ObjectPool.Create(new ConcurrentOperationPoolPolicy<BinaryReadOperation>(o => new BinaryReadOperation(o)));
        }

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
            byte[] bytes = Serialize(value);

            var operate = GetOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();
            operation.Set(bytes, null);
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
            byte[] bytes = Serialize(value);

            var operate = GetOperate(info, fileOperate);
            var operation = _writeOperationPool.Get();
            operation.Set(bytes, b =>
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

            return Read<T>(info.CreateFileOperate(FileExtensions.BinaryFileExtensions), fileOperate, options);
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
            operation.Set(s =>
            {
                var dataBuffer = DataBuffer<T>.GetDataBuffer();
                dataBuffer.Item = Deserialize<T>(s);
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

            operation.Set((s, d) =>
            {
                var dataBuffer = d as DataBuffer<Delegate>;
                T result = Deserialize<T>(s);
                var callback = dataBuffer.Item as Action<T>;
                callback?.Invoke(result);
                dataBuffer.Release();
            });

            operate.QueueOperation(operation);
        }

        #endregion

        #region Serialize

        public byte[] Serialize<T>(T value)
        {
            byte[] bytes = _arrayPool.Rent(MaxByteArrayLenght);
            var writer = new ExtenderBinaryWriter(_sequencePool, bytes);
            Serialize(ref writer, value);

            var result = writer.FlushAndGetArray();
            _arrayPool.Return(bytes);
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

        #region Count

        public int GetCount<T>(T value)
        {
            return _resolver.GetFormatterWithVerify<T>().GetCount(value);
        }

        #endregion
    }
}
