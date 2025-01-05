using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    /// <summary>
    /// 静态可空格式化器类，用于处理可空的结构体类型的序列化和反序列化。
    /// </summary>
    /// <typeparam name="T">结构体类型</typeparam>
    public sealed class NullableFormatter<T> : ExtenderFormatter<T?> where T : struct
    {
        /// <summary>
        /// 底层格式化器，用于处理非可空结构体类型的序列化和反序列化。
        /// </summary>
        private readonly IBinaryFormatter<T> _formatter;

        /// <summary>
        /// 初始化 StaticNullableFormatter 类的新实例。
        /// </summary>
        /// <param name="underlyingFormatter">底层格式化器。</param>
        /// <param name="writerConvert">写入转换器。</param>
        /// <param name="readerConvert">读取转换器。</param>
        /// <param name="options">二进制选项。</param>
        public NullableFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert writerConvert, ExtenderBinaryReaderConvert readerConvert, BinaryOptions options) : base(writerConvert, readerConvert, options)
        {
            _formatter = resolver.GetFormatter<T>();
        }

        /// <summary>
        /// 从二进制读取器中反序列化出对象。
        /// </summary>
        /// <param name="reader">二进制读取器。</param>
        /// <returns>反序列化出的对象。</returns>
        public override T? Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader))
            {
                return null;
            }
            else
            {
                return _formatter.Deserialize(ref reader);
            }
        }

        /// <summary>
        /// 将对象序列化到二进制写入器中。
        /// </summary>
        /// <param name="writer">二进制写入器。</param>
        /// <param name="value">要序列化的对象。</param>
        public override void Serialize(ref ExtenderBinaryWriter writer, T? value)
        {
            if (value == null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
            }
            else
            {
                _formatter.Serialize(ref writer, value.Value);
            }
        }
    }
}
