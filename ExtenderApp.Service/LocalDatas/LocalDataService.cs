using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 本地数据服务类，实现了ILocalDataService接口。
    /// </summary>
    internal class LocalDataService : ILocalDataService
    {
        /// <summary>
        /// 二进制解析器接口，用于解析二进制数据。
        /// </summary>
        private readonly IBinaryParser _binaryParser;

        /// <summary>
        /// 日志服务接口，用于记录日志信息。
        /// </summary>
        private readonly ILogger<ILocalDataService> _logger;

        public LocalDataService(IBinaryParser parser, IBinaryFormatterStore store, ILogger<ILocalDataService> logger)
        {
            _binaryParser = parser;
            _logger = logger;
        }

        private ExpectLocalFileInfo GetFileFullPath(string fileName)
        {
            return new(ProgramDirectory.DataPath, fileName);
        }

        //#region Load

        //public Result<T?> LoadData<T>(string fileName)
        //{
        //    try
        //    {
        //        var fileOperate = _fileOperateProvider.GetFileOperate(GetFileFullPath(fileName));
        //        fileOperate.Read(out ByteBuffer buffer);
        //        _fileOperateProvider.ReleaseOperate(fileOperate);
        //        T? result = _binaryParser.Deserialize<T>(ref buffer);
        //        buffer.Dispose();
        //        return Result.Success(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "加载本地数据时发生错误，文件名：{FileName}", fileName);
        //        return Result.FromException<T?>(ex);
        //    }
        //}

        //public async ValueTask<Result<T?>> LoadDataAsync<T>(string fileName)
        //{
        //    try
        //    {
        //        var fileOperate = _fileOperateProvider.GetFileOperate(GetFileFullPath(fileName));
        //        int length= (int)fileOperate.Info.Length;
        //        ByteBlock block = new(length);
        //        await fileOperate.ReadByteBlockAsync(block.GetMemory(length));
        //        _fileOperateProvider.ReleaseOperate(fileOperate);
        //        T? result = _binaryParser.Deserialize<T>(block.UnreadMemory);
        //        block.Dispose();
        //        return Result.Success(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "加载本地数据时发生错误，文件名：{FileName}", fileName);
        //        return Result.FromException<T?>(ex);
        //    }
        //}

        //#endregion Load

        //#region Save

        //public void SaveData<T>(string fileName, T data, CompressionType compressionType = CompressionType.Lz4Block)
        //{
        //    ByteBuffer buffer = new();
        //    try
        //    {
        //        _binaryParser.Serialize(data, out buffer, compressionType);
        //        var fileOperate = _fileOperateProvider.GetFileOperate(GetFileFullPath(fileName));
        //        fileOperate.Write(ref buffer);
        //        _fileOperateProvider.ReleaseOperate(fileOperate);
        //    }
        //    catch (Exception ex)
        //    {
        //        buffer.Dispose();
        //        _logger.LogError(ex, "保存本地数据时发生错误，文件名：{FileName}", fileName);
        //    }
        //    finally
        //    {
        //        buffer.Dispose();
        //    }
        //}

        //public ValueTask<Result> SaveDataAsync<T>(string fileName, T data, CancellationToken token, CompressionType compressionType = CompressionType.Lz4Block)
        //{
        //    if (string.IsNullOrEmpty(fileName))
        //        return ValueTask.FromResult(Result.Failure("文件名不能为空"));

        //    try
        //    {
        //        return _binaryParser.WriteAsync(GetFileFullPath(fileName), data, compressionType);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "保存本地数据时发生错误，文件名：{FileName}", fileName);
        //        return ValueTask.FromException<Result>(ex);
        //    }
        //}

        //#endregion Save
    }
}