using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binary.Formatters;
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
            if(TryReadNil(ref reader))
                return LocalFileInfo.Empty;

            var result = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(result))
                return LocalFileInfo.Empty;

            return result;
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, LocalFileInfo value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }
            _string.Serialize(ref writer, value.FullPath);
        }

        public override long GetLength(LocalFileInfo value)
        {
            if (value.IsEmpty)
                return 1;
            return _string.GetLength(value.FullPath);
        }
    }
}
