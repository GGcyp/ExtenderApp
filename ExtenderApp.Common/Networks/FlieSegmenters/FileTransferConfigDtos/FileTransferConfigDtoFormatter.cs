using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.FlieSegmenters.FileResponseDtos
{
    internal class FileTransferConfigDtoFormatter : ResolverFormatter<FileTransferConfigDto>
    {
        private readonly IBinaryFormatter<int> _int;

        public override int DefaultLength => _int.DefaultLength;

        public FileTransferConfigDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override FileTransferConfigDto Deserialize(ref ByteBuffer buffer)
        {
            var infos = _int.Deserialize(ref buffer);
            return new FileTransferConfigDto(infos);
        }

        public override void Serialize(ref ByteBuffer buffer, FileTransferConfigDto value)
        {
            _int.Serialize(ref buffer, value.LinkerCount);
        }

        public override long GetLength(FileTransferConfigDto value)
        {
            return _int.DefaultLength;
        }
    }
}
