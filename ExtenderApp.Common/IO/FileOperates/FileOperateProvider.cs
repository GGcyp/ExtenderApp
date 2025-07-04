using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件存储类，用于管理文件操作的并发处理。
    /// </summary>
    public class FileOperateProvider : DisposableObject, IFileOperateProvider
    {
        private const int ReleaseTime = 1;

        /// <summary>
        /// 文件并发操作对象池
        /// </summary>
        private readonly ObjectPool<FileConcurrentOperate> _operatePool;

        /// <summary>
        /// 文件操作数据对象池
        /// </summary>
        private readonly ObjectPool<FileOperateData> _dataPool;

        /// <summary>
        /// 文件操作字典，用于存储文件操作信息。
        /// </summary>
        private readonly ConcurrentDictionary<int, FileConcurrentOperate> _operateDict;

        /// <summary>
        /// 一个私有的 ScheduledTask 对象。
        /// </summary>
        private readonly ScheduledTask _task;

        /// <summary>
        /// 文件操作完成后执行的释放动作。
        /// </summary>
        private readonly Action<FileOperateInfo> _releaseAction;

        /// <summary>
        /// 初始化 FileStorage 类的新实例。
        /// </summary>
        /// <param name="operatePool">并发操作池实例。</param>
        public FileOperateProvider()
        {
            _operateDict = new();
            _task = new();
            _task.StartCycle(o => ReleaseOperate(), TimeSpan.FromSeconds(60));
            _releaseAction = ReleaseOperate;
            _dataPool = ObjectPool.CreateDefaultPool<FileOperateData>();
            _operatePool = ObjectPool.CreateDefaultPool<FileConcurrentOperate>();
        }

        /// <summary>
        /// 根据给定的文件操作信息获取文件并发操作实例。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <returns>返回文件并发操作实例。</returns>
        public IFileOperate GetOperate(FileOperateInfo info)
        {
            var id = info.GetHashCode();
            if (_operateDict.TryGetValue(id, out var operate))
            {
                return operate;
            }

            lock (_operateDict)
            {
                if (_operateDict.TryGetValue(id, out operate))
                {
                    return operate;
                }

                operate = _operatePool.Get();

                var data = _dataPool.Get();
                data.Set(info, _releaseAction);
                operate.Start(data);

                _operateDict.TryAdd(id, operate);
                _task.Resume();
                return operate;
            }
        }

        public void ReleaseOperate(IFileOperate fileOperate)
        {
            ReleaseOperate(fileOperate.Info.GetHashCode());
        }

        /// <summary>
        /// 释放指定的文件操作对象。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        public void ReleaseOperate(FileOperateInfo info)
        {
            ReleaseOperate(info.GetHashCode());
        }

        /// <summary>
        /// 释放操作资源
        /// </summary>
        /// <param name="id">操作的唯一标识符</param>
        /// <remarks>
        /// 该方法尝试从操作字典中移除指定ID的操作，并调用该操作的Release方法释放资源。
        /// 如果指定的ID不存在于操作字典中，则直接返回，不进行任何操作。
        /// </remarks>
        public void ReleaseOperate(int id)
        {
            if (!_operateDict.TryRemove(id, out var operate))
            {
                return;
            }

            _operatePool.Release(operate);
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

            List<int> list = new List<int>(_operateDict.Count);
            var now = DateTime.Now;
            foreach (var pair in _operateDict)
            {
                var fileOperate = pair.Value;
                if (!fileOperate.IsHosted)
                    continue;

                if (!fileOperate.IsExecuting && (System.Math.Abs((fileOperate.LastOperateTime - now).TotalMinutes) > ReleaseTime))
                {
                    list.Add(pair.Key);
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                ReleaseOperate(list[i]);
            }

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
