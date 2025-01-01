using ExtenderApp.Abstract;
using ExtenderApp.Data;
using System.Text.Json;

namespace ExtenderApp.Common.File
{
    internal class JsonParser : IJsonParser
    {
        public FileExtensionType ExtensionTypeType => FileExtensionType.Json;

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonParser()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                // 例如: 配置缩进、命名策略等
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
        }

        public object? Deserialize(FileOperate operate, Type type, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            object? result = null;
            using (FileStream stream = operate.OpenFile())
            {
                result = JsonSerializer.Deserialize(stream, type, jsonOptions);
            }
            return result;
        }

        public object? Deserialize(string json, Type type, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            return JsonSerializer.Deserialize(json, type, jsonOptions);
        }

        public T? Deserialize<T>(FileOperate operate, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            T? result = default;
            using (FileStream stream = operate.OpenFile())
            {
                result = JsonSerializer.Deserialize<T>(stream, jsonOptions);
            }
            return result;
        }

        public T? Deserialize<T>(string json, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            return JsonSerializer.Deserialize<T>(json, jsonOptions);
        }

        public async ValueTask<T?> DeserializeAsync<T>(FileOperate operate, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            T? result = default;
            using (FileStream stream = operate.OpenFile())
            {
                result = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
            }
            return result;
        }

        public ValueTask<object?> DeserializeAsync(FileOperate operate, Type type, object? options = null)
        {
            try
            {
                var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
                ValueTask<object?> result;
                using (FileStream stream = operate.OpenFile())
                {
                    result = JsonSerializer.DeserializeAsync(stream, type, jsonOptions);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool Serialize(object jsonObject, FileOperate operate, object? options = null)
        {
            try
            {
                var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;

                using (FileStream stream = operate.OpenFile())
                {
                    JsonSerializer.Serialize(stream, jsonObject, jsonOptions);
                }
                return true;
            }
            catch (Exception ex)
            {
                // 在这里处理异常，例如记录日志或通知用户
                throw;
            }
        }

        public string SerializeToString(object jsonObject, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;

            return JsonSerializer.Serialize(jsonObject, jsonOptions);
        }

        public async ValueTask<bool> SerializeAsync(object jsonObject, FileOperate operate, object? options = null)
        {
            try
            {
                var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
                using (FileStream stream = operate.OpenFile())
                {
                    await JsonSerializer.SerializeAsync(stream, jsonObject, jsonOptions);
                }
                return true; // 表示序列化成功
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
