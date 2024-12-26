using System.Collections.Concurrent;

namespace ExtenderApp.Common.ObjectPools
{
    /// <summary>
    /// 继承至<see cref="ObjectPool{T}"/>的默认对象池。
    /// </summary>
    /// <typeparam name="T">The type to pool objects for.</typeparam>
    /// <remarks>此实现保留了一个保留对象的缓存。这意味着，如果在池还未达到已达到最大缓存对象时可以回收对象，则这些对象将被垃圾回收。</remarks>
    public class DefaultObjectPool<T> : ObjectPool<T> where T : class
    {
        private readonly Func<T> _createFunc;
        private readonly Func<T, bool> _releaseFunc;
        private readonly int _maxCapacity;
        private int m_NumItems;

        private protected readonly ConcurrentQueue<T> _items = new();
        private protected T? _fastItem;

        /// <summary>
        /// 创建<see cref="DefaultObjectPool{T}"/>实例。
        /// </summary>
        /// <param name="policy">对象生成策略</param>
        public DefaultObjectPool(IPooledObjectPolicy<T> policy)
            : this(policy, Environment.ProcessorCount * 2)
        {
        }

        /// <summary>
        /// 创建<see cref="DefaultObjectPool{T}"/>实例。
        /// </summary>
        /// <param name="policy">对象生成策略</param>
        /// <param name="maximumRetained">对象池最大容量</param>
        public DefaultObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained)
        {
            _createFunc = policy.Create;
            _releaseFunc = policy.Release;
            _maxCapacity = maximumRetained - 1;  // -1 to account for _fastItem
        }

        public override T Get()
        {
            var item = _fastItem;
            if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
            {
                if (_items.TryDequeue(out item))
                {
                    Interlocked.Decrement(ref m_NumItems);
                    return item;
                }

                //
                return _createFunc();
            }

            return item;
        }

        public override void Release(T obj)
        {
            ReleaseCore(obj);
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <returns>已经回收了就返回<see cref="true"/></returns>
        protected bool ReleaseCore(T obj)
        {
            if (!_releaseFunc(obj))
            {
                return false;
            }

            if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, obj, null) != null)
            {
                if (Interlocked.Increment(ref m_NumItems) <= _maxCapacity)
                {
                    _items.Enqueue(obj);
                    return true;
                }

                Interlocked.Decrement(ref m_NumItems);
                return false;
            }

            return true;
        }
    }
}
