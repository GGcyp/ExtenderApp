
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.NetWorks
{
    internal class PacketSegmentHeadFromatter : ResolverFormatter<PacketSegmentHead>
    {
        private readonly IBinaryFormatter<int> _int;

        public override int Length => _int.Length * 3;

        public PacketSegmentHeadFromatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override PacketSegmentHead Deserialize(ref ExtenderBinaryReader reader)
        {
            int length = _int.Deserialize(ref reader);
            int typeCode = _int.Deserialize(ref reader);
            int count = _int.Deserialize(ref reader);
            return new PacketSegmentHead(length, typeCode, count);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, PacketSegmentHead value)
        {
            _int.Serialize(ref writer, value.Length);
            _int.Serialize(ref writer, value.TypeCode);
            _int.Serialize(ref writer, value.Count);
        }
    }
}
