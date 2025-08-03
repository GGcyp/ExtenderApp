using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Binary.Formatter
{
    internal class BitFieldDataFormatter : ExtenderFormatter<BitFieldData>
    {
        public override int DefaultLength => 1;

        public BitFieldDataFormatter(ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
        }

        public override BitFieldData Deserialize(ref ExtenderBinaryReader reader)
        {
            if (!_binaryReaderConvert.TryReadArrayHeader(ref reader, out var length))
            {
                return null;
            }
            var bitFieldData = new BitFieldData(length);

            int index = 0;
            var memories = _binaryReaderConvert.ReadRaw(ref reader, length);
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

        public override void Serialize(ref ExtenderBinaryWriter writer, BitFieldData value)
        {
            if (value is null)
            {
                _binaryWriterConvert.WriteNil(ref writer);
                return;
            }

            var bytes = ArrayPool<byte>.Shared.Rent(value.Length);
            value.ToBytes(bytes);
            _binaryWriterConvert.WriteArrayHeader(ref writer, value.Length);
            _binaryWriterConvert.WriteRaw(ref writer, bytes.AsSpan(0, value.Length));
            ArrayPool<byte>.Shared.Return(bytes);
        }

        public override long GetLength(BitFieldData value)
        {
            if (value is null)
                return 1; // 如果是null，返回1字节表示null

            return 5 + value.Length;
        }
    }
}
