using System.Collections.Concurrent;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.ObjectPools
{
    /// <summary>
    /// 一个对象池基类。
    /// </summary>
    public abstract class ObjectPool<T> : IObjectPool<T> where T : class
    {
        /// <summary>
        /// 对象池中还有多少对象
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// 从池中获取一个对象（如果有），否则创建一个对象。
        /// </summary>
        /// <returns><typeparamref name="T"/></returns>
        public abstract T Get();

        /// <summary>
        /// 将对象返回到池中。
        /// </summary>
        public abstract void Release(T obj);

        public void Release(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            if (obj is not T result)
                throw new InvalidOperationException(string.Format("回收类型不正确：{0}", obj.GetType().FullName));

            Release(result);
        }

        object IObjectPool.Get()
        {
            return Get();
        }
    }

    /// <summary>
    /// 创建一个<see cref="ObjectPool{T}"/>实例。
    /// </summary>
    public static class ObjectPool
    {
        private static readonly ConcurrentDictionary<Type, IObjectPool> _poolDict
            = new ConcurrentDictionary<Type, IObjectPool>();

        /// <summary>
        /// 创建一个默认的对象池。
        /// </summary>
        /// <typeparam name="T">对象池中存储的对象的类型，必须是一个引用类型且拥有一个无参构造函数。</typeparam>
        /// <returns>返回创建的对象池。</returns>
        public static ObjectPool<T> CreateDefaultPool<T>() where T : class, new()
        {
            return Create(new DefaultPooledObjectPolicy<T>());
        }

        /// <summary>
        /// 创建一个<see cref="ObjectPool{T}"/>实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policy"></param>
        /// <returns><see cref="ObjectPool{T}"/></returns>
        public static ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy, ObjectPoolProvider? objectPoolProvider = null, int maximumRetained = -1) where T : class
        {
            //暂时
            Type poolType = typeof(T);
            if (_poolDict.TryGetValue(poolType, out var pool))
            {
                return (ObjectPool<T>)pool;
            }
            var provider = objectPoolProvider ?? DefaultObjectPoolProvider.Default;
            var objPool = provider.Create(policy, maximumRetained);
            _poolDict.TryAdd(poolType, objPool);
            return objPool;
        }
    }
}
