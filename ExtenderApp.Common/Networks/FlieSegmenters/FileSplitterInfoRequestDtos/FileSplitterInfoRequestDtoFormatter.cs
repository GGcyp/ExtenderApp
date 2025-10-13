using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class FileSplitterInfoRequestDtoFormatter : ResolverFormatter<FileSplitterInfoRequestDto>
    {
        private readonly IBinaryFormatter<int> _int;
        public override int DefaultLength => _int.DefaultLength * 2;

        public FileSplitterInfoRequestDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override FileSplitterInfoRequestDto Deserialize(ref ByteBuffer buffer)
        {
            FileSplitterInfoRequestDto result = new FileSplitterInfoRequestDto();

            result.FileHashCode = _int.Deserialize(ref buffer);
            result.SplitterSize = _int.Deserialize(ref buffer);

            return result;
        }

        public override void Serialize(ref ByteBuffer buffer, FileSplitterInfoRequestDto value)
        {
            _int.Serialize(ref buffer, value.FileHashCode);
            _int.Serialize(ref buffer, value.SplitterSize);
        }

        public override long GetLength(FileSplitterInfoRequestDto value)
        {
            return _int.GetLength(value.FileHashCode) + _int.GetLength(value.SplitterSize);
        }
    }
}
