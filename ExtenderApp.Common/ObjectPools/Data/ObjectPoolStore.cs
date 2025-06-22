using ExtenderApp.Abstract;

namespace ExtenderApp.Common.ObjectPools
{
    /// <summary>
    /// 对象池存储类，继承自ObjectPoolConcurrentDictionary泛型类，使用Type作为键。
    /// </summary>
    public class ObjectPoolStore : ObjectPoolConcurrentDictionary<Type>
    {
        /// <summary>
        /// 向对象池中添加特定类型的对象池。
        /// </summary>
        /// <typeparam name="T">要添加的对象池的类型。</typeparam>
        /// <param name="pool">要添加的对象池实例。</param>
        /// <exception cref="ArgumentNullException">如果传入的对象池实例为null，则抛出此异常。</exception>
        public void AddPool<T>(IObjectPool pool) where T : class
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            AddPool(typeof(T), pool);
        }

        /// <summary>
        /// 从对象池中获取特定类型的对象池。
        /// </summary>
        /// <typeparam name="T">要获取的对象池的类型。</typeparam>
        /// <returns>返回指定类型的对象池实例。</returns>
        public ObjectPool<T> GetPool<T>() where T : class, new()
        {
            if (TryGetValue(typeof(T), out var pool))
            {
                return (ObjectPool<T>)pool;
            }

            lock (this)
            {
                if (TryGetValue(typeof(T), out pool))
                {
                    return (ObjectPool<T>)pool;
                }
                var result = ObjectPool.CreateDefaultPool<T>();
                AddPool(typeof(T), result);
                return result;
            }
        }

        /// <summary>
        /// 获取指定类型的对象池。
        /// </summary>
        /// <typeparam name="T">对象池中的对象类型。</typeparam>
        /// <param name="policy">对象池策略，用于控制对象的创建、激活和释放。</param>
        /// <param name="objectPoolProvider">对象池提供程序，用于自定义对象池的实现。如果为null，则使用默认实现。</param>
        /// <param name="maximumRetained">保留在对象池中的最大对象数。如果为-1，则使用默认值。</param>
        /// <param name="canReuse">是否允许重用对象池中的对象。如果为true，则对象池中的对象可以被重复使用；如果为false，则对象池中的对象在释放后将被销毁。</param>
        /// <returns>返回指定类型的对象池。</returns>
        public ObjectPool<T> GetPool<T>(IPooledObjectPolicy<T> policy, ObjectPoolProvider? objectPoolProvider = null, int maximumRetained = -1, bool canReuse = true) where T : class
        {
            Type type = typeof(T);
            if (TryGetValue(type, out var pool))
            {
                return (ObjectPool<T>)pool;
            }

            lock (this)
            {
                if (TryGetValue(type, out pool))
                {
                    return (ObjectPool<T>)pool;
                }
                var result = ObjectPool.Create(policy, objectPoolProvider, maximumRetained, canReuse);
                return result;
            }
        }
    }
}
