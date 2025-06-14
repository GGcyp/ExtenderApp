using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.FlieSegmenters.FileResponseDtos
{
    internal class FileTransferConfigDtoFormatter : ResolverFormatter<FileTransferConfigDto>
    {
        private readonly IBinaryFormatter<int> _int;

        public override int Length => _int.Length;

        public FileTransferConfigDtoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _int = GetFormatter<int>();
        }

        public override FileTransferConfigDto Deserialize(ref ExtenderBinaryReader reader)
        {
            var infos = _int.Deserialize(ref reader);
            return new FileTransferConfigDto(infos);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, FileTransferConfigDto value)
        {
            _int.Serialize(ref writer, value.LinkerCount);
        }

        public override long GetLength(FileTransferConfigDto value)
        {
            return _int.Length;
        }
    }
}
