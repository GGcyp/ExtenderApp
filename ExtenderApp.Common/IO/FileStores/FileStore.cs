using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件存储类，用于管理文件操作的并发处理。
    /// </summary>
    public class FileStore : DisposableObject
    {
        /// <summary>
        /// 文件操作字典，用于存储文件操作信息。
        /// </summary>
        private readonly ConcurrentDictionary<FileOperateInfo, IConcurrentOperate> _operateDict;

        /// <summary>
        /// 用于取消操作的取消令牌源。
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 一个私有的 ScheduledTask 对象。
        /// </summary>
        private readonly ScheduledTask _task;

        private readonly List<FileOperateInfo> _operateList;

        /// <summary>
        /// 初始化 FileStorage 类的新实例。
        /// </summary>
        /// <param name="operatePool">并发操作池实例。</param>
        public FileStore()
        {
            _operateDict = new();
            _cancellationTokenSource = new();
            _operateList = new();
            _task = new();
            _task.StartCycle(o => ReleaseOperate(), TimeSpan.FromSeconds(60));
        }

        /// <summary>
        /// 获取并发操作对象
        /// </summary>
        /// <typeparam name="T">并发操作数据的类型，必须是FileStreamConcurrentOperateData的子类</typeparam>
        /// <param name="info">文件操作信息</param>
        /// <param name="data">并发操作数据，默认为null</param>
        /// <param name="createFunc">用于创建并发操作对象的函数，默认为null</param>
        /// <returns>并发操作对象</returns>
        /// <exception cref="ArgumentNullException">如果data或createFunc为null，则抛出此异常</exception>
        public IConcurrentOperate<FileStream, T>? GetOperate<T>(FileOperateInfo info, Func<IConcurrentOperate<FileStream, T>> createFunc) where T : FileStreamConcurrentOperateData
        {
            if (_operateDict.TryGetValue(info, out var operate))
            {
                return operate as IConcurrentOperate<FileStream, T>;
            }

            lock (_operateDict)
            {
                if (_operateDict.TryGetValue(info, out operate))
                {
                    return operate as IConcurrentOperate<FileStream, T>;
                }

                createFunc.ArgumentNull();

                var result = createFunc?.Invoke();
                result.Data.OpenFile(info, _cancellationTokenSource.Token, ReleaseOperate);
                _operateDict.TryAdd(info, result);
                _task.Resume();
                return result;
            }
        }

        /// <summary>
        /// 释放指定的文件操作对象。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        public void ReleaseOperate(FileOperateInfo info)
        {
            if (!_operateDict.TryRemove(info, out var operate))
            {
                return;
            }

            operate.Release();
        }

        /// <summary>
        /// 释放操作
        /// </summary>
        /// <remarks>
        /// 该方法会遍历操作字典，将不在执行中的操作添加到操作列表中，然后依次释放这些操作，并清空操作列表。
        /// </remarks>
        private void ReleaseOperate()
        {
            if (_operateDict.Count <= 0)
                _task.Pause();

            foreach (var pair in _operateDict)
            {
                if (!pair.Value.IsExecuting)
                {
                    _operateList.Add(pair.Key);
                }
            }

            for (int i = 0; i < _operateList.Count; i++)
            {
                ReleaseOperate(_operateList[i]);
            }
            _operateList.Clear();

            if (_operateDict.Count <= 0)
                _task.Pause();
        }

        /// <summary>
        /// 释放 FileStorage 类使用的资源。
        /// </summary>
        /// <param name="disposing">指示是否正在显式释放托管资源。</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            foreach (var operate in _operateDict.Values)
            {
                operate.Dispose();
            }
            _operateDict.Clear();
            base.Dispose(disposing);
        }
    }
}
