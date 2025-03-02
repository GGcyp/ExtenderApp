using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.NetWorks
{
    internal class SplitterInfoParse : NetworkParse<SplitterInfo>
    {
        public SplitterInfoParse(IBinaryParser binaryParser) : base(binaryParser)
        {
        }

        public override NetworkPacket Parse(SplitterInfo value)
        {
            return GetPacket(_binaryParser.Serialize(value));
        }

        public override void Parse(NetworkPacket packet)
        {
            throw new NotImplementedException();
        }
    }
}
