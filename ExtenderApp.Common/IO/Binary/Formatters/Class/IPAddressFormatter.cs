using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters.Class
{
    public class IPAddressFormatter : ResolverFormatter<IPAddress>
    {
        public override int DefaultLength => 1;

        private readonly IBinaryFormatter<byte[]> _bytes;

        public IPAddressFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bytes = resolver.GetFormatter<byte[]>();
        }

        public override IPAddress Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return null;
            }

            var bytes = _bytes.Deserialize(ref buffer);
            return new IPAddress(bytes);
        }

        public override void Serialize(ref ByteBuffer buffer, IPAddress value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
                return;
            }

            _bytes.Serialize(ref buffer, value.GetAddressBytes());
        }

        public override long GetLength(IPAddress value)
        {
            if (value == null)
            {
                return 1;
            }

            return _bytes.GetLength(value.GetAddressBytes());
        }
    }
}
