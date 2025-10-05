using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义“文件 ⇄ 对象”的序列化/反序列化契约（同步与异步）。
    /// </summary>
    /// <remarks>
    /// 约定：
    /// - 偏移与长度均以字节为单位。<br/>
    /// - 读取方法：从文件读取原始字节并反序列化为 T；失败或内容为空可返回 null。<br/>
    /// - 写入方法：将值序列化为字节并写入到实现定义的位置/区间（覆盖或追加策略由实现决定）。<br/>
    /// - 并发与线程安全由具体实现决定；结合 <see cref="IFileOperate"/> 可实现更细粒度控制。<br/>
    /// - 常见异常（如 IOException、UnauthorizedAccessException）由具体实现决定是否抛出。
    /// </remarks>
    public interface IFileParser
    {
        #region Read

        /// <summary>
        /// 读取文件全部（或实现定义的默认范围）并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <returns>反序列化后的对象；失败、文件为空或格式不匹配时返回 null。</returns>
        T? Read<T>(ExpectLocalFileInfo info);

        /// <summary>
        /// 读取文件全部（或实现定义的默认范围）并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="info">文件操作信息（包含路径/模式/访问权限）。</param>
        /// <returns>反序列化后的对象；失败、文件为空或格式不匹配时返回 null。</returns>
        T? Read<T>(FileOperateInfo info);

        /// <summary>
        /// 读取文件全部（或实现定义的默认范围）并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <returns>反序列化后的对象；失败、文件为空或格式不匹配时返回 null。</returns>
        T? Read<T>(IFileOperate fileOperate);

        /// <summary>
        /// 从指定字节偏移读取指定长度的数据，并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="position">读取起始字节偏移（≥ 0）。</param>
        /// <param name="length">读取字节长度（≥ 0）。</param>
        /// <returns>反序列化后的对象；失败或数据不足时返回 null。</returns>
        T? Read<T>(ExpectLocalFileInfo info, long position, int length);

        /// <summary>
        /// 从指定字节偏移读取指定长度的数据，并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="info">文件操作信息（包含路径/模式/访问权限）。</param>
        /// <param name="position">读取起始字节偏移（≥ 0）。</param>
        /// <param name="length">读取字节长度（≥ 0）。</param>
        /// <returns>反序列化后的对象；失败或数据不足时返回 null。</returns>
        T? Read<T>(FileOperateInfo info, long position, int length);

        /// <summary>
        /// 从指定字节偏移读取指定长度的数据，并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="position">读取起始字节偏移（≥ 0）。</param>
        /// <param name="length">读取字节长度（≥ 0）。</param>
        /// <returns>反序列化后的对象；失败或数据不足时返回 null。</returns>
        T? Read<T>(IFileOperate fileOperate, long position, int length);

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步读取文件全部（或实现定义的默认范围）并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败、文件为空或格式不匹配时返回 null。</returns>
        /// <remarks>提供多种重载以支持不同上下文与读取范围。</remarks>
        Task<T?> ReadAsync<T>(ExpectLocalFileInfo info, CancellationToken token = default);

        /// <summary>
        /// 异步读取文件全部（或实现定义的默认范围）并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="info">文件操作信息（包含路径/模式/访问权限）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败、文件为空或格式不匹配时返回 null。</returns>
        Task<T?> ReadAsync<T>(FileOperateInfo info, CancellationToken token = default);

        /// <summary>
        /// 异步读取文件全部（或实现定义的默认范围）并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败、文件为空或格式不匹配时返回 null。</returns>
        Task<T?> ReadAsync<T>(IFileOperate fileOperate, CancellationToken token = default);

        /// <summary>
        /// 异步从指定字节偏移读取指定长度的数据，并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="position">读取起始字节偏移（≥ 0）。</param>
        /// <param name="length">读取字节长度（≥ 0）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败或数据不足时返回 null。</returns>
        Task<T?> ReadAsync<T>(ExpectLocalFileInfo info, long position, int length, CancellationToken token = default);

        /// <summary>
        /// 异步从指定字节偏移读取指定长度的数据，并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="info">文件操作信息（包含路径/模式/访问权限）。</param>
        /// <param name="position">读取起始字节偏移（≥ 0）。</param>
        /// <param name="length">读取字节长度（≥ 0）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败或数据不足时返回 null。</returns>
        Task<T?> ReadAsync<T>(FileOperateInfo info, long position, int length, CancellationToken token = default);

        /// <summary>
        /// 异步从指定字节偏移读取指定长度的数据，并反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">返回对象的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="position">读取起始字节偏移（≥ 0）。</param>
        /// <param name="length">读取字节长度（≥ 0）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败或数据不足时返回 null。</returns>
        Task<T?> ReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token = default);

        #endregion ReadAsync

        #region Write

        /// <summary>
        /// 将指定的值序列化后写入文件（位置策略由实现定义）。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="value">要写入的值。</param>
        void Write<T>(ExpectLocalFileInfo info, T value);

        /// <summary>
        /// 将指定的值序列化后写入文件（位置策略由实现定义）。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="info">文件操作信息（包含路径/模式/访问权限）。</param>
        /// <param name="value">要写入的值。</param>
        void Write<T>(FileOperateInfo info, T value);

        /// <summary>
        /// 将指定的值序列化后写入文件（位置策略由实现定义）。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="value">要写入的值。</param>
        void Write<T>(IFileOperate fileOperate, T value);

        /// <summary>
        /// 将指定的值序列化后写入到指定字节偏移位置。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">写入起始字节偏移（≥ 0）。</param>
        void Write<T>(ExpectLocalFileInfo info, T value, long position);

        /// <summary>
        /// 将指定的值序列化后写入到指定字节偏移位置。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="info">文件操作信息（包含路径/模式/访问权限）。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">写入起始字节偏移（≥ 0）。</param>
        void Write<T>(FileOperateInfo info, T value, long position);

        /// <summary>
        /// 将指定的值序列化后写入到指定字节偏移位置。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">写入起始字节偏移（≥ 0）。</param>
        void Write<T>(IFileOperate fileOperate, T value, long position);

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步将指定的值序列化后写入文件（位置策略由实现定义）。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(ExpectLocalFileInfo info, T value, CancellationToken token = default);

        /// <summary>
        /// 异步将指定的值序列化后写入文件（位置策略由实现定义）。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="info">文件操作信息（包含路径/模式/访问权限）。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(FileOperateInfo info, T value, CancellationToken token = default);

        /// <summary>
        /// 异步将指定的值序列化后写入文件（位置策略由实现定义）。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(IFileOperate fileOperate, T value, CancellationToken token = default);

        /// <summary>
        /// 异步将指定的值序列化后写入到指定字节偏移位置。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="info">期望的本地文件信息。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">写入起始字节偏移（≥ 0）。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(ExpectLocalFileInfo info, T value, long position, CancellationToken token = default);

        /// <summary>
        /// 异步将指定的值序列化后写入到指定字节偏移位置。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="info">文件操作信息（包含路径/模式/访问权限）。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">写入起始字节偏移（≥ 0）。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(FileOperateInfo info, T value, long position, CancellationToken token = default);

        /// <summary>
        /// 异步将指定的值序列化后写入到指定字节偏移位置。
        /// </summary>
        /// <typeparam name="T">值的类型。</typeparam>
        /// <param name="fileOperate">并发文件操作接口。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">写入起始字节偏移（≥ 0）。</param>
        /// <param name="token">取消令牌。</param>
        Task WriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default);

        #endregion WriteAsync

        #region Operate

        /// <summary>
        /// 删除由期望文件信息指向的文件。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <remarks>实现应尽量幂等：文件不存在时不应视为错误。</remarks>
        void Delete(ExpectLocalFileInfo info);

        /// <summary>
        /// 获取与期望文件信息对应的文件操作对象。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <returns>返回期望的本地文件</returns>
        IFileOperate GetFileOperate(ExpectLocalFileInfo info);

        #endregion Operate
    }
}