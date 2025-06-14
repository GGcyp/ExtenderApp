using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class FileTransferRequestDtoFormatter : ResolverFormatter<FileTransferRequestDto>
    {
        private readonly IBinaryFormatter<FileInfoDto[]> _fileInfoDtos;

        public FileTransferRequestDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _fileInfoDtos = GetFormatter<FileInfoDto[]>();
        }

        public override int Length => _fileInfoDtos.Length;

        public override FileTransferRequestDto Deserialize(ref ExtenderBinaryReader reader)
        {
            var fileInfoDtos = _fileInfoDtos.Deserialize(ref reader);
            var result = new FileTransferRequestDto(fileInfoDtos);
            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, FileTransferRequestDto value)
        {
            _fileInfoDtos.Serialize(ref writer, value.FileInfoDtos);
        }

        public override long GetLength(FileTransferRequestDto value)
        {
            return _fileInfoDtos.GetLength(value.FileInfoDtos);
        }
    }
}
