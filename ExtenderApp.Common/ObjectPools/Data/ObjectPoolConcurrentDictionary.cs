using System.Collections.Concurrent;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.ObjectPools
{
    /// <summary>
    /// 基于ConcurrentDictionary实现的对象池容器类
    /// </summary>
    /// <typeparam name="T">对象池中的对象类型</typeparam>
    public class ObjectPoolConcurrentDictionary<T> : ConcurrentDictionary<T, IObjectPool> where T : notnull
    {
        /// <summary>
        /// 向对象池容器中添加对象池
        /// </summary>
        /// <param name="key">对象池的键</param>
        /// <param name="pool">要添加的对象池</param>
        public void AddPool(T key, IObjectPool pool)
        {
            TryAdd(key, pool);
        }

        /// <summary>
        /// 从对象池容器中获取指定键的对象池
        /// </summary>
        /// <param name="key">对象池的键</param>
        /// <param name="pool">返回的对象池</param>
        public void GetPool(T key, out IObjectPool? pool)
        {
            if (TryGetValue(key, out var existingPool))
            {
                pool = existingPool;
            }
            else
            {
                pool = null;
            }
        }
    }
}
