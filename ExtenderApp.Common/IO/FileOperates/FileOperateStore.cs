using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Error;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件存储类，用于管理文件操作的并发处理。
    /// </summary>
    public class FileOperateStore : DisposableObject
    {
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

        private readonly ObjectPool<FileOperateData> _pool;

        /// <summary>
        /// 初始化 FileStorage 类的新实例。
        /// </summary>
        /// <param name="operatePool">并发操作池实例。</param>
        public FileOperateStore(ObjectPool<FileOperateData> pool)
        {
            _operateDict = new();
            _task = new();
            _task.StartCycle(o => ReleaseOperate(), TimeSpan.FromSeconds(60));
            _releaseAction = ReleaseOperate;
            _pool = pool ?? throw new ArgumentNullException(nameof(pool), "操作池不能为空。");
        }

        /// <summary>
        /// 根据给定的文件操作信息获取文件并发操作实例。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <returns>返回文件并发操作实例。</returns>
        public FileConcurrentOperate GetOperate(FileOperateInfo info)
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

                operate = FileConcurrentOperate.Get();

                var data = _pool.Get();
                data.Set(info, _releaseAction);
                operate.Start(data);

                _operateDict.TryAdd(id, operate);
                _task.Resume();
                return operate;
            }
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

            List<int> list = new List<int>(_operateDict.Count);
            foreach (var pair in _operateDict)
            {
                if (!pair.Value.IsExecuting)
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
        /// 删除本地文件信息
        /// </summary>
        /// <param name="info">本地文件信息对象</param>
        public void Delete(LocalFileInfo info)
        {
            ReleaseOperate(info.GetHashCode());
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
