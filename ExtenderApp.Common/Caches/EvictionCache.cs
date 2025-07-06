using System.Collections.Concurrent;

namespace ExtenderApp.Common.Caches
{
    /// <summary>
    /// 抽象缓存类，用于实现具有逐出机制的缓存系统。
    /// </summary>
    /// <typeparam name="TValue">缓存项的类型，必须为类类型。</typeparam>
    public class EvictionCache<Tkey, TValue> : DisposableObject where TValue : class
    {
        /// <summary>
        /// 存储缓存项的字典，键为整数类型，值为<see cref="EvictionCacheInfo"/>类型。
        /// </summary>
        private readonly ConcurrentDictionary<Tkey, EvictionCacheInfo> _dict;

        /// <summary>
        /// 定时任务，用于定期检查并逐出缓存项。
        /// </summary>
        private readonly ScheduledTask _task;

        /// <summary>
        /// 检查间隔，默认为5分钟。
        /// </summary>
        private TimeSpan checkInterval { get; set; }

        /// <summary>
        /// 初始化<see cref="EvictionCache{T}"/>类的新实例。
        /// </summary>
        public EvictionCache() : this(TimeSpan.FromMinutes(5))
        {

        }

        /// <summary>
        /// 初始化EvictionCache实例。
        /// </summary>
        /// <param name="Interval">设置缓存项的清除间隔时间。</param>
        public EvictionCache(TimeSpan Interval)
        {
            _dict = new();
            _task = new ScheduledTask();
            ChangeInterval(Interval);
        }

        /// <summary>
        /// 向缓存中添加或更新缓存项。
        /// </summary>
        /// <param name="key">缓存项的键。</param>
        /// <param name="value">缓存项的值。</param>
        public void AddOrUpdate(Tkey key, TValue value)
        {
            if (_dict.TryGetValue(key, out var info))
            {
                info.Value = value!;
                info.LastVisitTime = DateTime.UtcNow;
            }
            else
            {
                var cacheInfo = EvictionCacheInfo.Get();
                cacheInfo.Value = value!;
                cacheInfo.LastVisitTime = DateTime.UtcNow;
                _dict[key] = cacheInfo;
            }
        }

        /// <summary>
        /// 尝试从缓存中获取指定键的缓存项。
        /// </summary>
        /// <param name="key">缓存项的键。</param>
        /// <param name="value">输出参数，用于存储获取的缓存项的值。</param>
        /// <returns>如果找到缓存项，则返回true；否则返回false。</returns>
        public bool TryGet(Tkey key, out TValue value)
        {
            if (_dict.TryGetValue(key, out var info))
            {
                info.LastVisitTime = DateTime.UtcNow;
                value = (TValue?)info.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// 从缓存中移除指定键的缓存项。
        /// </summary>
        /// <param name="key">缓存项的键。</param>
        /// <param name="value">输出参数，用于存储移除的缓存项的值。</param>
        /// <returns>如果找到并移除缓存项，则返回true；否则返回false。</returns>
        public bool Remove(Tkey key, out TValue value)
        {
            value = default;
            if (_dict.Remove(key, out var info))
            {
                value = info.Value as TValue;
                info.Release();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空缓存中的所有缓存项。
        /// </summary>
        public void Clear()
        {
            foreach (var info in _dict.Values)
            {
                info.Release();
            }
            _dict.Clear();
        }

        /// <summary>
        /// 定期检查并逐出缓存项。
        /// </summary>
        private void EvictionCheck()
        {
            if (_dict.Count == 0) return;

            var now = DateTime.UtcNow;
            var toRemove = new List<Tkey>();
            foreach (var pair in _dict)
            {
                if (pair.Value.LastVisitTime - now > checkInterval)
                    continue;

                if (ShouldEvict(pair.Value.GetValue<TValue>()))
                {
                    toRemove.Add(pair.Key);
                }
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                var key = toRemove[i];
                Remove(key, out var value);
                Evict(key, value);
            }
        }

        /// <summary>
        /// 判断指定缓存项是否应该被逐出。
        /// </summary>
        /// <param name="value">缓存中的值</param>
        /// <returns>如果应该移除，则返回true；否则返回false</returns>
        protected virtual bool ShouldEvict(TValue value)
        {
            return true;
        }

        /// <summary>
        /// 移除缓存中的键值对。
        /// </summary>
        /// <param name="key">要移除的键。</param>
        /// <param name="value">与键对应的值。</param>
        protected virtual void Evict(Tkey key, TValue value)
        {

        }

        /// <summary>
        /// 修改检查间隔
        /// </summary>
        /// <param name="timeSpan">新的检查间隔</param>
        public void ChangeInterval(TimeSpan timeSpan)
        {
            checkInterval = timeSpan;
            _task.Start(EvictionCheck, checkInterval, checkInterval);
        }

        /// <summary>
        /// 获取所有缓存项的键的集合。
        /// </summary>
        public IEnumerable<Tkey> Keys => _dict.Keys;

        /// <summary>
        /// 获取所有缓存项的值的集合。
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (var info in _dict.Values)
                {
                    yield return info.GetValue<TValue>();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var value in Values)
            {
                if (value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _dict.Clear();
            _task.Dispose();
            base.Dispose(disposing);
        }
    }
}
