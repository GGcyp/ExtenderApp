using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
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

        public override int DefaultLength => _fileInfoDtos.DefaultLength;

        public override FileTransferRequestDto Deserialize(ref ByteBuffer buffer)
        {
            var fileInfoDtos = _fileInfoDtos.Deserialize(ref buffer);
            var result = new FileTransferRequestDto(fileInfoDtos);
            return result;
        }

        public override void Serialize(ref ByteBuffer buffer, FileTransferRequestDto value)
        {
            _fileInfoDtos.Serialize(ref buffer, value.FileInfoDtos);
        }

        public override long GetLength(FileTransferRequestDto value)
        {
            return _fileInfoDtos.GetLength(value.FileInfoDtos);
        }
    }
}
