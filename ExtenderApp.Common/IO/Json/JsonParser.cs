using System.Text.Json;
using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.FileParsers;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// Json 文件解析器
    /// </summary>
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

        public void Serialize<T>(T value, Stream stream)
        {
            JsonSerializer.Serialize(stream, value, _jsonSerializerOptions);
        }

        public T? Deserialize<T>(ReadOnlySpan<byte> span)
        {
            return JsonSerializer.Deserialize<T>(span, _jsonSerializerOptions);
        }

        public byte[] SerializeToBytes<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _jsonSerializerOptions);
        }

        protected override Result<T?> ExecuteRead<T>(IFileOperate fileOperate, long position, int length) where T : default
        {
            try
            {
                fileOperate.Read(out ByteBlock block);
                var result = Deserialize<T>(block);
                block.Dispose();
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.FromException<T?>(ex);
            }
        }

        protected override async ValueTask<Result<T?>> ExecuteReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token) where T : default
        {
            try
            {
                ByteBlock block = await fileOperate.ReadByteBlockAsync(position, length);
                var result = Deserialize<T>(block);
                block.Dispose();
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.FromException<T?>(ex);
            }
        }

        protected override Result ExecuteWrite<T>(IFileOperate fileOperate, T value, long position)
        {
            try
            {
                byte[] bytes = SerializeToBytes(value);
                return fileOperate.Write(position, bytes.AsMemory());
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }

        protected override async ValueTask<Result> ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default)
        {
            try
            {
                byte[] bytes = SerializeToBytes(value);
                return await fileOperate.WriteAsync(position, bytes.AsMemory(), token);
            }
            catch (Exception ex)
            {
                return Result.FromException(ex);
            }
        }
    }
}