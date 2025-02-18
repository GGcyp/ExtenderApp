using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
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
            _operateList = new();
            _task = new();
            _task.StartCycle(o => ReleaseOperate(), TimeSpan.FromSeconds(60));
        }

        /// <summary>
        /// 获取指定文件的并发操作对象。
        /// </summary>
        /// <typeparam name="T">并发操作数据的类型。</typeparam>
        /// <param name="info">文件操作信息。</param>
        /// <param name="createFunc">创建并发操作对象的函数。</param>
        /// <param name="fileLength">文件的长度。</param>
        /// <returns>并发操作对象，如果对象已存在则返回null。</returns>
        /// <remarks>
        /// T必须继承自<see cref="FileConcurrentOperateData"/>。
        /// </remarks>
        public IConcurrentOperate<MemoryMappedViewAccessor, T>? GetOperate<T>(FileOperateInfo info, Func<FileOperateInfo, long, IConcurrentOperate<MemoryMappedViewAccessor, T>> createFunc, long fileLength) where T : FileConcurrentOperateData
        {
            if (_operateDict.TryGetValue(info, out var operate))
            {
                return operate as IConcurrentOperate<MemoryMappedViewAccessor, T>;
            }

            lock (_operateDict)
            {
                if (_operateDict.TryGetValue(info, out operate))
                {
                    return operate as IConcurrentOperate<MemoryMappedViewAccessor, T>;
                }

                createFunc.ArgumentNull();

                var result = createFunc?.Invoke(info, fileLength);
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
            foreach (var operate in _operateDict.Values)
            {
                operate.Dispose();
            }
            _operateDict.Clear();
            base.Dispose(disposing);
        }
    }
}
