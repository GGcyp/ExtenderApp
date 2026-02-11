using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
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

        public override void Serialize(AbstractBuffer<byte> buffer, LocalFileInfo value)
        {
            if (value.IsEmpty)
            {
                WriteNil(buffer);
                return;
            }
            _string.Serialize(buffer, value.FullPath);
        }

        public override void Serialize(ref SpanWriter<byte> writer, LocalFileInfo value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }
            _string.Serialize(ref writer, value.FullPath);
        }

        public override LocalFileInfo Deserialize(AbstractBufferReader<byte> reader)
        {
            if (TryReadNil(reader))
                return LocalFileInfo.Empty;

            var result = _string.Deserialize(reader);
            if (string.IsNullOrEmpty(result))
                return LocalFileInfo.Empty;

            return result;
        }

        public override LocalFileInfo Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
                return LocalFileInfo.Empty;

            var result = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(result))
                return LocalFileInfo.Empty;

            return result;
        }

        public override long GetLength(LocalFileInfo value)
        {
            if (value.IsEmpty)
                return 1;
            return _string.GetLength(value.FullPath);
        }
    }
}