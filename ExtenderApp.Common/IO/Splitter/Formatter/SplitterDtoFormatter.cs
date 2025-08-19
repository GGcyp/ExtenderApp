using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    internal class SplitterDtoFormatter : ExtenderFormatter<SplitterDto>
    {
        private readonly IBinaryFormatter<uint> _uint;
        private readonly IBinaryFormatter<int> _int;
        private readonly IBinaryFormatter<string> _string;

        public override int DefaultLength => _uint.DefaultLength + _int.DefaultLength * 2;

        public SplitterDtoFormatter(IBinaryFormatterResolver resolver, ExtenderBinaryWriterConvert binaryWriterConvert, ExtenderBinaryReaderConvert binaryReaderConvert, BinaryOptions options) : base(binaryWriterConvert, binaryReaderConvert, options)
        {
            _uint = resolver.GetFormatter<uint>();
            _int = resolver.GetFormatter<int>();
        }

        public override SplitterDto Deserialize(ref ExtenderBinaryReader reader)
        {
            var index = _uint.Deserialize(ref reader);
            var length = _int.Deserialize(ref reader);
            var lengthBytes = _int.Deserialize(ref reader);
            var bytesSequence = _binaryReaderConvert.ReadRaw(ref reader, lengthBytes);

            var bytes = ArrayPool<byte>.Shared.Rent(lengthBytes);
            int bytesIndex = 0;
            foreach (var segment in bytesSequence)
            {
                segment.Span.CopyTo(bytes.AsSpan(bytesIndex));
                bytesIndex += segment.Length;
            }

            return new SplitterDto(index, bytes, length);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, SplitterDto value)
        {
            _uint.Serialize(ref writer, value.ChunkIndex);
            _int.Serialize(ref writer, value.Length);
            _int.Serialize(ref writer, value.Bytes.Length);
            _binaryWriterConvert.WriteRaw(ref writer, value.Bytes.AsSpan(0, value.Length));
        }

        public override long GetLength(SplitterDto value)
        {
            return DefaultLength + 5 + value.Length;
        }
    }
}
