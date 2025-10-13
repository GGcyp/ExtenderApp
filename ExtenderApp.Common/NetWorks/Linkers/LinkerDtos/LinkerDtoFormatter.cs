using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class LinkerDtoFormatter : ResolverFormatter<LinkerDto>
    {
        private readonly IBinaryFormatter<bool> _bool;

        public override int DefaultLength => _bool.DefaultLength;

        public LinkerDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bool = GetFormatter<bool>();
        }

        public override LinkerDto Deserialize(ref ByteBuffer buffer)
        {
            var result = new LinkerDto();

            result.NeedHeartbeat = _bool.Deserialize(ref buffer);

            return result;
        }

        public override void Serialize(ref ByteBuffer buffer, LinkerDto value)
        {
            _bool.Serialize(ref buffer, value.NeedHeartbeat);
        }

        public override long GetLength(LinkerDto value)
        {
            return _bool.GetLength(value.NeedHeartbeat);
        }
    }
}
