using System.Text;
using System.Text.Json;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.IO.FileParsers;

namespace ExtenderApp.Common.Serializations.Json
{
    /// <summary>
    /// Json 序列化实现，基于 System.Text.Json。
    /// </summary>
    internal sealed class JsonSerialization : Serialization, IJsonSerialization
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonSerialization()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                // 例如: 配置缩进、命名策略等
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
        }

        public T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }

        public string SerializeToString<T>(T value)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public override byte[] Serialize<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
        }

        public override void Serialize<T>(ref SpanWriter<byte> writer, T value)
        {
            var jsonstring = JsonSerializer.Serialize(value, _jsonSerializerOptions);

            Encoding encoding = Encoding.UTF8;
            int maxLength = encoding.GetMaxByteCount(jsonstring.Length);
            if (writer.Remaining < maxLength)
            {
                throw new InvalidOperationException("SpanWriter 空间不足以写入序列化数据。");
            }
            writer.Advance(Encoding.UTF8.GetBytes(jsonstring, writer.UnwrittenSpan));
        }

        public override void Serialize<T>(ref BinaryWriterAdapter writer, T value)
        {
            var jsonstring = JsonSerializer.Serialize(value, _jsonSerializerOptions);

            Encoding encoding = Encoding.UTF8;
            int maxLength = encoding.GetMaxByteCount(jsonstring.Length);
            writer.Advance(Encoding.UTF8.GetBytes(jsonstring, writer.GetSpan(maxLength)));
        }

        public override void Serialize<T>(T value, out AbstractBuffer<byte> buffer)
        {
            var jsonstring = JsonSerializer.Serialize(value, _jsonSerializerOptions);

            Encoding encoding = Encoding.UTF8;
            int maxLength = encoding.GetMaxByteCount(jsonstring.Length);
            buffer = MemoryBlock<byte>.GetBuffer(maxLength);
            Span<byte> span = buffer.GetSpan(maxLength);
            if (span.Length < maxLength)
            {
                buffer.TryRelease();
                throw new InvalidOperationException("AbstractBuffer<byte> 空间不足以写入序列化数据。");
            }
            buffer.Advance(Encoding.UTF8.GetBytes(jsonstring, span));
        }

        public override T? Deserialize<T>(ref BinaryReaderAdapter reader) where T : default
        {
            var span = reader.ToArray();
            var result = JsonSerializer.Deserialize<T>(span, _jsonSerializerOptions);
            reader.Advance(span.Length);
            return result!;
        }

        public override T? Deserialize<T>(ref SpanReader<byte> reader) where T : default
        {
            var span = reader.UnreadSpan;
            var result = JsonSerializer.Deserialize<T>(span, _jsonSerializerOptions);
            reader.Advance(span.Length);
            return result!;
        }
    }
}