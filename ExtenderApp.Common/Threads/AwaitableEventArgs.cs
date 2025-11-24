using System.Threading.Tasks.Sources;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Threads
{
    /// <summary>
    /// 表示一个可等待的、可池化的通用异步操作参数。
    /// 此类实现了 <see cref="IValueTaskSource{TResult}"/>，使其能够被 <see cref="ValueTask{TResult}"/> 等待，
    /// 从而在异步操作同步完成时避免不必要的堆内存分配。
    /// </summary>
    /// <typeparam name="T">异步操作返回的结果类型。</typeparam>
    public sealed class AwaitableEventArgs<T> : IValueTaskSource<T>
    {
        public readonly static ObjectPool<AwaitableEventArgs<T>> _pool
            = ObjectPool.CreateDefaultPool<AwaitableEventArgs<T>>();

        /// <summary>
        /// 从对象池中获取一个 <see cref="AwaitableEventArgs{T}"/> 实例。
        /// </summary>
        /// <returns><see cref="AwaitableEventArgs{T}"/> 实例</returns>
        public static AwaitableEventArgs<T> Get()
            => _pool.Get();

        /// <summary>
        /// 回收指定的 <see cref="AwaitableEventArgs{T}"/> 实例到对象池中以供重用。
        /// </summary>
        /// <param name="args">指定的 <see cref="AwaitableEventArgs{T}"/> 实例</param>
        public static void Release(AwaitableEventArgs<T> args)
            => _pool.Release(args);

        /// <summary>
        /// 回收当前实例到对象池中以供重用。
        /// </summary>
        public void Release()
            => _pool.Release(this);

        /// <summary>
        /// 多线程重置值任务源的核心实现。
        /// </summary>
        private ManualResetValueTaskSourceCore<T> vts;

        /// <summary>
        /// 获取当前操作的版本令牌，用于向 <see cref="ValueTask{TResult}"/> 验证此源的有效性。
        /// </summary>
        public short Version => vts.Version;

        public AwaitableEventArgs()
        {
            vts = new();
        }

        /// <summary>
        /// 标记操作成功完成并设置结果。
        /// </summary>
        /// <param name="response">操作的结果。</param>
        public void SetResult(T response)
        {
            vts.SetResult(response);
        }

        /// <summary>
        /// 标记操作因异常而失败。
        /// </summary>
        /// <param name="error">导致失败的异常。</param>
        public void SetException(Exception error)
        {
            vts.SetException(error);
        }

        /// <summary>
        /// 重置 <see cref="IValueTaskSource"/> 的核心状态，以便此实例可以被安全地重用。
        /// </summary>
        public void Reset()
        {
            vts.Reset();
        }

        /// <inheritdoc/>
        public T GetResult(short token)
        {
            try
            {
                return vts.GetResult(token);
            }
            finally
            {
                // 获取结果后立即重置，为下一次操作做准备
                Reset();
            }
        }

        /// <inheritdoc/>
        public ValueTaskSourceStatus GetStatus(short token)
        {
            return vts.GetStatus(token);
        }

        /// <inheritdoc/>
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            vts.OnCompleted(continuation, state, token, flags);
        }

        public static implicit operator ValueTask<T>(AwaitableEventArgs<T> args)
            => new ValueTask<T>(args, args.Version);
    }
}