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
        /// 用于取消操作的取消令牌源。
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// FileParser 类的受保护构造函数。
        /// </summary>
        /// <param name="store">文件存储对象</param>
        protected FileParser(IFileOperateProvider provider)
        {
            /// <summary>
            /// 初始化文件存储对象
            /// </summary>
            _provider = provider;
            /// <summary>
            /// 初始化取消令牌源
            /// </summary>
            _cancellationTokenSource = new CancellationTokenSource();
        }

        #region Read
        public abstract T? Read<T>(ExpectLocalFileInfo info);

        public abstract T? Read<T>(FileOperateInfo info);

        public abstract T? Read<T>(IFileOperate fileOperate);

        public abstract T? Read<T>(ExpectLocalFileInfo info, long position, int length);

        public abstract T? Read<T>(FileOperateInfo info, long position, int length);

        public abstract T? Read<T>(IFileOperate fileOperate, long position, int length);

        #endregion

        #region ReadAsync

        public abstract Task<T?> ReadAsync<T>(ExpectLocalFileInfo info);

        public abstract Task<T?> ReadAsync<T>(FileOperateInfo info);

        public abstract Task<T?> ReadAsync<T>(IFileOperate fileOperate);

        public abstract Task<T?> ReadAsync<T>(ExpectLocalFileInfo info, long position, int length);

        public abstract Task<T?> ReadAsync<T>(FileOperateInfo info, long position, int length);

        public abstract Task<T?> ReadAsync<T>(IFileOperate fileOperate, long position, int length);

        #endregion

        #region Write
        public abstract void Write<T>(ExpectLocalFileInfo info, T value);

        public abstract void Write<T>(FileOperateInfo info, T value);

        public abstract void Write<T>(IFileOperate fileOperate, T value);

        public abstract void Write<T>(ExpectLocalFileInfo info, T value, long position);

        public abstract void Write<T>(FileOperateInfo info, T value, long position);

        public abstract void Write<T>(IFileOperate fileOperate, T value, long position);

        #endregion

        #region WriteAsync

        public abstract Task WriteAsync<T>(ExpectLocalFileInfo info, T value);

        public abstract Task WriteAsync<T>(FileOperateInfo info, T value);

        public abstract Task WriteAsync<T>(IFileOperate fileOperate, T value);

        public abstract Task WriteAsync<T>(ExpectLocalFileInfo info, T value, long position);

        public abstract Task WriteAsync<T>(FileOperateInfo info, T value, long position);

        public abstract Task WriteAsync<T>(IFileOperate fileOperate, T value, long position);

        #endregion

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

        #endregion

        #region Delete

        /// <summary>
        /// 删除本地文件
        /// </summary>
        /// <param name="info">包含要删除文件信息的ExpectLocalFileInfo对象</param>
        public abstract void Delete(ExpectLocalFileInfo info);

        public void Delete(LocalFileInfo info)
        {
            //_provider.Delete(info);
            info.Delete();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            base.Dispose(disposing);
        }
    }
}
