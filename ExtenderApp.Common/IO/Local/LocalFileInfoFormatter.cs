using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    internal class LocalFileInfoFormatter : ResolverFormatter<LocalFileInfo>
    {
        private readonly IBinaryFormatter<string> _string;

        public override int DefaultLength => _string.DefaultLength;

        public LocalFileInfoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = resolver.GetFormatter<string>();
        }

        public override LocalFileInfo Deserialize(ref ExtenderBinaryReader reader)
        {
            var result = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(result))
                return LocalFileInfo.Empty;

            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, LocalFileInfo value)
        {
            if (value.IsEmpty)
            {
                _string.Serialize(ref writer, string.Empty);
                return;
            }
            _string.Serialize(ref writer, value.FilePath);
        }

        public override long GetLength(LocalFileInfo value)
        {
            if (value.IsEmpty)
                return 1;
            return _string.GetLength(value.FilePath);
        }
    }
}
