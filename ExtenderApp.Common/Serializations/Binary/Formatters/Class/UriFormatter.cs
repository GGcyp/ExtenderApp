using System;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Buffer.Reader;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Binary.Formatters
{
    /// <summary>
    /// UriFormatter 类，继承自 ResolverFormatter<Uri> 类。
    /// 用于格式化 Uri 类型的对象。
    /// </summary>
    internal class UriFormatter : ResolverFormatter<Uri>
    {
        private readonly IBinaryFormatter<string> _formatter;

        public UriFormatter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _formatter = GetFormatter<string>();
        }

        public override int DefaultLength => _formatter.DefaultLength;

        public override Uri Deserialize(AbstractBufferReader<byte> reader)
        {
            var result = _formatter.Deserialize(reader);
            if (string.IsNullOrEmpty(result))
                return null!;
            return new Uri(result);
        }

        public override Uri Deserialize(ref SpanReader<byte> reader)
        {
            var result = _formatter.Deserialize(ref reader);
            if (string.IsNullOrEmpty(result))
                return null!;
            return new Uri(result);
        }

        public override void Serialize(AbstractBuffer<byte> buffer, Uri value)
        {
            _formatter.Serialize(buffer, value?.ToString() ?? string.Empty);
        }

        public override void Serialize(ref SpanWriter<byte> writer, Uri value)
        {
            _formatter.Serialize(ref writer, value?.ToString() ?? string.Empty);
        }

        public override long GetLength(Uri value)
        {
            if (value == null)
            {
                return NilLength;
            }

            return _formatter.GetLength(value.ToString());
        }
    }
}
