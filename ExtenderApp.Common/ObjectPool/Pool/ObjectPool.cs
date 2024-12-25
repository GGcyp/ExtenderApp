using System.Collections.Concurrent;

namespace ExtenderApp.Common.ObjectPools
{
    public interface IObjectPool
    {

    }

    /// <summary>
    /// 一个对象池基类。
    /// </summary>
    public abstract class ObjectPool<T> : IObjectPool where T : class
    {
        /// <summary>
        /// 从池中获取一个对象（如果有），否则创建一个对象。
        /// </summary>
        /// <returns><typeparamref name="T"/></returns>
        public abstract T Get();

        /// <summary>
        /// 将对象返回到池中。
        /// </summary>
        public abstract void Release(T obj);
    }

    /// <summary>
    /// 创建一个<see cref="ObjectPool{T}"/>实例。
    /// </summary>
    public static class ObjectPool
    {
        private static readonly ConcurrentDictionary<Type, IObjectPool> _poolDict
            = new ConcurrentDictionary<Type, IObjectPool>();

        /// <summary>
        /// 创建一个<see cref="ObjectPool{T}"/>实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policy"></param>
        /// <returns><see cref="ObjectPool{T}"/></returns>
        public static ObjectPool<T> Create<T>(IPooledObjectPolicy<T>? policy = null, ObjectPoolProvider objectPoolProvider = null) where T : class, new()
        {
            //暂时
            Type poolType = typeof(T);
            if (_poolDict.TryGetValue(poolType, out var pool))
            {
                return (ObjectPool<T>)pool;
            }
            var provider = objectPoolProvider ?? new DefaultObjectPoolProvider();
            var objPool = provider.Create(policy ?? new DefaultPooledObjectPolicy<T>());
            _poolDict.TryAdd(poolType, objPool);
            return objPool;
        }
    }
}
