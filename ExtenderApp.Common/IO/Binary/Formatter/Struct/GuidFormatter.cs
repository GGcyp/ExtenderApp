using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatter
{
    internal class GuidFormatter : ResolverFormatter<Guid>
    {
        private readonly IBinaryFormatter<string> _formatter;

        public override int Count => throw new NotImplementedException();

        public GuidFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = GetFormatter<string>();
        }

        public override Guid Deserialize(ref ExtenderBinaryReader reader)
        {
            //_binaryReaderConvert.TryReadStringSpan(ref reader, out ReadOnlySpan<byte> bytes);
            //return Guid.Parse(_binaryReaderConvert.UTF8ToString(bytes));

            var result = _formatter.Deserialize(ref reader);
            return Guid.Parse(result);
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

            //_binaryWriterConvert.Write(ref writer, value.ToString());

            _formatter.Serialize(ref writer, value.ToString());
        }

        public override int GetCount(Guid value)
        {
            return _formatter.GetCount(value.ToString());
        }
    }
}
