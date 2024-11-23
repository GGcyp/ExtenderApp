using ExtenderApp.Abstract;
using ExtenderApp.Data;
using System.Text.Json;

namespace ExtenderApp.Common.File
{
    internal class JsonParser_Microsoft : IJsonParser
    {
        public string LibraryName => LibrarySetting.MICROSOFT_LIBRARY;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonParser_Microsoft()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                // 例如: 配置缩进、命名策略等
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public void Parser(FileInfoData infoData, Action<object?> callback, object? options = null)
        {
            var obj = Deserialize(infoData, typeof(object), options);
            callback?.Invoke(obj);
        }

        public void Parser(FileInfoData infoData, object obj, Action<object?> callback, object? options = null)
        {
            Serialize(infoData, options);
            callback?.Invoke(obj);
        }

        public object? Deserialize(FileInfoData infoData, Type type, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            object? result = null;
            using (FileStream stream = new FileStream(infoData.Path, FileMode.Open, FileAccess.Read))
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

        public bool Serialize(object jsonObject, FileInfoData infoData, object? options = null)
        {
            try
            {
                var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;

                using (FileStream stream = new FileStream(infoData.Path, FileMode.Create, FileAccess.Write))
                {
                    JsonSerializer.Serialize(stream, jsonObject, jsonOptions);
                }
                return true;
            }
            catch (Exception ex)
            {
                // 在这里处理异常，例如记录日志或通知用户
                Console.WriteLine($"An error occurred during serialization: {ex.Message}");
                return false;
            }
        }

        public string Serialize(object jsonObject, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;

            return JsonSerializer.Serialize(jsonObject, jsonOptions);
        }

        public ValueTask<object?> DeserializeAsync(FileInfoData infoData, Type type, object? options = null)
        {
            try
            {
                var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
                ValueTask<object?> result;
                using (FileStream stream = new FileStream(infoData.Path, FileMode.Open, FileAccess.Read))
                {
                    result = JsonSerializer.DeserializeAsync(stream, type, jsonOptions);
                }
                return result;
            }
            catch(Exception ex)
            {

                return default;
            }
        }

        public async ValueTask<bool> SerializeAsync(object jsonObject, FileInfoData infoData, object? options = null)
        {
            try
            {
                var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
                using (FileStream stream = new FileStream(infoData.Path, FileMode.Create, FileAccess.Write))
                {
                    await JsonSerializer.SerializeAsync(stream, jsonObject, jsonOptions);
                }
                return true; // 表示序列化成功
            }
            catch (Exception ex)
            {
                // 在这里处理异常，例如记录日志或通知用户
                return false; 
            }
        }
    }
}
