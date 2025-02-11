using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO.StreamOperates
{
    /// <summary>
    /// 流操作池类，用于管理和复用流操作对象。
    /// </summary>
    public class StreamOperatePool : DisposableObject
    {
        /// <summary>
        /// 流操作池策略类，用于创建和释放流操作对象。
        /// </summary>
        /// <typeparam name="T">流操作对象的类型。</typeparam>
        private class StreamOperatePoolPolicy<T> : PooledObjectPolicy<T> where T : StreamOperate, new()
        {
            /// <summary>
            /// 流操作池策略
            /// </summary>
            /// <param name="pool">流操作池</param>
            private readonly StreamOperationPool _pool;

            public StreamOperatePoolPolicy(StreamOperationPool pool)
            {
                _pool = pool;
            }

            /// <summary>
            /// 创建一个新的流操作对象。
            /// </summary>
            /// <returns>返回创建的流操作对象。</returns>
            public override T Create()
            {
                var result = new T();
                result.ReleaseAtion = _pool.ReleaseStreamOperation;
                return result;
            }

            /// <summary>
            /// 释放指定的流操作对象。
            /// </summary>
            /// <param name="obj">要释放的流操作对象。</param>
            /// <returns>如果对象成功释放，则返回true；否则返回false。</returns>
            public override bool Release(T obj)
            {
                return obj.TryReset();
            }
        }

        private const int MaxPoolSize = 100;

        /// <summary>
        /// 流操作池策略
        /// </summary>
        /// <param name="pool">流操作池</param>
        private readonly StreamOperationPool _streamOperationPool;

        /// <summary>
        /// 存储文件操作信息的字典。
        /// </summary>
        private readonly ConcurrentDictionary<FileOperateInfo, StreamOperate> _optionsDict;

        /// <summary>
        /// 存储对象池的字典。
        /// </summary>
        private readonly ConcurrentDictionary<Type, IObjectPool> _poolDict;

        /// <summary>
        /// 用于同步访问的锁对象。
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// 存储待释放文件操作信息的列表。
        /// </summary>
        private readonly List<FileOperateInfo> _releaseList;

        /// <summary>
        /// 定时任务对象，用于定期释放流操作对象。
        /// </summary>
        private readonly ScheduledTask _task;

        private bool isReleasing;

        /// <summary>
        /// 初始化StreamOperationPool类的实例。
        /// </summary>
        public StreamOperatePool()
        {
            _optionsDict = new();
            _poolDict = new();
            _lock = new();
            _releaseList = new();
            _task = new();
            _task.StartCycle(o => ReleaseOperate(), TimeSpan.FromSeconds(60));
        }

        /// <summary>
        /// 根据文件操作信息获取流操作对象。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        /// <returns>返回对应的流操作对象。</returns>
        public StreamOperate GetOperate(FileOperateInfo info)
        {
            return GetOperate<StreamOperate>(info);
        }

        /// <summary>
        /// 根据文件操作信息获取指定类型的流操作对象。
        /// </summary>
        /// <typeparam name="T">流操作对象的类型。</typeparam>
        /// <param name="info">文件操作信息。</param>
        /// <returns>返回对应的流操作对象。</returns>
        public T GetOperate<T>(FileOperateInfo info) where T : StreamOperate, new()
        {
            if (_optionsDict.TryGetValue(info, out var options))
            {
                _task.Resume();
                return (T)options;
            }

            lock (_lock)
            {
                if (_optionsDict.TryGetValue(info, out options))
                {
                    _task.Resume();
                    return (T)options;
                }

                var result = GetStreamOperateForPool<T>();
                result.OpenFile(info);
                _optionsDict.TryAdd(info, result);
                _task.Resume();
                if (_optionsDict.Count > MaxPoolSize)
                {
                    ReleaseOperate();
                }
                return result;
            }
        }

        /// <summary>
        /// 从池中获取指定类型的流操作对象。
        /// </summary>
        /// <typeparam name="T">流操作对象的类型。</typeparam>
        /// <returns>返回从池中获取的流操作对象。</returns>
        private T GetStreamOperateForPool<T>() where T : StreamOperate, new()
        {
            if (_poolDict.TryGetValue(typeof(T), out var pool))
            {
                return (T)pool.Get();
            }

            lock (_lock)
            {
                if (_poolDict.TryGetValue(typeof(T), out pool))
                {
                    return (T)pool.Get();
                }
                pool = ObjectPool.Create(new StreamOperatePoolPolicy<T>(_streamOperationPool));
                _poolDict.TryAdd(typeof(T), pool);
                return (T)pool.Get();
            }
        }

        /// <summary>
        /// 定期释放流操作对象。
        /// </summary>
        private void ReleaseOperate()
        {
            if (isReleasing)
            {
                return;
            }

            isReleasing = true;
            foreach (var info in _optionsDict.Keys)
            {
                if (_optionsDict.TryGetValue(info, out var options))
                {
                    if (!options.IsExecuting)
                    {
                        _releaseList.Add(info);
                    }
                }
            }

            for (int i = 0; i < _releaseList.Count; i++)
            {
                ReleaseOperate(_releaseList[i]);
            }
            _releaseList.Clear();
            isReleasing = false;
        }

        /// <summary>
        /// 释放指定的文件操作信息对应的流操作对象。
        /// </summary>
        /// <param name="info">文件操作信息。</param>
        public void ReleaseOperate(FileOperateInfo info)
        {
            if (_optionsDict.TryRemove(info, out var options))
            {
                if (_poolDict.TryGetValue(options.GetType(), out var pool))
                {
                    pool.Release(options);
                    return;
                }
                options.Dispose();
            }

            if (_optionsDict.Count == 0)
            {
                _task.Pause();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (var options in _optionsDict.Values)
                {
                    options.Dispose();
                }
            }
        }
    }
}
