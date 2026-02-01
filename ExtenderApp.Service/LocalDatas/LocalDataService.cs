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
        private readonly IBinarySerialization _binaryParser;

        /// <summary>
        /// 日志服务接口，用于记录日志信息。
        /// </summary>
        private readonly ILogger<ILocalDataService> _logger;

        public LocalDataService(IBinarySerialization parser, IBinaryFormatterStore store, ILogger<ILocalDataService> logger)
        {
            _binaryParser = parser;
            _logger = logger;
        }

        /// <summary>
        /// 获取本地文件信息。
        /// </summary>
        /// <param name="fileName">文件名或相对于数据目录的路径。</param>
        /// <returns>相对本地数据目录的文件信息。</returns>
        private ExpectLocalFileInfo GetFileInfo(string fileName)
        {
            return new(ProgramDirectory.DataPath, fileName);
        }

        #region Load

        ///<inheritdoc/>
        public Result<T?> LoadData<T>(string fileName)
        {
            //try
            //{
            //    T? result = _binaryParser.Read<T>(GetFileInfo(fileName));
            //    return Result.Success(result);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "加载本地数据时发生错误，文件名：{FileName}", fileName);
            //    return Result.FromException<T?>(ex);
            //}
            return Result.Success<T?>(default!);
        }

        ///<inheritdoc/>
        public async ValueTask<Result<T?>> LoadDataAsync<T>(string fileName, CancellationToken token = default)
        {
            //try
            //{
            //    var result = await _binaryParser.ReadAsync<T>(GetFileInfo(fileName), token);
            //    return Result.Success(result);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "加载本地数据时发生错误，文件名：{FileName}", fileName);
            //    return Result.FromException<T?>(ex);
            //}
            return Result.Success<T?>(default!);
        }

        #endregion Load

        #region Save

        ///<inheritdoc/>
        public Result SaveData<T>(string fileName, T data, CompressionType compressionType = CompressionType.Block)
        {
            //try
            //{
            //    _binaryParser.Write(GetFileInfo(fileName), data, compressionType);
            //    return Result.Success();
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "保存本地数据时发生错误，文件名：{FileName}", fileName);
            //    return Result.FromException(ex);
            //}
            return Result.Success();
        }

        ///<inheritdoc/>
        public ValueTask<Result> SaveDataAsync<T>(string fileName, T data, CancellationToken token = default, CompressionType compressionType = CompressionType.Block)
        {
            if (string.IsNullOrEmpty(fileName))
                return ValueTask.FromResult(Result.Failure("文件名不能为空"));

            //try
            //{
            //    return _binaryParser.WriteAsync(GetFileInfo(fileName), data, compressionType, token);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "保存本地数据时发生错误，文件名：{FileName}", fileName);
            //    return ValueTask.FromException<Result>(ex);
            //}
            return ValueTask.FromResult(Result.Success());
        }

        #endregion Save
    }
}