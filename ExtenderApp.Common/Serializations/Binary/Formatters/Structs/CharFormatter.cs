using ExtenderApp.Abstract;
using ExtenderApp.Data;

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

        public override char Deserialize(ref ByteBuffer buffer)
        {
            return (char)_uint16.Deserialize(ref buffer);
        }

        public override void Serialize(ref ByteBuffer buffer, char value)
        {
            _uint16.Serialize(ref buffer, (UInt16)value);
        }

        public override long GetLength(char value)
        {
            return _uint16.GetLength((UInt16)value);
        }
    }
}