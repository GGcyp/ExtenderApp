using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters.Class
{
    /// <summary>
    /// IPEndPoin 二进制格式化器
    /// </summary>
    internal class IPEndPoinFormatter : ResolverFormatter<IPEndPoint>
    {
        private readonly IBinaryFormatter<IPAddress> _address;
        private readonly IBinaryFormatter<int> _int;

        public IPEndPoinFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _address = resolver.GetFormatter<IPAddress>();
            _int = resolver.GetFormatter<int>();
        }

        public override IPEndPoint Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return null!;
            }

            var address = _address.Deserialize(reader);
            var port = _int.Deserialize(reader);
            return new IPEndPoint(address, port);
        }

        public override IPEndPoint Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return null!;
            }

            var address = _address.Deserialize(ref reader);
            var port = _int.Deserialize(ref reader);
            return new IPEndPoint(address, port);
        }

        public override void Serialize(AbstractBuffer<byte> buffer, IPEndPoint value)
        {
            if (value == null)
            {
                WriteNil(buffer);
                return;
            }

            _address.Serialize(buffer, value.Address);
            _int.Serialize(buffer, value.Port);
        }

        public override void Serialize(ref SpanWriter<byte> writer, IPEndPoint value)
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
                return NilLength;
            }

            return _int.DefaultLength + _address.GetLength(value.Address);
        }
    }
}
