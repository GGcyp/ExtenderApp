using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Data;
using System.Text.Json;

namespace ExtenderApp.Common.IO
{
    internal class JsonParser : FileParser, IJsonParser
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public JsonParser(FileStore store) : base(store)
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                // 例如: 配置缩进、命名策略等
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
        }


        #region Write

        public bool Write<T>(FileOperateInfo operate, T value)
        {
            try
            {
                //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
                var jsonOptions = _jsonSerializerOptions;

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

        public bool Write<T>(ExpectLocalFileInfo info, T value)
        {
            return Write(info.CreatLocalFileInfo(FileExtensions.JsonFileExtensions).CreateFileOperate(), value);
        }

        #endregion

        #region Read

        #endregion

        #region Deserialize

        public T? Read<T>(FileOperateInfo operate)
        {
            //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            var jsonOptions = _jsonSerializerOptions;
            T? result = default;
            using (FileStream stream = operate.OpenFile())
            {
                result = JsonSerializer.Deserialize<T>(stream, jsonOptions);
            }
            return result;
        }

        public T? Deserialize<T>(string json)
        {
            //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            var jsonOptions = _jsonSerializerOptions;
            return JsonSerializer.Deserialize<T>(json, jsonOptions);
        }

        public T? Read<T>(ExpectLocalFileInfo info)
        {
            return Read<T>(info.CreateFileOperate(FileExtensions.JsonFileExtensions));
        }

        public async ValueTask<T?> ReadAsync<T>(FileOperateInfo operate)
        {
            //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            var jsonOptions = _jsonSerializerOptions;
            T? result = default;
            using (FileStream stream = operate.OpenFile())
            {
                result = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
            }
            return result;
        }

        public ValueTask<T?> ReadAsync<T>(ExpectLocalFileInfo info)
        {
            return ReadAsync<T>(info.CreateFileOperate(FileExtensions.JsonFileExtensions));
        }

        #endregion

        #region Serialize

        public string Serialize<T>(T value)
        {
            //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            var jsonOptions = _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, jsonOptions);
        }

        public async ValueTask<bool> WriteAsync<T>(FileOperateInfo operate, T value)
        {
            try
            {
                //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
                var jsonOptions = _jsonSerializerOptions;
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

        public override T? Read<T>(ExpectLocalFileInfo info, IConcurrentOperate fileOperate = null) where T : default
        {
            return Read<T>(info.CreateWriteOperate(FileExtensions.JsonFileExtensions), fileOperate);
        }

        public override T? Read<T>(FileOperateInfo info, IConcurrentOperate fileOperate = null) where T : default
        {
            //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            var jsonOptions = _jsonSerializerOptions;
            T? result = default;
            using (FileStream stream = info.OpenFile())
            {
                result = JsonSerializer.Deserialize<T>(stream, jsonOptions);
            }
            return result;
        }

        public override void ReadAsync<T>(ExpectLocalFileInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null)
        {
            throw new NotImplementedException();
        }

        public override void ReadAsync<T>(FileOperateInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null)
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(ExpectLocalFileInfo info, T value, IConcurrentOperate fileOperate = null)
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(FileOperateInfo info, T value, IConcurrentOperate fileOperate = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Delete

        public override void Delete(ExpectLocalFileInfo info)
        {
            var jsonFileInfo = info.CreatLocalFileInfo(FileExtensions.JsonFileExtensions);
            _store.Delete(jsonFileInfo);
            jsonFileInfo.Delete();
        }

        #endregion
    }
}
