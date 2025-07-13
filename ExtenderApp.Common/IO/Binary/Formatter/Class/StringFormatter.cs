using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binaries.Formatter
{
    /// <summary>
    /// 字符串格式化器类
    /// </summary>
    /// <remarks>
    /// 继承自<see cref="ExtenderFormatter{T}"/>泛型类，专门用于对字符串类型的对象进行格式化。
    /// </remarks>
    internal class StringFormatter : ExtenderFormatter<string>
    {
        public StringFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override int Length => 5;

        public override long GetLength(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Length;
            }
            return _binaryOptions.BinaryEncoding.GetMaxByteCount(value.Length) + Length;
            //return value.Length + Length;
        }

        public override string Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader))
            {
                return string.Empty;
            }

            if (!_binaryReaderConvert.TryReadStringSpan(ref reader, out ReadOnlySpan<byte> bytes))
                return string.Empty;

            return _binaryReaderConvert.UTF8ToString(bytes);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, string value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }
    }
}
