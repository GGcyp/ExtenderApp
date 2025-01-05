using ExtenderApp.Data;

namespace ExtenderApp.Common.File.Binary.Formatter
{
    internal class GuidFormatter : ExtenderFormatter<Guid>
    {
        public GuidFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override Guid Deserialize(ref ExtenderBinaryReader reader)
        {
            _binaryReaderConvert.TryReadStringSpan(ref reader, out ReadOnlySpan<byte> bytes);
            return Guid.Parse(_binaryReaderConvert.UTF8ToString(bytes));
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, Guid value)
        {
            //byte* pBytes = stackalloc byte[36];
            //Span<byte> bytes = new Span<byte>(pBytes, 36);
            //new GuidBits(ref value).Write(bytes);
            //writer.WriteString(bytes);

            //writer.Advance(16);

            //var span = writer.GetSpan(16);
            //value.TryWriteBytes(span);
            //_binaryWriterConvert.WriteRaw(ref writer, span);
            //writer.Advance(16);
            _binaryWriterConvert.Write(ref writer, value.ToString());
        }
    }
}
