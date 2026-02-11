using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters.Collection
{
    internal class ByteArrayFormatter : ResolverFormatter<byte[]>
    {
        private readonly IBinaryFormatter<int> _int;

        public ByteArrayFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override byte[] Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
            {
                return Array.Empty<byte>();
            }

            if (!TryReadArrayHeader(reader))
            {
                throw new Exception("无法反序列化数据，数据类型不匹配。");
            }

            var len = _int.Deserialize(reader);
            if (len == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] array = new byte[len];
            reader.Read(array.AsSpan());
            return array;
        }

        public override byte[] Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
            {
                return Array.Empty<byte>();
            }

            if (!TryReadArrayHeader(ref reader))
            {
                throw new Exception("无法反序列化数据，数据类型不匹配。");
            }

            var len = _int.Deserialize(ref reader);
            if (len == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] array = new byte[len];
            reader.Read(array.AsSpan());
            return array;
        }

        public override void Serialize(AbstractBuffer<byte> buffer, byte[] value)
        {
            if (value == null)
            {
                WriteNil(buffer);
                return;
            }

            WriteArrayHeader(buffer);
            _int.Serialize(buffer, value.Length);
            buffer.Write(value);
        }

        public override void Serialize(ref SpanWriter<byte> writer, byte[] value)
        {
            if (value == null)
            {
                WriteNil(ref writer);
                return;
            }

            WriteArrayHeader(ref writer);
            _int.Serialize(ref writer, value.Length);
            writer.Write(value);
        }

        public override long GetLength(byte[] value)
        {
            if (value == null)
            {
                return NilLength;
            }

            long result = _int.GetLength(value.Length);
            result += value.Length;
            return result;
        }
    }
}