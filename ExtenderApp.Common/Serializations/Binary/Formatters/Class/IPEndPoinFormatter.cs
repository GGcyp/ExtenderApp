using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Serializations.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters.Class
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

        public override IPEndPoint Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return null;
            }

            var address = _address.Deserialize(ref buffer);
            var port = _int.Deserialize(ref buffer);
            return new IPEndPoint(address, port);
        }

        public override void Serialize(ref ByteBuffer buffer, IPEndPoint value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }

            _address.Serialize(ref buffer, value.Address);
            _int.Serialize(ref buffer, value.Port);
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
