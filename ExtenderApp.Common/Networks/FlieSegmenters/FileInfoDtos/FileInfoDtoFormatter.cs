using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class FileInfoDtoFormatter : ResolverFormatter<FileInfoDto>
    {
        private readonly IBinaryFormatter<string> _string;
        private readonly IBinaryFormatter<long> _long;

        public override int DefaultLength => throw new NotImplementedException();

        public FileInfoDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = GetFormatter<string>();
            _long = GetFormatter<long>();
        }

        public override FileInfoDto Deserialize(ref ExtenderBinaryReader reader)
        {
            FileInfoDto dto = new FileInfoDto();

            dto.FileName = _string.Deserialize(ref reader);
            dto.FileSize = _long.Deserialize(ref reader);

            return dto;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, FileInfoDto value)
        {
            _string.Serialize(ref writer, value.FileName);
            _long.Serialize(ref writer, value.FileSize);
        }
    }
}
