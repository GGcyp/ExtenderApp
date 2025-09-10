using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Caches
{
    /// <summary>
    /// 驱逐缓存信息类，实现了IResettable接口
    /// </summary>
    public class EvictionCacheInfo<T> : IResettable
    {
        /// <summary>
        /// 静态对象池，用于复用EvictionCacheInfo对象
        /// </summary>
        private static readonly ObjectPool<EvictionCacheInfo<T>> _pool
            = ObjectPool.CreateDefaultPool<EvictionCacheInfo<T>>();

        /// <summary>
        /// 从对象池中获取EvictionCacheInfo对象
        /// </summary>
        /// <returns>从对象池中获取的EvictionCacheInfo对象</returns>
        public static EvictionCacheInfo<T> Get() => _pool.Get();

        /// <summary>
        /// 将EvictionCacheInfo对象释放回对象池
        /// </summary>
        /// <param name="info">需要释放的EvictionCacheInfo对象</param>
        public static void Release(EvictionCacheInfo<T> info) => _pool.Release(info);

        /// <summary>
        /// 获取或设置缓存的值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 获取或设置上次访问时间
        /// </summary>
        public DateTime LastVisitTime { get; set; }

        /// <summary>
        /// 释放当前EvictionCacheInfo对象
        /// </summary>
        public void Release()
        {
            Release(this);
        }

        /// <summary>
        /// 尝试重置EvictionCacheInfo对象
        /// </summary>
        /// <returns>如果重置成功返回true，否则返回false</returns>
        public bool TryReset()
        {
            Value = default;
            return true;
        }
    }
}
