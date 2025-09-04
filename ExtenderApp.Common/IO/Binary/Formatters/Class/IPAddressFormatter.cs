using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
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

        public override IPAddress Deserialize(ref ExtenderBinaryReader reader)
        {
            if (TryReadNil(ref reader))
            {
                return null;
            }

            var bytes = _bytes.Deserialize(ref reader);
            return new IPAddress(bytes);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, IPAddress value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            _bytes.Serialize(ref writer, value.GetAddressBytes());
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
