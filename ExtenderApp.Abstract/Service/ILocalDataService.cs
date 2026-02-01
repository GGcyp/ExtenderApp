using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 本地数据服务：负责将任意类型的数据序列化/反序列化并持久化到本地存储（以及相应的异步版本）。
    /// 实现应在失败场景通过 <see cref="Result"/>/ <see cref="Result{T}"/> 返回错误信息或封装异常，而不是直接抛出未处理异常。
    /// </summary>
    public interface ILocalDataService
    {
        /// <summary>
        /// 同步从本地存储加载并反序列化指定文件的数据。
        /// </summary>
        /// <typeparam name="T">要加载的数据类型。</typeparam>
        /// <param name="fileName">文件名或相对于数据目录的路径。</param>
        /// <returns>
        /// 操作结果：成功时 <see cref="Result{T}.Value"/> 为反序列化得到的数据（文件不存在或内容为空时可能为 <c>null</c>）；失败时包含错误信息或异常。
        /// </returns>
        Result<T?> LoadData<T>(string fileName);

        /// <summary>
        /// 异步从本地存储加载并反序列化指定文件的数据。
        /// </summary>
        /// <typeparam name="T">要加载的数据类型。</typeparam>
        /// <param name="fileName">文件名或相对于数据目录的路径。</param>
        /// <param name="token">取消令牌。调用方可通过该令牌请求取消加载操作。</param>
        /// <returns>
        /// 异步操作结果：成功时返回包含反序列化数据的 <see cref="Result{T}"/>；失败时包含错误信息或异常。
        /// </returns>
        ValueTask<Result<T?>> LoadDataAsync<T>(string fileName, CancellationToken token = default);

        /// <summary>
        /// 同步将数据序列化并保存到本地存储。
        /// </summary>
        /// <typeparam name="T">要保存的数据类型。</typeparam>
        /// <param name="fileName">目标文件名或相对于数据目录的路径。</param>
        /// <param name="data">待保存的数据实例。</param>
        /// <param name="compressionType">可选的压缩策略（默认为 <see cref="CompressionType.Block"/>）。</param>
        /// <returns>操作结果：成功或失败（失败时包含错误信息或异常）。</returns>
        Result SaveData<T>(string fileName, T data, CompressionType compressionType = CompressionType.Block);

        /// <summary>
        /// 异步将数据序列化并保存到本地存储。
        /// </summary>
        /// <typeparam name="T">要保存的数据类型。</typeparam>
        /// <param name="fileName">目标文件名或相对于数据目录的路径。</param>
        /// <param name="data">待保存的数据实例。</param>
        /// <param name="token">取消令牌。调用方可通过该令牌请求取消保存操作。</param>
        /// <param name="compressionType">可选的压缩策略（默认为 <see cref="CompressionType.Block"/>）。</param>
        /// <returns>异步操作结果：成功或失败（失败时包含错误信息或异常）。</returns>
        ValueTask<Result> SaveDataAsync<T>(string fileName, T data, CancellationToken token = default, CompressionType compressionType = CompressionType.Block);
    }
}