using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    internal class SplitterDtoFormatter : BinaryFormatter<SplitterDto>
    {
        private readonly IBinaryFormatter<uint> _uint;
        private readonly IBinaryFormatter<int> _int;
        private readonly IBinaryFormatter<string> _string;

        public override int DefaultLength => _uint.DefaultLength + _int.DefaultLength * 2;

        public SplitterDtoFormatter(IBinaryFormatterResolver resolver, ByteBufferConvert convert, BinaryOptions options) : base(convert, options)
        {
            _uint = resolver.GetFormatter<uint>();
            _int = resolver.GetFormatter<int>();
        }

        public override SplitterDto Deserialize(ref ByteBuffer buffer)
        {
            var index = _uint.Deserialize(ref buffer);
            var length = _int.Deserialize(ref buffer);
            var lengthBytes = _int.Deserialize(ref buffer);
            var bytesSequence = _bufferConvert.ReadRaw(ref buffer, lengthBytes);

            var bytes = ArrayPool<byte>.Shared.Rent(lengthBytes);
            int bytesIndex = 0;
            foreach (var segment in bytesSequence)
            {
                segment.Span.CopyTo(bytes.AsSpan(bytesIndex));
                bytesIndex += segment.Length;
            }

            return new SplitterDto(index, bytes, length);
        }

        public override void Serialize(ref ByteBuffer buffer, SplitterDto value)
        {
            _uint.Serialize(ref buffer, value.ChunkIndex);
            _int.Serialize(ref buffer, value.Length);
            _int.Serialize(ref buffer, value.Bytes.Length);
            _bufferConvert.WriteRaw(ref buffer, value.Bytes.AsSpan(0, value.Length));
        }

        public override long GetLength(SplitterDto value)
        {
            return DefaultLength + 5 + value.Length;
        }
    }
}
