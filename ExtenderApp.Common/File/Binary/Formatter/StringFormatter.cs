using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    internal class StringFormatter : ExtenderFormatter<string>
    {
        public StringFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
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
