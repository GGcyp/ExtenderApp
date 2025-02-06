﻿using System.Collections.Concurrent;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.ObjectPools
{
    /// <summary>
    /// 默认的对象池类，继承自ObjectPool<T>泛型类，适用于需要频繁创建和销毁对象的场景。
    /// </summary>
    /// <typeparam name="T">对象池管理的对象类型，必须为引用类型。</typeparam>
    public class DefaultObjectPool<T> : ObjectPool<T> where T : class
    {
        /// <summary>
        /// 创建对象的函数。
        /// </summary>
        private readonly Func<T> _createFunc;

        /// <summary>
        /// 释放对象的函数。
        /// </summary>
        private readonly Func<T, bool> _releaseFunc;

        /// <summary>
        /// 对象池的最大容量。
        /// </summary>
        private readonly int _maxCapacity;

        /// <summary>
        /// 当前对象池中的对象数量。
        /// </summary>
        private int numItems;

        /// <summary>
        /// 存储对象池中对象的并发队列。
        /// </summary>
        private protected readonly ConcurrentQueue<T> _items = new();

        /// <summary>
        /// 存储快速获取的对象。
        /// </summary>
        private protected T? _fastItem;

        /// <summary>
        /// 获取当前对象池中的对象数量。
        /// </summary>
        public override int Count => numItems;

        /// <summary>
        /// 使用指定的对象池策略初始化DefaultObjectPool对象。
        /// </summary>
        /// <param name="policy">对象池策略。</param>
        public DefaultObjectPool(IPooledObjectPolicy<T> policy)
            : this(policy, Environment.ProcessorCount * 2)
        {
        }

        /// <summary>
        /// 使用指定的对象池策略和最大保留数量初始化DefaultObjectPool对象。
        /// </summary>
        /// <param name="policy">对象池策略。</param>
        /// <param name="maximumRetained">对象池的最大保留数量。</param>
        public DefaultObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained)
        {
            _createFunc = policy.Create;
            _releaseFunc = policy.Release;
            _maxCapacity = maximumRetained - 1;  // -1 to account for _fastItem
        }

        /// <summary>
        /// 从对象池中获取一个对象。
        /// </summary>
        /// <returns>从对象池中获取的对象。</returns>
        public override T Get()
        {
            var item = _fastItem;
            if (item == null || Interlocked.CompareExchange(ref _fastItem, null, item) != item)
            {
                if (_items.TryDequeue(out item))
                {
                    Interlocked.Decrement(ref numItems);
                    return item;
                }

                //
                return _createFunc();
            }

            return item;
        }

        /// <summary>
        /// 释放对象到对象池中。
        /// </summary>
        /// <param name="obj">要释放的对象。</param>
        public override void Release(T obj)
        {
            ReleaseCore(obj);
        }

        /// <summary>
        /// 释放对象到对象池中的核心方法。
        /// </summary>
        /// <param name="obj">要释放的对象。</param>
        /// <returns>如果对象成功释放到对象池中，则返回true；否则返回false。</returns>
        protected bool ReleaseCore(T obj)
        {
            if (!_releaseFunc(obj))
            {
                return false;
            }

            if (_fastItem != null || Interlocked.CompareExchange(ref _fastItem, obj, null) != null)
            {
                if (Interlocked.Increment(ref numItems) <= _maxCapacity)
                {
                    _items.Enqueue(obj);
                    return true;
                }

                Interlocked.Decrement(ref numItems);
                return false;
            }

            return true;
        }
    }
}
