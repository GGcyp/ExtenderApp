using System.Buffers;
using System.Text.Json;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Serializations.Json
{
    /// <summary>
    /// Json 序列化实现，基于 System.Text.Json。
    /// </summary>
    internal class JsonSerialization : Serialization, IJsonSerialization
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

        public override void Serialize<T>(T value, Span<byte> span)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
            bytes.AsSpan().CopyTo(span);
        }

        public override void Serialize<T>(T value, out ByteBuffer buffer)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
            buffer = new ();
            buffer.Write(bytes);
        }

        public override T? Deserialize<T>(ReadOnlySpan<byte> span) where T : default
        {
            return JsonSerializer.Deserialize<T>(span, _jsonSerializerOptions);
        }

        public override T? Deserialize<T>(ReadOnlyMemory<byte> memory) where T : default
        {
            return JsonSerializer.Deserialize<T>(memory.Span, _jsonSerializerOptions);
        }

        public override T? Deserialize<T>(ReadOnlySequence<byte> memories) where T : default
        {
            return JsonSerializer.Deserialize<T>(memories.ToArray(), _jsonSerializerOptions);
        }
    }
}