using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 实现IDisposable接口的可释放资源对象
    /// </summary>
    public class DisposableObject : IDisposableObject
    {
        /// <summary>
        /// 表示对象是否已被释放
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 析构函数，用于在垃圾回收时释放资源
        /// </summary>
        ~DisposableObject()
        {
            Dispose(true);
        }

        /// <summary>
        /// 检查对象是否已经被释放，如果已释放则抛出异常
        /// </summary>
        /// <exception cref="ObjectDisposedException">如果对象已被释放，则抛出此异常</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfDisposed()
        {
            // 检查对象是否已经被释放
            if (IsDisposed)
            {
                // 如果对象已被释放，抛出ObjectDisposedException异常
                //ThrowHelper.ThrowObjectDisposedException(this);
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        /// <summary>
        /// 释放对象占用的资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放对象占用的资源
        /// </summary>
        /// <param name="disposing">指示调用者是否显式调用Dispose方法</param>
        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }
    }
}
