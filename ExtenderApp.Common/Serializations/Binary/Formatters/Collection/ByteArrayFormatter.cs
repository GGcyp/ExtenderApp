using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters.Collection
{
    internal class ByteArrayFormatter : ResolverFormatter<byte[]>
    {
        private readonly IBinaryFormatter<int> _int;

        public ByteArrayFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override byte[] Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
            {
                return Array.Empty<byte>();
            }

            if (!TryReadArrayHeader(ref buffer))
            {
                throw new Exception("无法反序列化数据，数据类型不匹配。");
            }

            var len = _int.Deserialize(ref buffer);
            if (len == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] array = new byte[len];
            buffer.Read(array.AsMemory());
            return array;
        }

        public override void Serialize(ref ByteBuffer buffer, byte[] value)
        {
            if (value == null)
            {
                WriteNil(ref buffer);
            }
            else
            {
                WriteArrayHeader(ref buffer);
                _int.Serialize(ref buffer, value.Length);
                buffer.Write(value);
            }
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