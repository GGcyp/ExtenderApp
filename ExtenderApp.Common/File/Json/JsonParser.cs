using ExtenderApp.Abstract;
using ExtenderApp.Data;
using System.Text.Json;

namespace ExtenderApp.Common.Files
{
    internal class JsonParser : IJsonParser
    {
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

        #region Deserialize

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

        public T? Deserialize<T>(ExpectLocalFileInfo info, object? options = null)
        {
            return Deserialize<T>(info.CreateFileOperate(FileExtensions.JsonFileExtensions), options);
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

        public ValueTask<T?> DeserializeAsync<T>(ExpectLocalFileInfo info, object? options = null)
        {
            return DeserializeAsync<T>(info.CreateFileOperate(FileExtensions.JsonFileExtensions), options);
        }

        #endregion

        #region Serialize

        public string Serialize<T>(T value, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, jsonOptions);
        }

        public bool Serialize<T>(FileOperate operate, T value, object? options = null)
        {
            try
            {
                var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;

                using (FileStream stream = operate.OpenFile())
                {
                    JsonSerializer.Serialize(stream, value, jsonOptions);
                }
                return true;
            }
            catch (Exception ex)
            {
                // 在这里处理异常，例如记录日志或通知用户
                throw;
            }
        }

        public bool Serialize<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            return Serialize(info.CreatLocalFileInfo(FileExtensions.JsonFileExtensions).CreateFileOperate(), value, options);
        }

        public async ValueTask<bool> SerializeAsync<T>(FileOperate operate, T value, object? options = null)
        {
            try
            {
                var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
                using (FileStream stream = operate.OpenFile())
                {
                    await JsonSerializer.SerializeAsync(stream, value, jsonOptions);
                }
                return true; // 表示序列化成功
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ValueTask<bool> SerializeAsync<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            return SerializeAsync(info.CreateFileOperate(FileExtensions.JsonFileExtensions), value, options);
        }

        #endregion
    }
}
