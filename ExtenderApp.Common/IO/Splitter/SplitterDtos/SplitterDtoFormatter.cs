using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    internal class SplitterDtoFormatter : ResolverFormatter<SplitterDto>
    {
        private readonly IBinaryFormatter<uint> _uint;
        private readonly IBinaryFormatter<byte[]> _bytes;
        private readonly IBinaryFormatter<int> _int;

        public override int Length => _uint.Length + _bytes.Length + _int.Length;

        public SplitterDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _uint = GetFormatter<uint>();
            _bytes = GetFormatter<byte[]>();
            _int = GetFormatter<int>();
        }

        public override SplitterDto Deserialize(ref ExtenderBinaryReader reader)
        {
            var index = _uint.Deserialize(ref reader);
            var length = _int.Deserialize(ref reader);
            var bytes = _bytes.Deserialize(ref reader);
            return new SplitterDto(index, bytes, length);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, SplitterDto value)
        {
            _uint.Serialize(ref writer, value.ChunkIndex);
            _int.Serialize(ref writer, value.Length);
            _bytes.Serialize(ref writer, value.Bytes);
        }

        public override long GetLength(SplitterDto value)
        {
            return Length + _bytes.GetLength(value.Bytes);
        }
    }
}
