using ExtenderApp.Abstract;
using ExtenderApp.Common.DataBuffers;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 表示一个用于序列化和反序列化 <see cref="FileNode{T}"/> 对象的格式化器。
    /// </summary>
    /// <typeparam name="T">表示 <see cref="FileNode{T}"/> 的具体类型。</typeparam>
    public class FileNodeFormatter<T> : NodeFormatter<T> where T : FileNode<T>, new()
    {
        /// <summary>
        /// 表示用于序列化和反序列化布尔值的格式化器。
        /// </summary>
        protected readonly IBinaryFormatter<bool> _bool;

        /// <summary>
        /// 表示用于序列化和反序列化长整型的格式化器。
        /// </summary>
        protected readonly IBinaryFormatter<long> _long;

        /// <summary>
        /// 表示用于序列化和反序列化字符串的格式化器。
        /// </summary>
        protected readonly IBinaryFormatter<string> _string;

        /// <summary>
        /// 初始化 <see cref="FileNodeFormatter{T}"/> 类的新实例。
        /// </summary>
        /// <param name="resolver">用于解析格式化器的解析器。</param>
        /// <param name="binaryWriterConvert">用于二进制写入的转换器。</param>
        /// <param name="binaryReaderConvert">用于二进制读取的转换器。</param>
        /// <param name="options">二进制选项。</param>
        public FileNodeFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(resolver, binaryWriterConvert, binaryReaderConvert, options)
        {
            _bool = resolver.GetFormatter<bool>();
            _long = resolver.GetFormatter<long>();
            _string = resolver.GetFormatter<string>();
        }

        /// <summary>
        /// 反序列化二进制数据为 <see cref="T"/> 类型的对象。
        /// </summary>
        /// <param name="reader">用于读取二进制数据的读取器。</param>
        /// <returns>反序列化后的 <see cref="T"/> 对象。</returns>
        protected override T ProtectedDeserialize(ref ExtenderBinaryReader reader)
        {
            T node = new();

            node.IsFile = _bool.Deserialize(ref reader);
            node.Length = _long.Deserialize(ref reader);
            node.Name = _string.Deserialize(ref reader);

            return node;
        }

        /// <summary>
        /// 序列化 <see cref="T"/> 对象为二进制数据。
        /// </summary>
        /// <param name="writer">用于写入二进制数据的写入器。</param>
        /// <param name="value"><see cref="T"/> 对象。</param>
        protected override void ProtectedSerialize(ref ExtenderBinaryWriter writer, T value)
        {
            _bool.Serialize(ref writer, value.IsFile);
            _long.Serialize(ref writer, value.Length);
            _string.Serialize(ref writer, value.Name);
        }

        /// <summary>
        /// 计算 <see cref="T"/> 对象的二进制长度。
        /// </summary>
        /// <param name="value"><see cref="T"/> 对象。</param>
        /// <param name="length">二进制长度。</param>
        protected override void ProtectedGetLength(T value, DataBuffer<long> dataBuffer)
        {
            if (value == null)
            {
                dataBuffer.Item1 += 1;
                return;
            }
            dataBuffer.Item1 += _bool.Length + _long.Length + _string.GetLength(value.Name);
        }
    }
}
