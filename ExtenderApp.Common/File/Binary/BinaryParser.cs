using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Data.File;


namespace ExtenderApp.Common.File
{
    public class BinaryParser : IBinaryParser
    {
        private const int MaxByteArrayLenght = 65536;

        private readonly IBinaryFormatterResolver _binaryFormatterResolver;
        private readonly SequencePool<byte> _sequencePool;
        private readonly ArrayPool<byte> _arrayPool;

        public BinaryParser(IBinaryFormatterResolver binaryFormatterResolver)
        {
            _binaryFormatterResolver = binaryFormatterResolver;
            _arrayPool = ArrayPool<byte>.Create(MaxByteArrayLenght, 100);
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
                _binaryFormatterResolver.GetFormatterWithVerify<T>().Serialize(ref writer, value);
            }
            catch (Exception ex)
            {
                //throw new MessagePackSerializationException($"Failed to serialize {typeof(T).FullName} value.", ex);
                throw;
            }
        }

        public bool Serialize<T>(FileOperate operate, T value, object? options = null)
        {
            //暂时先这么写
            byte[] bytes = Serialize(value);
            using (FileStream stream = operate.OpenFile())
            {
                stream.Write(bytes);
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
            return Deserialize<T>(reader);
        }

        /// <summary>
        /// 反序列化数据。
        /// </summary>
        /// <typeparam name="T">反序列化后的数据类型</typeparam>
        /// <param name="reader">用于读取数据的<see cref="ExtenderBinaryReader"/>对象</param>
        /// <returns>反序列化后的对象，如果剩余数据为0且没有默认格式化器，则返回默认值</returns>
        private T? Deserialize<T>(ExtenderBinaryReader reader)
        {
            if (reader.Remaining == 0)
            {
                var formatter = _binaryFormatterResolver.GetFormatter<T>();
                if (formatter == null) 
                    return default;

                return formatter.Default;
            }

            return _binaryFormatterResolver.GetFormatterWithVerify<T>().Deserialize(ref reader);
        }

        public T? Deserialize<T>(FileOperate operate, object? options = null)
        {
            //暂时先这么写
            byte[] bytes = _arrayPool.Rent(MaxByteArrayLenght);
            int length = -1;

            if (!operate.LocalFileInfo.FileInfo.Exists)
                return Deserialize<T>(ExtenderBinaryReader.Empty);

            using (FileStream stream = operate.OpenFile())
            {
                length = stream.Read(bytes);
            }

            ExtenderBinaryReader reader = new ExtenderBinaryReader(new ReadOnlyMemory<byte>(bytes, 0, length));

            return Deserialize<T>(reader);
        }

        #endregion
    }
}
