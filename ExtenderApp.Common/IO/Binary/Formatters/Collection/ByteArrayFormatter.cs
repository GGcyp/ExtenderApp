

using System.Buffers;
using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatters.Collection
{
    internal class ByteArrayFormatter : BinaryFormatter<byte[]>
    {
        public ByteArrayFormatter(ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
        }

        public override int DefaultLength => 5;

        public override byte[] Deserialize(ref ByteBuffer buffer)
        {
            if (_bufferConvert.TryReadNil(ref buffer))
            {
                return Array.Empty<byte>();
            }

            var len = _bufferConvert.ReadArrayHeader(ref buffer);
            if (len == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] array = Array.Empty<byte>();

            array = _bufferConvert.ReadRaw(ref buffer, len).ToArray();

            return array;
        }

        public override void Serialize(ref ByteBuffer buffer, byte[] value)
        {
            if (value == null)
            {
                _bufferConvert.WriteNil(ref buffer);
            }
            else
            {
                _bufferConvert.WriteArrayHeader(ref buffer, value.Length);
                _bufferConvert.Write(ref buffer, value);
            }
        }

        public override long GetLength(byte[] value)
        {
            if (value == null)
            {
                return 1;
            }

            var result = DefaultLength;
            result += value.Length;
            return result;
        }
    }
}
