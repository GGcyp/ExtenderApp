using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.NetWorks
{
    internal class LinkerDtoFormatter : ResolverFormatter<LinkerDto>
    {
        private readonly IBinaryFormatter<bool> _bool;

        public override int Length => _bool.Length;

        public LinkerDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bool = GetFormatter<bool>();
        }

        public override LinkerDto Deserialize(ref ExtenderBinaryReader reader)
        {
            var result = new LinkerDto();

            result.NeedHeartbeat = _bool.Deserialize(ref reader);

            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, LinkerDto value)
        {
            _bool.Serialize(ref writer, value.NeedHeartbeat);
        }
    }
}
