using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters.Class
{
    internal class IPEndPoinFormatter : ResolverFormatter<IPEndPoint>
    {
        public override int DefaultLength => 1;

        private readonly IBinaryFormatter<IPAddress> _address;
        private readonly IBinaryFormatter<int> _int;

        public IPEndPoinFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _address = resolver.GetFormatter<IPAddress>();
            _int = resolver.GetFormatter<int>();
        }

        public override IPEndPoint Deserialize(ref ExtenderBinaryReader reader)
        {
            if (TryReadNil(ref reader))
            {
                return null;
            }

            var address = _address.Deserialize(ref reader);
            var port = _int.Deserialize(ref reader);
            return new IPEndPoint(address, port);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, IPEndPoint value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            _address.Serialize(ref writer, value.Address);
            _int.Serialize(ref writer, value.Port);
        }

        public override long GetLength(IPEndPoint value)
        {
            if (value == null)
            {
                return 1;
            }

            return _int.DefaultLength + _address.GetLength(value.Address);
        }
    }
}
