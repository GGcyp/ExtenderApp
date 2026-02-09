using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    internal class LocalFileInfoFormatter : ResolverFormatter<LocalFileInfo>
    {
        private readonly IBinaryFormatter<string> _string;

        public override int DefaultLength => _string.DefaultLength;

        public LocalFileInfoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = resolver.GetFormatter<string>();
        }

        public override LocalFileInfo Deserialize(ref ByteBuffer buffer)
        {
            if (TryReadNil(ref buffer))
                return LocalFileInfo.Empty;

            var result = _string.Deserialize(ref buffer);
            if (string.IsNullOrEmpty(result))
                return LocalFileInfo.Empty;

            return result;
        }

        public override void Serialize(ref ByteBuffer buffer, LocalFileInfo value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref buffer);
                return;
            }
            _string.Serialize(ref buffer, value.FullPath);
        }

        public override long GetLength(LocalFileInfo value)
        {
            if (value.IsEmpty)
                return 1;
            return _string.GetLength(value.FullPath);
        }
    }
}