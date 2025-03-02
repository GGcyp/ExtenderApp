using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Splitter
{
    internal class SplitterDtoFormatter : ResolverFormatter<SplitterDto>
    {
        private readonly IBinaryFormatter<string> _string;
        private readonly IBinaryFormatter<uint> _uint;
        private readonly IBinaryFormatter<byte[]> _bytes;

        public override int Length => _uint.Length + _bytes.Length;

        public SplitterDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = GetFormatter<string>();
            _uint = GetFormatter<uint>();
            _bytes = GetFormatter<byte[]>();
        }

        public override SplitterDto Deserialize(ref ExtenderBinaryReader reader)
        {
            var fileName = _string.Deserialize(ref reader);
            var index = _uint.Deserialize(ref reader);
            var bytes = _bytes.Deserialize(ref reader);
            return new SplitterDto(fileName, index, bytes);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, SplitterDto value)
        {
            _string.Serialize(ref writer, value.FileName);
            _uint.Serialize(ref writer, value.ChunkIndex);
            _bytes.Serialize(ref writer, value.Bytes);
        }

        public override long GetLength(SplitterDto value)
        {
            return _uint.Length + _string.GetLength(value.FileName) + _bytes.GetLength(value.Bytes);
        }
    }
}
