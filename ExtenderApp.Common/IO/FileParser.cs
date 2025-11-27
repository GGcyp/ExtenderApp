using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.FileParsers
{
    /// <summary>
    /// 文件解析器基类，继承自 DisposableObject 类并实现 IFileParser 接口。
    /// </summary>
    public abstract class FileParser : DisposableObject, IFileParser
    {
        /// <summary>
        /// 文件存储的实例
        /// </summary>
        protected readonly IFileOperateProvider _provider;

        /// <summary>
        /// 解析器所针对的文件扩展名。
        /// </summary>
        /// <remarks>
        /// 用于由 <see cref="ExpectLocalFileInfo"/> 生成实际的路径/操作信息。是否包含“。”由实现与调用方约定保持一致。
        /// </remarks>
        protected abstract string FileExtension { get; }

        /// <summary>
        /// FileParser 类的受保护构造函数。
        /// </summary>
        /// <param name="provider">文件操作提供者。</param>
        protected FileParser(IFileOperateProvider provider)
        {
            _provider = provider;
        }

        #region Read

        public T? Read<T>(ExpectLocalFileInfo info)
        {
            return Read<T>(info.CreateReadWriteOperate(FileExtension));
        }

        public T? Read<T>(FileOperateInfo info)
        {
            return Read<T>(GetOperate(info));
        }

        public T? Read<T>(IFileOperate fileOperate)
        {
            CheckFileOperate(fileOperate);
            return ExecuteRead<T>(fileOperate);
        }

        public T? Read<T>(ExpectLocalFileInfo info, long position, int length)
        {
            return Read<T>(info.CreateReadWriteOperate(FileExtension), position, length);
        }

        public T? Read<T>(FileOperateInfo info, long position, int length)
        {
            return Read<T>(GetOperate(info), position, length);
        }

        public T? Read<T>(IFileOperate fileOperate, long position, int length)
        {
            CheckFileOperate(fileOperate);
            return ExecuteRead<T>(fileOperate, position, length);
        }

        #endregion Read

        #region ReadAsync

        public Task<T?> ReadAsync<T>(ExpectLocalFileInfo info, CancellationToken token = default)
        {
            return ReadAsync<T>(info.CreateReadWriteOperate(FileExtension), token);
        }

        public Task<T?> ReadAsync<T>(FileOperateInfo info, CancellationToken token = default)
        {
            return ReadAsync<T>(GetOperate(info), token);
        }

        public Task<T?> ReadAsync<T>(IFileOperate fileOperate, CancellationToken token = default)
        {
            CheckFileOperate(fileOperate);
            return ExecuteReadAsync<T>(fileOperate, token);
        }

        public Task<T?> ReadAsync<T>(ExpectLocalFileInfo info, long position, int length, CancellationToken token = default)
        {
            return ReadAsync<T>(info.CreateReadWriteOperate(FileExtension), position, length, token);
        }

        public Task<T?> ReadAsync<T>(FileOperateInfo info, long position, int length, CancellationToken token = default)
        {
            return ReadAsync<T>(GetOperate(info), position, length, token);
        }

        public Task<T?> ReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token = default)
        {
            CheckFileOperate(fileOperate);
            return ExecuteReadAsync<T>(fileOperate, position, length, token);
        }

        #endregion ReadAsync

        #region Write

        public void Write<T>(ExpectLocalFileInfo info, T value)
        {
            Write(info.CreateReadWriteOperate(FileExtension), value);
        }

        public void Write<T>(FileOperateInfo info, T value)
        {
            Write(GetOperate(info), value);
        }

        public void Write<T>(IFileOperate fileOperate, T value)
        {
            CheckFileOperate(fileOperate);
            ExecuteWrite(fileOperate, value);
        }

        public void Write<T>(ExpectLocalFileInfo info, T value, long position)
        {
            Write(info.CreateReadWriteOperate(FileExtension), value, position);
        }

        public void Write<T>(FileOperateInfo info, T value, long position)
        {
            Write(GetOperate(info), value, position);
        }

        public void Write<T>(IFileOperate fileOperate, T value, long position)
        {
            CheckFileOperate(fileOperate);
            ExecuteWrite(fileOperate, value, position);
        }

        #endregion Write

        #region WriteAsync

        public Task WriteAsync<T>(ExpectLocalFileInfo info, T value, CancellationToken token = default)
        {
            return WriteAsync(info.CreateReadWriteOperate(FileExtension), value, token);
        }

        public Task WriteAsync<T>(FileOperateInfo info, T value, CancellationToken token = default)
        {
            return WriteAsync(GetOperate(info), value, token);
        }

        public Task WriteAsync<T>(IFileOperate fileOperate, T value, CancellationToken token = default)
        {
            CheckFileOperate(fileOperate);
            return ExecuteWriteAsync(fileOperate, value, token);
        }

        public Task WriteAsync<T>(ExpectLocalFileInfo info, T value, long position, CancellationToken token = default)
        {
            return WriteAsync(info.CreateReadWriteOperate(FileExtension), value, position, token);
        }

        public Task WriteAsync<T>(FileOperateInfo info, T value, long position, CancellationToken token = default)
        {
            return WriteAsync(GetOperate(info), value, position, token);
        }

        public Task WriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default)
        {
            CheckFileOperate(fileOperate);
            return ExecuteWriteAsync(fileOperate, value, position, token);
        }

        #endregion WriteAsync

        #region GetOperate

        /// <summary>
        /// 根据给定的文件操作信息获取对应的文件并发操作对象。
        /// </summary>
        /// <param name="info">文件操作信息对象。</param>
        /// <returns>返回对应的文件并发操作对象。</returns>
        public IFileOperate GetOperate(FileOperateInfo info)
        {
            return _provider.GetOperate(info);
        }

        #endregion GetOperate

        #region Execute

        #region ExecuteRead

        /// <summary>
        /// 执行实际读取并反序列化（同步，默认范围）。
        /// </summary>
        /// <typeparam name="T">反序列化后的类型。</typeparam>
        /// <param name="fileOperate">底层文件并发操作对象。</param>
        /// <returns>反序列化后的对象；失败或内容为空返回 null。</returns>
        /// <remarks>实现应定义“默认读取范围”的含义（如整文件或固定区间）。</remarks>
        protected abstract T? ExecuteRead<T>(IFileOperate fileOperate);

        /// <summary>
        /// 执行实际读取并反序列化（同步，指定区间）。
        /// </summary>
        /// <typeparam name="T">反序列化后的类型。</typeparam>
        /// <param name="fileOperate">底层文件并发操作对象。</param>
        /// <param name="position">读取起始字节偏移（≥ 0）。</param>
        /// <param name="length">读取字节长度（≥ 0）。</param>
        /// <returns>反序列化后的对象；失败或数据不足返回 null。</returns>
        protected abstract T? ExecuteRead<T>(IFileOperate fileOperate, long position, int length);

        /// <summary>
        /// 执行实际读取并反序列化（异步，默认范围）。
        /// </summary>
        /// <typeparam name="T">反序列化后的类型。</typeparam>
        /// <param name="fileOperate">底层文件并发操作对象。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败或内容为空返回 null。</returns>
        protected abstract Task<T?> ExecuteReadAsync<T>(IFileOperate fileOperate, CancellationToken token);

        /// <summary>
        /// 执行实际读取并反序列化（异步，指定区间）。
        /// </summary>
        /// <typeparam name="T">反序列化后的类型。</typeparam>
        /// <param name="fileOperate">底层文件并发操作对象。</param>
        /// <param name="position">读取起始字节偏移（≥ 0）。</param>
        /// <param name="length">读取字节长度（≥ 0）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>反序列化后的对象；失败或数据不足返回 null。</returns>
        protected abstract Task<T?> ExecuteReadAsync<T>(IFileOperate fileOperate, long position, int length, CancellationToken token);

        #endregion ExecuteRead

        #region ExecuteWrite

        /// <summary>
        /// 执行实际序列化并写入（同步，位置策略由实现定义）。
        /// </summary>
        /// <typeparam name="T">要写入值的类型。</typeparam>
        /// <param name="fileOperate">底层文件并发操作对象。</param>
        /// <param name="value">要写入的值。</param>
        protected abstract void ExecuteWrite<T>(IFileOperate fileOperate, T value);

        /// <summary>
        /// 执行实际序列化并写入（同步，指定起始偏移）。
        /// </summary>
        /// <typeparam name="T">要写入值的类型。</typeparam>
        /// <param name="fileOperate">底层文件并发操作对象。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">写入起始字节偏移（≥ 0）。</param>
        protected abstract void ExecuteWrite<T>(IFileOperate fileOperate, T value, long position);

        /// <summary>
        /// 执行实际序列化并写入（异步，位置策略由实现定义）。
        /// </summary>
        /// <typeparam name="T">要写入值的类型。</typeparam>
        /// <param name="fileOperate">底层文件并发操作对象。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示写入过程的任务。</returns>
        protected abstract Task ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, CancellationToken token = default);

        /// <summary>
        /// 执行实际序列化并写入（异步，指定起始偏移）。
        /// </summary>
        /// <typeparam name="T">要写入值的类型。</typeparam>
        /// <param name="fileOperate">底层文件并发操作对象。</param>
        /// <param name="value">要写入的值。</param>
        /// <param name="position">写入起始字节偏移（≥ 0）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示写入过程的任务。</returns>
        protected abstract Task ExecuteWriteAsync<T>(IFileOperate fileOperate, T value, long position, CancellationToken token = default);

        #endregion ExecuteWrite

        #endregion Execute

        #region Operate

        /// <summary>
        /// 删除由期望文件信息与当前扩展名组合得到的目标文件。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <remarks>应尽量保持幂等：文件不存在时不视为错误。</remarks>
        public void Delete(ExpectLocalFileInfo info)
        {
            info.CreatLocalFileInfo(FileExtension).Delete();
        }

        /// <summary>
        /// 基于期望文件信息与当前扩展名创建并返回文件操作对象。
        /// </summary>
        /// <param name="info">期望的本地文件信息。</param>
        /// <returns>可用于并发读写的 <see cref="IFileOperate"/> 实例。</returns>
        public IFileOperate GetFileOperate(ExpectLocalFileInfo info)
        {
            return _provider.GetOperate(info.CreateReadWriteOperate(FileExtension));
        }

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