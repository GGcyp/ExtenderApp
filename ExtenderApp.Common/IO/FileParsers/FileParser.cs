using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.FileParsers
{
    /// <summary>
    /// 文件解析器基类，用于处理文件解析。
    /// </summary>
    /// <typeparam name="FileStreamConcurrentOperateData">文件流并发操作数据类型</typeparam>
    public abstract class FileParser : FileParser<FileStreamConcurrentOperatePolicy, FileStreamConcurrentOperateData>
    {
        protected override FileStreamConcurrentOperatePolicy Policy { get; }

        /// <summary>
        /// 初始化 FileParser 类的新实例。
        /// </summary>
        /// <param name="store">文件存储对象</param>
        protected FileParser(FileStore store) : base(store)
        {
            Policy = new FileStreamConcurrentOperatePolicy();
        }

    }

    /// <summary>
    /// 文件解析抽象类
    /// </summary>
    public abstract class FileParser<TPolicy, TData> : DisposableObject, IFileParser
        where TPolicy : class, IConcurrentOperatePolicy<FileStream, TData>
        where TData : FileStreamConcurrentOperateData
    {
        /// <summary>
        /// 文件存储的实例
        /// </summary>
        protected readonly FileStore _store;

        protected abstract TPolicy Policy { get; }

        protected FileParser(FileStore store)
        {
            _store = store;
        }

        #region Read

        /// <summary>
        /// 同步读取文件内容，返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        /// <returns>读取到的数据，如果读取失败则返回null</returns>
        public abstract T? Read<T>(ExpectLocalFileInfo info, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 同步读取文件内容，返回指定类型的数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        /// <returns>读取到的数据，如果读取失败则返回null</returns>
        public abstract T? Read<T>(FileOperateInfo info, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 异步读取文件内容，读取完成后调用回调函数
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="callback">回调函数，参数为读取到的数据</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        public abstract void ReadAsync<T>(ExpectLocalFileInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 异步读取文件内容，读取完成后调用回调函数
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="callback">回调函数，参数为读取到的数据</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        public abstract void ReadAsync<T>(FileOperateInfo info, Action<T>? callback, IConcurrentOperate fileOperate = null, object? options = null);

        #endregion

        #region Write

        /// <summary>
        /// 同步写入数据到文件
        /// </summary>
        /// <typeparam name="T">要写入的数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        public abstract void Write<T>(ExpectLocalFileInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 同步写入数据到文件
        /// </summary>
        /// <typeparam name="T">要写入的数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        public abstract void Write<T>(FileOperateInfo info, T value, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 异步写入数据到文件，写入完成后调用回调函数
        /// </summary>
        /// <typeparam name="T">要写入的数据类型</typeparam>
        /// <param name="info">文件信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="callback">回调函数，无参数</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        public abstract void WriteAsync<T>(ExpectLocalFileInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null);

        /// <summary>
        /// 异步写入数据到文件，写入完成后调用回调函数
        /// </summary>
        /// <typeparam name="T">要写入的数据类型</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="value">要写入的数据</param>
        /// <param name="callback">回调函数，无参数</param>
        /// <param name="fileOperate">文件操作对象，可以为null</param>
        /// <param name="options">操作选项，可以为null</param>
        public abstract void WriteAsync<T>(FileOperateInfo info, T value, Action? callback = null, IConcurrentOperate fileOperate = null, object? options = null);

        #endregion

        #region GetOperate

        protected IConcurrentOperate<FileStream, TData> GetOperate(FileOperateInfo info, object? fileOperate)
        {
            if (!(fileOperate != null && fileOperate is IConcurrentOperate<FileStream, TData> operate))
            {

                operate = _store.GetOperate(info, ConcurrentOperate<TPolicy, FileStream, TData>.Get);
                operate.Start(Policy);
                if (operate == null)
                {
                    throw new InvalidOperationException(string.Format("查找文件处理类出问题了：{0}", info.LocalFileInfo));
                }
            }
            return operate;
        }

        #endregion
    }
}
