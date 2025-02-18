using System.IO.MemoryMappedFiles;
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
    public abstract class FileParser : FileParser<FileConcurrentOperatePolicy, FileConcurrentOperateData>
    {
        protected override FileConcurrentOperatePolicy Policy { get; }

        /// <summary>
        /// 初始化 FileParser 类的新实例。
        /// </summary>
        /// <param name="store">文件存储对象</param>
        protected FileParser(FileStore store) : base(store)
        {
            Policy = new FileConcurrentOperatePolicy();
        }

    }

    /// <summary>
    /// 文件解析抽象类
    /// </summary>
    public abstract class FileParser<TPolicy, TData> : DisposableObject, IFileParser
        where TPolicy : class, IConcurrentOperatePolicy<MemoryMappedViewAccessor, TData>
        where TData : FileConcurrentOperateData
    {
        /// <summary>
        /// 文件存储的实例
        /// </summary>
        protected readonly FileStore _store;

        /// <summary>
        /// 用于取消操作的取消令牌源。
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 获取策略对象属性，该属性为抽象属性，必须由派生类实现。
        /// </summary>
        /// <returns>返回策略对象</returns>
        protected abstract TPolicy Policy { get; }

        /// <summary>
        /// FileParser 类的受保护构造函数。
        /// </summary>
        /// <param name="store">文件存储对象</param>
        protected FileParser(FileStore store)
        {
            /// <summary>
            /// 初始化文件存储对象
            /// </summary>
            _store = store;
            /// <summary>
            /// 初始化取消令牌源
            /// </summary>
            _cancellationTokenSource = new CancellationTokenSource();
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


        /// <summary>
        /// 获取指定文件的并发操作实例。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <param name="fileOperate">文件操作实例，可以为null。</param>
        /// <param name="fileLength">文件长度，默认为-1表示自动计算。</param>
        /// <returns>返回一个实现了IConcurrentOperate<MemoryMappedViewAccessor, TData>接口的文件操作实例。</returns>
        /// <exception cref="InvalidOperationException">当无法找到文件处理类时抛出。</exception>
        protected IConcurrentOperate<MemoryMappedViewAccessor, TData> GetOperate(FileOperateInfo info, object? fileOperate, long fileLength = 0)
        {
            if (!(fileOperate != null && fileOperate is IConcurrentOperate<MemoryMappedViewAccessor, TData> operate))
            {
                var data = Policy.GetData();
                operate = _store.GetOperate(info, CreateOperate, fileLength);
                if (operate == null)
                {
                    throw new InvalidOperationException(string.Format("查找文件处理类出问题了：{0}", info.LocalFileInfo));
                }
            }
            return operate;
        }

        /// <summary>
        /// 创建一个并发操作对象。
        /// </summary>
        /// <returns>返回一个并发操作对象。</returns>
        private IConcurrentOperate<MemoryMappedViewAccessor, TData> CreateOperate(FileOperateInfo info, long fileLength)
        {
            var result = ConcurrentOperate<TPolicy, MemoryMappedViewAccessor, TData>.Get();
            var data = Policy.GetData();
            data.OpenFile(info, fileLength, _cancellationTokenSource.Token, _store.ReleaseOperate);
            result.SetPolicyAndData(Policy, data);
            return result;
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
