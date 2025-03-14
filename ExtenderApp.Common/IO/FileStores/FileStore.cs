﻿using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<int, IConcurrentOperate> _operateDict;

        /// <summary>
        /// 一个私有的 ScheduledTask 对象。
        /// </summary>
        private readonly ScheduledTask _task;

        private readonly List<int> _operateList;

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
        /// <typeparam name="TData">并发操作数据的类型。</typeparam>
        /// <param name="info">文件操作信息。</param>
        /// <param name="createFunc">创建并发操作对象的函数。</param>
        /// <param name="fileLength">文件的长度。</param>
        /// <returns>并发操作对象，如果对象已存在则返回null。</returns>
        /// <remarks>
        /// T必须继承自<see cref="FileOperateData"/>。
        /// </remarks>
        public IConcurrentOperate<MemoryMappedViewAccessor, TData>? GetOperate<TData>(FileOperateInfo info, Func<FileOperateInfo, long, IConcurrentOperate<MemoryMappedViewAccessor, TData>> createFunc, long fileLength) where TData : FileOperateData
        {
            var id = info.GetHashCode();
            if (_operateDict.TryGetValue(id, out var operate))
            {
                return operate as IConcurrentOperate<MemoryMappedViewAccessor, TData>;
            }

            lock (_operateDict)
            {
                if (_operateDict.TryGetValue(id, out operate))
                {
                    return operate as IConcurrentOperate<MemoryMappedViewAccessor, TData>;
                }

                createFunc.ArgumentNull();

                var result = createFunc?.Invoke(info, fileLength);
                _operateDict.TryAdd(id, result);
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
