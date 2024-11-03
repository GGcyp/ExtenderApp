﻿namespace MainApp.Common.ObjectPool
{
    /// <summary>
    /// 默认可以被销毁对象的对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class DisposableObjectPool<T> : DefaultObjectPool<T>, IDisposable where T : class
    {
        private volatile bool _isDisposed;

        public DisposableObjectPool(IPooledObjectPolicy<T> policy)
            : base(policy)
        {
        }

        public DisposableObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained)
            : base(policy, maximumRetained)
        {
        }

        public override T Get()
        {
            if (_isDisposed)
            {
                ThrowObjectDisposedException();
            }

            return base.Get();

            void ThrowObjectDisposedException()
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public override void Release(T obj)
        {
            if (_isDisposed || !ReleaseCore(obj))
            {
                DisposeItem(obj);
            }
        }

        public void Dispose()
        {
            _isDisposed = true;

            DisposeItem(_fastItem);
            _fastItem = null;

            while (_items.TryDequeue(out var item))
            {
                DisposeItem(item);
            }
        }

        private static void DisposeItem(T? item)
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
