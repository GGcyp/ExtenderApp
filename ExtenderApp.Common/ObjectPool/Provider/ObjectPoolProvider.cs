

namespace ExtenderApp.Common.ObjectPool
{
    /// <summary>
    /// 通用层，对象池提供器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ObjectPoolProvider
    {
        /// <summary>
        /// 生成一个<see cref="ObjectPool"/>实例。
        /// </summary>
        /// <typeparam name="T">用这个类型生成对象池</typeparam>
        public ObjectPool<T> Create<T>() where T : class, new()
        {
            return Create(new DefaultPooledObjectPolicy<T>());
        }

        /// <summary>
        /// 用<see cref="IPooledObjectPolicy{T}"/>生成<see cref="ObjectPool"/>。
        /// </summary>
        /// <typeparam name="T">用这个类型生成对象池</typeparam>
        public abstract ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy) where T : class;
    }
}
