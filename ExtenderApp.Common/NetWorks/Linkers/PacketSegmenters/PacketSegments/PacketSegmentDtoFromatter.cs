using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.NetWorks
{
    internal class PacketSegmentDtoFromatter : ExtenderFormatter<PacketSegmentDto>
    {
        private readonly IBinaryFormatter<int> _int;

        public override int Length => _int.Length * 3 + 5;

        public PacketSegmentDtoFromatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            _int = resolver.GetFormatterWithVerify<int>();
        }

        public override PacketSegmentDto Deserialize(ref ExtenderBinaryReader reader)
        {
            int segmentCount = _int.Deserialize(ref reader);
            int length = _int.Deserialize(ref reader);
            length = _int.Deserialize(ref reader);

            var data = ArrayPool<byte>.Shared.Rent(length);
            var sequence = _binaryReaderConvert.ReadRaw(ref reader, length);
            int readCount = 0;
            foreach (var segment in sequence)
            {
                segment.Span.CopyTo(data.AsSpan(readCount));
                readCount += segment.Span.Length;
            }

            return new PacketSegmentDto(segmentCount, length, new ReadOnlyMemory<byte>(data, 0, length));
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, PacketSegmentDto value)
        {
            _int.Serialize(ref writer, value.SegmentIndex);
            _int.Serialize(ref writer, value.Length);
            _int.Serialize(ref writer, value.Data.Length);

            _binaryWriterConvert.WriteRaw(ref writer, value.Data.Span);
        }

        public override long GetLength(PacketSegmentDto value)
        {
            return _int.Length * 3 + value.Data.Length;
        }
    }
}
