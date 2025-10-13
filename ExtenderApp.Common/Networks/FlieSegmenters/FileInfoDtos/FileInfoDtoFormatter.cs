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

        public override FileInfoDto Deserialize(ref ByteBuffer buffer)
        {
            FileInfoDto dto = new FileInfoDto();

            dto.FileName = _string.Deserialize(ref buffer);
            dto.FileSize = _long.Deserialize(ref buffer);

            return dto;
        }

        public override void Serialize(ref ByteBuffer buffer, FileInfoDto value)
        {
            _string.Serialize(ref buffer, value.FileName);
            _long.Serialize(ref buffer, value.FileSize);
        }

        public override long GetLength(FileInfoDto value)
        {
            return _string.GetLength(value.FileName) + _long.GetLength(value.FileSize);
        }
    }
}
