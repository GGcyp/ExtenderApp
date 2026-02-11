using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters.Class
{
    /// <summary>
    /// IPAddress 的二进制格式化器。
    /// </summary>
    public class IPAddressFormatter : ResolverFormatter<IPAddress>
    {
        public override int DefaultLength => 1;

        private readonly IBinaryFormatter<byte[]> _bytes;

        public IPAddressFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _bytes = resolver.GetFormatter<byte[]>();
        }

        public override IPAddress Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return null!;
            }

            var bytes = _bytes.Deserialize(reader);
            return new IPAddress(bytes);
        }

        public override IPAddress Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var bytes = _bytes.Deserialize(ref reader);
            return new IPAddress(bytes);
        }

        public override void Serialize(AbstractBuffer<byte> buffer, IPAddress value)
        {
            if (value == null)
            {
                WriteNil(buffer);
                return;
            }

            _bytes.Serialize(buffer, value.GetAddressBytes());
        }

        public override void Serialize(ref SpanWriter<byte> writer, IPAddress value)
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
                return NilLength;
            }

            return _bytes.GetLength(value.GetAddressBytes());
        }
    }
}