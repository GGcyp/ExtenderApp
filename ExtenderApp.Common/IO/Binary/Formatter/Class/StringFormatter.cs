using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatter
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

        public override int Count => 5;

        public override int GetCount(string value)
        {
            var result = Count;
            if (string.IsNullOrEmpty(value))
            {
                return 1;
            }
            return _binaryOptions.UTF8.GetMaxByteCount(value.Length) + Count;
        }

        public override string Deserialize(ref ExtenderBinaryReader reader)
        {
            if (_binaryReaderConvert.TryReadNil(ref reader))
            {
                return string.Empty;
            }

            _binaryReaderConvert.TryReadStringSpan(ref reader, out ReadOnlySpan<byte> bytes);
            return _binaryReaderConvert.UTF8ToString(bytes);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, string value)
        {
            _binaryWriterConvert.Write(ref writer, value);
        }
    }
}
