using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 流操作池类，继承自 DisposableObject 类。用于管理流操作对象的重用。
    /// </summary>
    public class StreamOperationPool : DisposableObject
    {
        /// <summary>
        /// 流操作池策略类，继承自 PooledObjectPolicy 类，泛型参数 T 必须是实现了 IStreamOperation 接口的类，并且 T 必须有一个无参构造函数。
        /// </summary>
        /// <typeparam name="T">泛型参数 T，必须是实现了 IStreamOperation 接口的类，并且 T 必须有一个无参构造函数。</typeparam>
        private class StreamOperationPoolPolicy<T> : PooledObjectPolicy<T> where T : IStreamOperation, new()
        {
            /// <summary>
            /// 创建一个新的 T 类型的实例。
            /// </summary>
            /// <returns>返回一个新的 T 类型的实例。</returns>
            public override T Create()
            {
                return new T();
            }

            /// <summary>
            /// 释放 T 类型的实例，如果实例可以被重置，则返回 true，否则返回 false。
            /// </summary>
            /// <param name="obj">需要释放的 T 类型的实例。</param>
            /// <returns>如果实例可以被重置，则返回 true，否则返回 false。</returns>
            public override bool Release(T obj)
            {
                return obj.TryReset();
            }
        }

        /// <summary>
        /// 存储不同类型的对象池的字典，键为类型，值为对应的对象池。
        /// </summary>
        private readonly ConcurrentDictionary<Type, IObjectPool> _poolDict;

        /// <summary>
        /// 初始化 StreamOperationPool 类的新实例。
        /// </summary>
        public StreamOperationPool()
        {
            _poolDict = new();
        }

        /// <summary>
        /// 从对象池中获取指定类型的流操作对象。
        /// </summary>
        /// <typeparam name="T">需要获取的流操作对象的类型，必须实现 IStreamOperation 接口并且有一个无参构造函数。</typeparam>
        /// <returns>返回指定类型的流操作对象。</returns>
        public T GetStreamOperation<T>() where T : class, IStreamOperation, new()
        {
            var type = typeof(T);
            if (_poolDict.TryGetValue(type, out var pool))
            {
                return (T)pool.Get();
            }

            lock (_poolDict)
            {
                if (_poolDict.TryGetValue(type, out pool))
                {
                    return (T)pool.Get();
                }

                pool = ObjectPool.Create(new StreamOperationPoolPolicy<T>());
                _poolDict.TryAdd(type, pool);
                return (T)pool.Get();
            }
        }

        /// <summary>
        /// 释放流操作对象
        /// </summary>
        /// <typeparam name="T">流操作对象的类型，需要实现IStreamOperation接口</typeparam>
        /// <param name="streamOperation">需要释放的流操作对象</param>
        /// <remarks>
        /// 如果传入的流操作对象为null，则直接返回。
        /// 如果对象池中存在对应类型的池，则将流操作对象释放回池中。
        /// </remarks>
        public void ReleaseStreamOperation<T>(T streamOperation) where T : class, IStreamOperation
        {
            if (streamOperation == null)
            {
                return;
            }
            var type = typeof(T);
            if (_poolDict.TryGetValue(type, out var pool))
            {
                pool.Release(streamOperation);
            }
        }
    }
}
