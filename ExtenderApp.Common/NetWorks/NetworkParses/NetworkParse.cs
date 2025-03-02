using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.NetWorks
{
    public abstract class NetworkParse<T> : INetWorkParse<T>
    {
        public int Code { get; }
        protected readonly IBinaryParser _binaryParser;

        protected NetworkParse(IBinaryParser binaryParser)
        {
            Code = typeof(T).FullName.GetHashCode();
            _binaryParser = binaryParser;
        }

        public abstract NetworkPacket Parse(T value);

        public abstract void Parse(NetworkPacket packet);

        protected NetworkPacket GetPacket(byte[] bytes)
        {
            return new NetworkPacket(Code, bytes);
        }

        protected byte[] Serialize(T value)
        {
            return _binaryParser.Serialize(value);
        }

        protected T? Deserialize(byte[] bytes)
        {
            return _binaryParser.Deserialize<T>(bytes);
        }
    }
}
