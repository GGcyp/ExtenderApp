using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal class JsonLinkClientFormatter<T> : LinkClientFormatter<T>
    {
        public JsonSerializerOptions Options { get; set; }

        public JsonLinkClientFormatter()
        {
            Options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
            };
        }

        protected override T Deserialize(ref ByteBuffer buffer)
        {
            Encoding encoding = Encoding.UTF8;
            string temp = encoding.GetString(buffer.UnreadSequence);
            buffer.ReadAdvance(buffer.Remaining);
            return JsonSerializer.Deserialize<T>(temp, Options)!;
        }

        protected override void Serialize(T value, ref ByteBuffer buffer)
        {
            var temp = JsonSerializer.Serialize(value, Options);
            Encoding encoding = Encoding.UTF8;
            int length = encoding.GetMaxByteCount(temp.Length);
            length = encoding.GetBytes(temp, buffer.GetSpan(length));
            buffer.WriteAdvance(length);
        }
    }
}