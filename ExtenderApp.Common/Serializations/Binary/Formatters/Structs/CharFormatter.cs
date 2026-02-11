using ExtenderApp.Abstract;
using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class CharFormatter : ResolverFormatter<Char>
    {
        private readonly IBinaryFormatter<UInt16> _uint16;

        public override int DefaultLength => _uint16.DefaultLength;

        public CharFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _uint16 = GetFormatter<UInt16>();
        }

        public override void Serialize(AbstractBuffer<byte> buffer, char value)
        {
            _uint16.Serialize(buffer, (UInt16)value);
        }

        public override void Serialize(ref SpanWriter<byte> writer, char value)
        {
            _uint16.Serialize(ref writer, (UInt16)value);
        }

        public override char Deserialize(AbstractBufferReader<byte> reader)
        {
            return (char)_uint16.Deserialize(reader);
        }

        public override char Deserialize(ref SpanReader<byte> reader)
        {
            return (char)_uint16.Deserialize(ref reader);
        }

        public override long GetLength(char value)
        {
            return _uint16.GetLength((UInt16)value);
        }
    }
}