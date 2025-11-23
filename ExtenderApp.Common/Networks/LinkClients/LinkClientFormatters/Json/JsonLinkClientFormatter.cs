

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

        private readonly IBinaryFormatter<string> _stringFormatter;

        public JsonLinkClientFormatter(IBinaryFormatter<string> stringFormatter)
        {
            _stringFormatter = stringFormatter;
            Options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
            };
        }

        protected override T Deserialize(ref ByteBuffer buffer)
        {
            string temp = _stringFormatter.Deserialize(ref buffer);
            return JsonSerializer.Deserialize<T>(temp, Options)!;
        }

        protected override void Serialize(T value, ref ByteBuffer buffer)
        {
            var temp = JsonSerializer.Serialize(value, Options);
            _stringFormatter.Serialize(ref buffer, temp);
        }
    }
}
