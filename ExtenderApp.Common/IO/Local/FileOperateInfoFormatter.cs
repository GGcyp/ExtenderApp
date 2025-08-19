using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatters;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.Local
{
    internal class FileOperateInfoFormatter : ResolverFormatter<FileOperateInfo>
    {
        private readonly IBinaryFormatter<LocalFileInfo> _localFileInfo;
        private readonly IBinaryFormatter<int> _int;

        public override int DefaultLength => _int.DefaultLength * 3 + _localFileInfo.DefaultLength;

        public FileOperateInfoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _localFileInfo = resolver.GetFormatter<LocalFileInfo>();
            _int = resolver.GetFormatter<int>();
        }

        public override FileOperateInfo Deserialize(ref ExtenderBinaryReader reader)
        {
            var fileMode = (FileMode)_int.Deserialize(ref reader);
            var fileAccess = (FileAccess)_int.Deserialize(ref reader);
            var localFileInfo = _localFileInfo.Deserialize(ref reader);
            return new FileOperateInfo(localFileInfo, fileMode, fileAccess);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, FileOperateInfo value)
        {
            _int.Serialize(ref writer, (int)value.FileMode);
            _int.Serialize(ref writer, (int)value.FileAccess);
            _localFileInfo.Serialize(ref writer, value.LocalFileInfo);
        }

        public override long GetLength(FileOperateInfo value)
        {
            return _localFileInfo.GetLength(value.LocalFileInfo) + _int.DefaultLength * 3;
        }
    }
}
