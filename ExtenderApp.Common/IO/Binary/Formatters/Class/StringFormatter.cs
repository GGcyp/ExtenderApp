using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters
{
    /// <summary>
    /// 字符串格式化器类
    /// </summary>
    /// <remarks>
    /// 继承自<see cref="BinaryFormatter{T}"/>泛型类，专门用于对字符串类型的对象进行格式化。
    /// </remarks>
    internal class StringFormatter : BinaryFormatter<string>
    {
        public StringFormatter(ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
        }

        public override int DefaultLength => 5;

        public override long GetLength(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return DefaultLength;
            }
            return _binaryOptions.BinaryEncoding.GetMaxByteCount(value.Length) + DefaultLength;
            //return value.Length + Length;
        }

        public override string Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadNil(ref buffer))
            {
                return string.Empty;
            }

            if (!_bufferConvert.TryReadStringSpan(ref buffer, out ReadOnlySpan<byte> bytes))
                return string.Empty;

            return _bufferConvert.UTF8ToString(bytes);
        }

        public override void Serialize(ref ByteBuffer buffer, string value)
        {
            _bufferConvert.Write(ref buffer, value);
        }
    }
}
