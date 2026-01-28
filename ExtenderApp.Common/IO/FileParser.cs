using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.FileParsers
{
    /// <summary>
    /// 文件解析器基类，继承自 <see cref="DisposableObject"/> 并实现 <see cref="IFileParser"/> 接口。
    /// 提供基于文件扩展名的一致性适配：大多数重载将通过 <see cref="FileExtension"/> 构建实际的文件操作上下文并委派到抽象执行方法。
    /// </summary>
    public abstract class FileParser : DisposableObject, IFileParser
    {
        /// <summary>
        /// 文件操作提供者，用于获取并发安全的 <see cref="IFileOperate"/> 实例。
        /// </summary>
        protected readonly IFileOperateProvider _provider;

        /// <summary>
        /// 解析器所针对的文件扩展名（实现可决定是否包含开头的点）。
        /// 用于由 <see cref="ExpectLocalFileInfo"/> / <see cref="FileOperateInfo"/> 生成实际的路径/操作信息。
        /// </summary>
        protected abstract string FileExtension { get; }

        /// <summary>
        /// 初始化 <see cref="FileParser"/> 实例。
        /// </summary>
        /// <param name="provider">用于获取 <see cref="IFileOperate"/> 的提供者，不能为空。</param>
        /// <exception cref="ArgumentNullException"><paramref name="provider"/> 为 <c>null</c> 时抛出。</exception>
        protected FileParser(IFileOperateProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        #region Read

        /// <summary>
        /// 读取指定期望文件信息指向的文件并反序列化为 <typeparamref name="T"/>（使用默认读取行为）。
        /// </summary>
        public Result<T?> Read<T>(ExpectLocalFileInfo info)
        {
            return Read<T>(info.CreateReadWriteOperate(FileExtension));
        }

        /// <summary>
        /// 读取使用指定文件操作信息表示的文件并反序列化为 <typeparamref name="T"/>（使用默认读取行为）。
        /// </summary>
        public Result<T?> Read<T>(FileOperateInfo info)
        {
            return Read<T>(GetFileOperate(info));
        }

        /// <summary>
        /// 使用指定的 <see cref="IFileOperate"/> 实例读取并反序列化为 <typeparamref name="T"/>（使用默认读取行为）。
        /// </summary>
        public Result<T?> Read<T>(IFileOperate fileOperate)
        {
            CheckFileOperate(fileOperate);
            return ExecuteRead<T>(fileOperate, 0, (int)fileOperate.Info.Length);
        }

        /// <summary>
        /// 从期望文件信息指向的文件的指定区间读取并反序列化为 <typeparamref name="T"/>。
        /// </summary>
        public Result<T?> Read<T>(ExpectLocalFileInfo info, long position, int length)
        {
            return Read<T>(info.CreateReadWriteOperate(FileExtension), position, length);
        }

        /// <summary>
        /// 从指定文件操作信息表示的文件的指定区间读取并反序列化为 <typeparamref name="T"/>。
        /// </summary>
        public Result<T?> Read<T>(FileOperateInfo info, long position, int length)
        {
            return Read<T>(GetFileOperate(info), position, length);
        }

        /// <summary>
        /// 使用指定的 <see cref="IFileOperate"/> 实例从指定区间读取并反序列化为 <typeparamref name="T"/>。
        /// </summary>
        public Result<T?> Read<T>(IFileOperate fileOperate, long position, int length)
        {
            CheckFileOperate(fileOperate);
            return ExecuteRead<T>(fileOperate, position, length);
        }

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步读取指定期望文件信息指向的文件并反序列化为 <typeparamref name="T"/>（使用默认读取行为）。
        /// </summary>
        public ValueTask<Result<T?>> ReadAsync<T>(ExpectLocalFileInfo info, CancellationToken token = default)
        {
            return ReadAsync<T>(info.CreateReadWriteOperate(FileExtension), token);
        }

        /// <summary>
        /// 异步读取使用指定文件操作信息表示的文件并反序列化为 <typeparamref name="T"/>（使用默认读取行为）。
        /// </summary>
        public ValueTask<Result<T?>> ReadAsync<T>(FileOperateInfo info, CancellationToken token = default)
        {
            return ReadAsync<T>(GetFileOperate(info), token);
        }

        /// <summary>
        /// 异步使用指定的 <see cref="IFileOperate"/> 实例读取并反序列化为 <typeparamref name="T"/>（使用默认读取行为）。
        /// </summary>
        public ValueTask<Result<T?>> ReadAsync<T>(IFileOperate fileOperate, CancellationToken token = default)
        {
            CheckFileOperate(fileOperate);
            return ExecuteReadAsync<T>(fileOperate, 0, (int)fileOperate.Info.Length, token);
        }

        /// <summary>
        /// 异步从期望文件信息指向的文件的指定区间读取并反序列化为 <typeparamref name="T"/>。
        /// </summary>
        public ValueTask<Result<T?>> ReadAsync<T>(ExpectLocalFileInfo info, long position, int length, CancellationToken token = default)
        {
            return ReadAsync<T>(info.CreateReadWriteOperate(FileExtension), position, length, token);
        }

        /// <summary>
        /// 异步从指定文件操作信息表示的文件的指定区间读取并反序列化为 <typeparamref name="T"/>。
        /// </summary>
        public ValueTask<Result<T?>> ReadAsync<T>(FileOperateInfo info, long position, int length, CancellationToken token = default)
        {
            return ReadAsync<T>(GetFileOperate(info), position, length, token);
        }

        /// <summary>
        /// 异步使用指定的 <see cref="IFileOperate"/> 实例从指定区间读取并反序列化为 <typeparamref name="T"/>。
        /// </summary>
        public ValueTask<Result<T?>> ReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token = default)
        {
            CheckFileOperate(fileOperate);
            return ExecuteReadAsync<T>(fileOperate, position, length, token);
        }

        #endregion ReadAsync

        #region Write

        /// <summary>
        /// 将值序列化并写入由期望文件信息表示的目标文件（位置策略由实现定义）。
        /// </summary>
        public Result Write<T>(ExpectLocalFileInfo info, T value)
        {
            return Write(info.CreateReadWriteOperate(FileExtension), value);
        }

        /// <summary>
        /// 将值序列化并写入由文件操作信息表示的目标文件（位置策略由实现定义）。
        /// </summary>
        public Result Write<T>(FileOperateInfo info, T value)
        {
            return Write(GetFileOperate(info), value);
        }

        /// <summary>
        /// 使用指定的 <see cref="IFileOperate"/> 实例将值序列化并写入（位置策略由实现定义）。
        /// </summary>
        public Result Write<T>(IFileOperate fileOperate, T value)
        {
            CheckFileOperate(fileOperate);
            return ExecuteWrite(fileOperate, value, 0);
        }

        /// <summary>
        /// 将值序列化并写入到由期望文件信息表示的文件的指定偏移位置。
        /// </summary>
        public Result Write<T>(ExpectLocalFileInfo info, T value, long position)
        {
            return Write(info.CreateReadWriteOperate(FileExtension), value, position);
        }

        /// <summary>
        /// 将值序列化并写入到由文件操作信息表示的文件的指定偏移位置。
        /// </summary>
        public Result Write<T>(FileOperateInfo info, T value, long position)
        {
            return Write(GetFileOperate(info), value, position);
        }

        /// <summary>
        /// 使用指定的 <see cref="IFileOperate"/> 实例将值序列化并写入到指定偏移位置。
        /// </summary>
        public Result Write<T>(IFileOperate fileOperate, T value, long position)
        {
            CheckFileOperate(fileOperate);
            return ExecuteWrite(fileOperate, value, position);
        }

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步将值序列化并写入由期望文件信息表示的目标文件（位置策略由实现定义）。
        /// </summary>
        public ValueTask<Result> WriteAsync<T>(ExpectLocalFileInfo info, T value, CancellationToken token = default)
        {
            return WriteAsync(info.CreateReadWriteOperate(FileExtension), value, token);
        }

        /// <summary>
        /// 异步将值序列化并写入由文件操作信息表示的目标文件（位置策略由实现定义）。
        /// </summary>
        public ValueTask<Result> WriteAsync<T>(FileOperateInfo info, T value, CancellationToken token = default)
        {
            return WriteAsync(GetFileOperate(info), value, token);
        }

        /// <summary>
        /// 异步使用指定的 <see cref="IFileOperate"/> 实例将值序列化并写入（位置策略由实现定义）。
        /// </summary>
        public ValueTask<Result> WriteAsync<T>(IFileOperate fileOperate, T value, CancellationToken token = default)
        {
            CheckFileOperate(fileOperate);
            return ExecuteWriteAsync(fileOperate, value, 0, token);
        }

        /// <summary>
        /// 异步将值序列化并写入到由期望文件信息表示的文件的指定偏移位置。
        /// </summary>
        public ValueTask<Result> WriteAsync<T>(ExpectLocalFileInfo info, T value, long position, CancellationToken token = default)
        {
            return WriteAsync(info.CreateReadWriteOperate(FileExtension), value, position, token);
        }

        /// <summary>
        /// 异步将值序列化并写入到由文件操作信息表示的文件的指定偏移位置。
        /// </summary>
        public ValueTask<Result> WriteAsync<T>(FileOperateInfo info, T value, long position, CancellationToken token = default)
        {
            return WriteAsync(GetFileOperate(info), value, position, token);
        }

        /// <summary>
        /// 异步使用指定的 <see cref="IFileOperate"/> 实例将值序列化并写入到指定偏移位置。
        /// </summary>
        public ValueTask<Result> WriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default)
        {
            CheckFileOperate(fileOperate);
            return ExecuteWriteAsync(fileOperate, value, position, token);
        }

        #endregion WriteAsync

        #region Execute

        #region ExecuteRead

        /// <summary>
        /// 执行实际读取并反序列化（同步，指定区间）。由派生类实现具体行为。
        /// </summary>
        protected abstract Result<T?> ExecuteRead<T>(IFileOperate fileOperate, long position, int length);

        /// <summary>
        /// 执行实际读取并反序列化（异步，指定区间）。由派生类实现具体行为。
        /// </summary>
        protected abstract ValueTask<Result<T?>> ExecuteReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token);

        #endregion ExecuteRead

        #region ExecuteWrite

        /// <summary>
        /// 执行实际序列化并写入（同步，指定起始偏移）。由派生类实现具体行为。
        /// </summary>
        protected abstract Result ExecuteWrite<T>(IFileOperate fileOperate, T value, long position);

        /// <summary>
        /// 执行实际序列化并写入（异步，指定起始偏移）。由派生类实现具体行为。
        /// </summary>
        protected abstract ValueTask<Result> ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default);

        #endregion ExecuteWrite

        #endregion Execute

        #region Operate

        /// <summary>
        /// 删除由期望文件信息与当前扩展名组合得到的目标文件。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <returns>操作结果，成功时返回成功的 <see cref="Result"/>。</returns>
        public Result Delete(ExpectLocalFileInfo info)
        {
            info.CreatLocalFileInfo(FileExtension).Delete();
            return Result.Success();
        }

        /// <summary>
        /// 基于期望文件信息与当前扩展名创建并返回文件操作对象。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <returns>可用于并发读写的 <see cref="IFileOperate"/> 实例（调用方负责遵守生命周期约定）。</returns>
        public IFileOperate GetFileOperate(ExpectLocalFileInfo info)
        {
            return GetFileOperate(info.CreateReadWriteOperate(FileExtension));
        }

        /// <summary>
        /// 根据给定的 <see cref="FileOperateInfo"/> 获取对应的并发文件操作对象。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <returns>对应的 <see cref="IFileOperate"/> 实例（调用方应遵守其生命周期约定）。</returns>
        public IFileOperate GetFileOperate(FileOperateInfo info)
        {
            return _provider.GetOperate(info);
        }

        /// <summary>
        /// 检查 <see cref="IFileOperate"/> 是否为 null，并在为 null 时抛出 <see cref="ArgumentNullException"/>。
        /// </summary>
        /// <param name="fileOperate">要检查的 <see cref="IFileOperate"/> 实例。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="fileOperate"/> 为 <c>null</c> 时抛出。</exception>
        private void CheckFileOperate(IFileOperate fileOperate)
        {
            if (fileOperate == null)
            {
                throw new ArgumentNullException(nameof(fileOperate));
            }
        }

        #endregion Operate
    }
}