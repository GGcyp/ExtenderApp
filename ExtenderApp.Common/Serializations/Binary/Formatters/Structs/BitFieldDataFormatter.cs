using System.Buffers;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class BitFieldDataFormatter : BinaryFormatter<BitFieldData>
    {
        public override int DefaultLength => 1;

        public BitFieldDataFormatter(ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
        }

        public override BitFieldData Deserialize(ref ByteBuffer buffer)
        {
            if (!_bufferConvert.TryReadArrayHeader(ref buffer, out var length))
            {
                return BitFieldData.Empty;
            }
            var bitFieldData = new BitFieldData(length);

            int index = 0;
            var memories = _bufferConvert.ReadRaw(ref buffer, length);
            foreach (var memory in memories)
            {
                for (int i = 0; i < memory.Length; i++)
                {
                    var code = memory.Span[i];
                    if (code > 0xFF)
                    {
                        bitFieldData.Set(i);
                    }
                    index++;
                }
            }

            return bitFieldData;
        }

        public override void Serialize(ref ByteBuffer buffer, BitFieldData value)
        {
            if (value.IsEmpty)
            {
                _bufferConvert.WriteNil(ref buffer);
                return;
            }

            var bytes = ArrayPool<byte>.Shared.Rent(value.Length);
            value.ToBytes(bytes);
            _bufferConvert.WriteArrayHeader(ref buffer, value.Length);
            _bufferConvert.WriteRaw(ref buffer, bytes.AsSpan(0, value.Length));
            ArrayPool<byte>.Shared.Return(bytes);
        }

        public override long GetLength(BitFieldData value)
        {
            if (value.IsEmpty)
                return 1; // 如果是null，返回1字节表示null

            return _bufferConvert.GetByteCountArrayHeader(value.Length) + value.Length;
        }
    }
}
