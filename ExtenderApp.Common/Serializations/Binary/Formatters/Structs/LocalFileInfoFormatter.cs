using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// 本地文件信息的二进制格式化器。 负责将 <see cref="LocalFileInfo"/> 类型的值序列化为二进制表示，并从二进制表示反序列化出 <see cref="LocalFileInfo"/> 实例。
    /// </summary>
    internal sealed class LocalFileInfoFormatter : ResolverFormatter<LocalFileInfo>
    {
        private readonly IBinaryFormatter<string> _string;

        public LocalFileInfoFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _string = resolver.GetFormatter<string>();
        }

        public override sealed void Serialize(ref SpanWriter<byte> writer, LocalFileInfo value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }
            _string.Serialize(ref writer, value.FullPath);
        }

        public override sealed void Serialize(ref BinaryWriterAdapter writer, LocalFileInfo value)
        {
            if (value.IsEmpty)
            {
                WriteNil(ref writer);
                return;
            }
            _string.Serialize(ref writer, value.FullPath);
        }

        public override sealed LocalFileInfo Deserialize(ref BinaryReaderAdapter reader)
        {
            if (TryReadNil(ref reader))
                return LocalFileInfo.Empty;

            var result = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(result))
                return LocalFileInfo.Empty;

            return result;
        }

        public override sealed LocalFileInfo Deserialize(ref SpanReader<byte> reader)
        {
            if (TryReadNil(ref reader))
                return LocalFileInfo.Empty;

            var result = _string.Deserialize(ref reader);
            if (string.IsNullOrEmpty(result))
                return LocalFileInfo.Empty;

            return result;
        }

        public override sealed long GetLength(LocalFileInfo value)
        {
            if (value.IsEmpty)
                return 1;
            return _string.GetLength(value.FullPath);
        }
    }
}