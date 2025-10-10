using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.FileParsers;
using System.Text.Json;

namespace ExtenderApp.Common.IO
{
    internal class JsonParser : FileParser, IJsonParser
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonParser(IFileOperateProvider store) : base(store)
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                // 例如: 配置缩进、命名策略等
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
        }

        protected override string FileExtension => ".json";

        public T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }

        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        protected override T? ExecuteRead<T>(IFileOperate fileOperate) where T : default
        {
            return JsonSerializer.Deserialize<T>(fileOperate.Read(), _jsonSerializerOptions);
        }

        protected override T? ExecuteRead<T>(IFileOperate fileOperate, long position, int length) where T : default
        {
            return JsonSerializer.Deserialize<T>(fileOperate.Read(position, length), _jsonSerializerOptions);
        }

        protected override Task<T?> ExecuteReadAsync<T>(IFileOperate fileOperate, CancellationToken token) where T : default
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(fileOperate.Read(), _jsonSerializerOptions));
        }

        protected override Task<T?> ExecuteReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token) where T : default
        {
            return Task.FromResult(JsonSerializer.Deserialize<T>(fileOperate.Read(position, length), _jsonSerializerOptions));
        }

        protected override void ExecuteWrite<T>(IFileOperate fileOperate, T value)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
            fileOperate.Write(bytes);
        }

        protected override void ExecuteWrite<T>(IFileOperate fileOperate, T value, long position)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
            fileOperate.Write(position, bytes);
        }

        protected override async Task ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, CancellationToken token = default)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
            await fileOperate.WriteAsync(bytes);
        }

        protected override async Task ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
            await fileOperate.WriteAsync(position, bytes);
        }
    }
}