using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Data.File;


namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 二进制解析器类
    /// </summary>
    public class BinaryParser : IBinaryParser
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

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver)
        {
            _resolver = binaryFormatterResolver;
            _arrayPool = ArrayPool<byte>.Shared;
            _sequencePool = new(Environment.ProcessorCount * 2, _arrayPool);
        }

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

        public bool Serialize<T>(FileOperateInfo operate, T value, object? options = null)
        {
            using (FileStream stream = operate.OpenFile())
            {
                Serialize(stream, value, options);
            }
            return true;
        }

        public bool Serialize<T>(Stream stream, T value, object? options = null)
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
            return true;
        }

        public bool Serialize<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            return Serialize(info.CreateFileOperate(FileExtensions.BinaryFileExtensions, FileMode.OpenOrCreate, FileAccess.ReadWrite), value, options);
        }

        public async ValueTask<bool> SerializeAsync<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            var fileOperate = info.CreateFileOperate(FileExtensions.BinaryFileExtensions, FileMode.OpenOrCreate, FileAccess.ReadWrite);


            byte[] bytes = Serialize(value);

            using (FileStream stream = fileOperate.OpenFile())
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            return true;
        }

        public async ValueTask<bool> SerializeAsync<T>(FileOperateInfo operate, T value, object? options = null)
        {
            byte[] bytes = Serialize(value);

            using (FileStream stream = operate.OpenFile())
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            return true;
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

        public T? Deserialize<T>(FileOperateInfo operate, object? options = null)
        {
            if (!operate.LocalFileInfo.Exists)
            {
                //throw new InvalidOperationException(string.Format("文件不存在: {0}", operate.LocalFileInfo.FilePath));
                var formatter = _resolver.GetFormatter<T>();
                if (formatter == null)
                    return default;

                return formatter.Default;
            }

            var stream = operate.OpenFile();
            T? result = Deserialize<T>(stream, options);
            stream.Dispose();
            return result;
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

        public T? Deserialize<T>(ExpectLocalFileInfo info, object? options = null)
        {
            return Deserialize<T>(info.CreateFileOperate(FileExtensions.BinaryFileExtensions), options);
        }

        public async ValueTask<T?> DeserializeAsync<T>(ExpectLocalFileInfo info, object? options = null)
        {
            var operate = info.CreateFileOperate(FileExtensions.BinaryFileExtensions);

            var stream = operate.OpenFile();
            T? result = await DeserializeAsync<T>(stream);
            stream.Dispose();

            return result;
        }

        public async ValueTask<T?> DeserializeAsync<T>(FileOperateInfo operate, object? options = null)
        {
            T? result = await DeserializeAsync<T>(operate.OpenFile());

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
