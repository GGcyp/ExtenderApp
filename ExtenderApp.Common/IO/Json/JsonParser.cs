﻿using ExtenderApp.Abstract;
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

        public bool Write<T>(FileOperateInfo operate, T value, object? options = null)
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

        public bool Write<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            return Write(info.CreatLocalFileInfo(FileExtensions.JsonFileExtensions).CreateFileOperate(), value, options);
        }

        #endregion

        #region Read

        #endregion

        #region Deserialize

        public T? Read<T>(FileOperateInfo operate, object? options = null)
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

        public T? Read<T>(ExpectLocalFileInfo info, object? options = null)
        {
            return Read<T>(info.CreateFileOperate(FileExtensions.JsonFileExtensions), options);
        }

        public async ValueTask<T?> ReadAsync<T>(FileOperateInfo operate, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;
            T? result = default;
            using (FileStream stream = operate.OpenFile())
            {
                result = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions);
            }
            return result;
        }

        public ValueTask<T?> ReadAsync<T>(ExpectLocalFileInfo info, object? options = null)
        {
            return ReadAsync<T>(info.CreateFileOperate(FileExtensions.JsonFileExtensions), options);
        }

        #endregion

        #region Serialize

        public string Serialize<T>(T value, object? options = null)
        {
            var jsonOptions = options as JsonSerializerOptions ?? _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, jsonOptions);
        }

        public async ValueTask<bool> WriteAsync<T>(FileOperateInfo operate, T value, object? options = null)
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

        public ValueTask<bool> WriteAsync<T>(ExpectLocalFileInfo info, T value, object? options = null)
        {
            return WriteAsync(info.CreateFileOperate(FileExtensions.JsonFileExtensions), value, options);
        }

        public void ReadAsync<T>(ExpectLocalFileInfo info, Action<T> callback, object? options = null)
        {
            throw new NotImplementedException();
        }

        public void ReadAsync<T>(FileOperateInfo operate, Action<T> callback, object? options = null)
        {
            throw new NotImplementedException();
        }

        public void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, object? options = null)
        {
            throw new NotImplementedException();
        }

        public void WriteAsync<T>(FileOperateInfo operate, T value, Action? callback = null, object? options = null)
        {
            throw new NotImplementedException();
        }

        public override T? Read<T>(ExpectLocalFileInfo info, IConcurrentOperate fileOperate = null, object? options = null) where T : default
        {
            throw new NotImplementedException();
        }

        public override T? Read<T>(FileOperateInfo info, IConcurrentOperate fileOperate = null, object? options = null) where T : default
        {
            throw new NotImplementedException();
        }

        public override void ReadAsync<T>(ExpectLocalFileInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null)
        {
            throw new NotImplementedException();
        }

        public override void ReadAsync<T>(FileOperateInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null)
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(ExpectLocalFileInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null)
        {
            throw new NotImplementedException();
        }

        public override void Write<T>(FileOperateInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null)
        {
            throw new NotImplementedException();
        }

        public override void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
