using System.Runtime.CompilerServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 一个实现了IDisposable接口的抽象类，用于管理可释放资源。
    /// </summary>
    public abstract class DisposableObject : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// 用于标记对象是否已被释放。
        /// </summary>
        private int _disposed;

        /// <summary>
        /// 获取一个值，指示对象是否已被释放。
        /// </summary>
        public bool IsDisposed => _disposed == 1;

        /// <summary>
        /// 析构函数，用于在对象被垃圾回收时释放资源。
        /// </summary>
        ~DisposableObject()
        {
            DisposeAsync(false);
            Dispose(false);
        }

        /// <summary>
        /// 如果对象已被释放，则抛出ObjectDisposedException异常。
        /// </summary>
        /// <exception cref="ObjectDisposedException">如果对象已被释放，则抛出此异常。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// 释放或重置由DisposableObject使用的所有资源。
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放或重置由DisposableObject使用的资源。
        /// </summary>
        /// <param name="disposing">指示是否由Dispose方法调用。</param>
        protected virtual void Dispose(bool disposing)
        {
            // 子类实现具体释放逻辑
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放或重置由DisposableObject使用的资源。
        /// </summary>
        /// <param name="disposing">指示是否由Dispose方法调用。</param>
        /// <returns>异步调用结束时返回</returns>
        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            // 子类实现具体释放逻辑
            return ValueTask.CompletedTask;
        }
    }
}
