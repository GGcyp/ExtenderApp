using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Data;
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

        #region Write

        public override void Write<T>(FileOperateInfo operate, T value)
        {
            try
            {
                //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
                var jsonOptions = _jsonSerializerOptions;

                using (FileStream stream = operate.OpenFile())
                {
                    JsonSerializer.Serialize(stream, value, jsonOptions);
                }
            }
            catch (Exception ex)
            {
                // 在这里处理异常，例如记录日志或通知用户
                throw;
            }
        }

        public override void Write<T>(ExpectLocalFileInfo info, T value)
        {
            Write(info.CreatLocalFileInfo(FileExtensions.JsonFileExtensions).CreateFileOperate(), value);
        }

        #endregion

        #region Read

        public override T? Read<T>(FileOperateInfo operate) where T : default
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

        public override T? Read<T>(ExpectLocalFileInfo info) where T : default
        {
            return Read<T>(info.CreateFileOperate(FileExtensions.JsonFileExtensions));
        }

        public override T? Read<T>(IFileOperate fileOperate) where T : default
        {
            throw new NotImplementedException();
        }

        public override T? Read<T>(ExpectLocalFileInfo info, long position, int length) where T : default
        {
            throw new NotImplementedException();
        }

        public override T? Read<T>(FileOperateInfo info, long position, int length) where T : default
        {
            throw new NotImplementedException();
        }

        public override T? Read<T>(IFileOperate fileOperate, long position, int length) where T : default
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Deserialize


        public T? Deserialize<T>(string json)
        {
            //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            var jsonOptions = _jsonSerializerOptions;
            return JsonSerializer.Deserialize<T>(json, jsonOptions);
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

        //public override T? Read<T>(ExpectLocalFileInfo info, IFileOperate fileOperate = null) where T : default
        //{
        //    return Read<T>(info.CreateWriteOperate(FileExtensions.JsonFileExtensions), fileOperate);
        //}

        //public override T? Read<T>(FileOperateInfo info, IFileOperate fileOperate = null) where T : default
        //{
        //    //var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
        //    var jsonOptions = _jsonSerializerOptions;
        //    T? result = default;
        //    using (FileStream stream = info.OpenFile())
        //    {
        //        result = JsonSerializer.Deserialize<T>(stream, jsonOptions);
        //    }
        //    return result;
        //}

        #endregion

        #region Delete

        public override void Delete(ExpectLocalFileInfo info)
        {
            var jsonFileInfo = info.CreatLocalFileInfo(FileExtensions.JsonFileExtensions);
            //_provider.Delete(jsonFileInfo);
            jsonFileInfo.Delete();
        }



        public override void ReadAsync<T>(ExpectLocalFileInfo info, Action<T?> callback) where T : default
        {
            throw new NotImplementedException();
        }

        public override void ReadAsync<T>(FileOperateInfo info, Action<T?> callback) where T : default
        {
            throw new NotImplementedException();
        }

        public override void ReadAsync<T>(IFileOperate fileOperate, Action<T?> callback) where T : default
        {
            throw new NotImplementedException();
        }

        public override void ReadAsync<T>(ExpectLocalFileInfo info, long position, int length, Action<T?> callback) where T : default
        {
            throw new NotImplementedException();
        }

        public override void ReadAsync<T>(FileOperateInfo info, long position, int length, Action<T?> callback) where T : default
        {
            throw new NotImplementedException();
        }

        public override void ReadAsync<T>(IFileOperate fileOperate, long position, int length, Action<T?> callback) where T : default
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(IFileOperate fileOperate, T value)
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(ExpectLocalFileInfo info, T value, long position)
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(FileOperateInfo info, T value, long position)
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(IFileOperate fileOperate, T value, long position)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(IFileOperate fileOperate, T value, Action? callback = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, long position, Action? callback = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, long position, Action? callback = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(IFileOperate fileOperate, T value, long position, Action? callback = null)
        {
            throw new NotImplementedException();
        }


        //public override T? Read<T>(ExpectLocalFileInfo info) where T : default
        //{
        //    throw new NotImplementedException();
        //}


        #endregion
    }
}
