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

        public override FileSplitterInfoRequestDto Deserialize(ref ExtenderBinaryReader reader)
        {
            FileSplitterInfoRequestDto result = new FileSplitterInfoRequestDto();

            result.FileHashCode = _int.Deserialize(ref reader);
            result.SplitterSize = _int.Deserialize(ref reader);

            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, FileSplitterInfoRequestDto value)
        {
            _int.Serialize(ref writer, value.FileHashCode);
            _int.Serialize(ref writer, value.SplitterSize);
        }
    }
}
