using System.Threading.Tasks.Sources;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Threads
{
    public sealed class AwaitableEventArgs : IValueTaskSource
    {
        public static readonly ObjectPool<AwaitableEventArgs> _pool
            = ObjectPool.CreateDefaultPool<AwaitableEventArgs>();

        /// <summary>
        /// 从对象池中获取一个 <see cref="AwaitableEventArgs"/> 实例。
        /// </summary>
        /// <returns><see cref="AwaitableEventArgs"/> 实例</returns>
        public static AwaitableEventArgs Get()
            => _pool.Get();

        /// <summary>
        /// 多线程重置值任务源的核心实现。
        /// </summary>
        private ManualResetValueTaskSourceCore<bool> vts;

        public AwaitableEventArgs()
        {
            vts = new();
        }

        /// <summary>
        /// 获取当前操作的版本令牌，用于向 <see cref="ValueTask"/> 验证此源的有效性。
        /// </summary>
        public short Version => vts.Version;

        /// <summary>
        /// 标记操作成功完成。
        /// </summary>
        public AwaitableEventArgs SetResult()
        {
            vts.SetResult(true);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个无参数的委托。
        /// 这允许将工作卸载到后台线程。
        /// </summary>
        /// <param name="action">要在线程池中执行的委托。</param>
        public AwaitableEventArgs SetResult(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ThreadPool.UnsafeQueueUserWorkItem(static s =>
            {
                var (args, action) = s;
                try
                {
                    action();
                    args.SetResult();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }, (this, action), false);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个带有一个参数的委托。
        /// 此重载通过传递状态参数来避免创建闭包，从而减少内存分配。
        /// </summary>
        /// <typeparam name="T">传递给委托的参数类型。</typeparam>
        /// <param name="action">要在线程池中执行的委托。</param>
        /// <param name="item">要传递给委托的参数值。</param>
        public AwaitableEventArgs SetResult<T>(Action<T> action, T item)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ThreadPool.UnsafeQueueUserWorkItem(static s =>
            {
                var (args, action, item) = s;
                try
                {
                    action(item);
                    args.SetResult();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }, (this, action, item), false);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个带有两个参数的委托。
        /// 此重载通过传递状态参数来避免创建闭包，从而减少内存分配。
        /// </summary>
        /// <typeparam name="T1">传递给委托的第一个参数类型。</typeparam>
        /// <typeparam name="T2">传递给委托的第二个参数类型。</typeparam>
        /// <param name="action">要在线程池中执行的委托。</param>
        /// <param name="item1">要传递给委托的第一个参数值。</param>
        /// <param name="item2">要传递给委托的第二个参数值。</param>
        public AwaitableEventArgs SetResult<T1, T2>(Action<T1, T2> action, T1 item1, T2 item2)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ThreadPool.UnsafeQueueUserWorkItem(static s =>
            {
                var (args, action, item1, item2) = s;
                try
                {
                    action(item1, item2);
                    args.SetResult();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }, (this, action, item1, item2), false);
            return this;
        }

        /// <summary>
        /// 标记操作因异常而失败。
        /// </summary>
        /// <param name="error">导致失败的异常。</param>
        public AwaitableEventArgs SetException(Exception error)
        {
            vts.SetException(error);
            return this;
        }

        /// <inheritdoc/>
        public void GetResult(short token)
        {
            try
            {
                vts.GetResult(token);
            }
            finally
            {
                // 获取结果后立即重置，为下一次操作做准备
                vts.Reset();
                _pool.Release(this);
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

        public static implicit operator ValueTask(AwaitableEventArgs args)
            => new ValueTask(args, args.Version);
    }

    /// <summary>
    /// 表示一个可等待的、可池化的通用异步操作参数。
    /// 此类实现了 <see cref="IValueTaskSource{TResult}"/>，使其能够被 <see cref="ValueTask{TResult}"/> 等待，
    /// 从而在异步操作同步完成时避免不必要的堆内存分配。
    /// </summary>
    /// <typeparam name="T">异步操作返回的结果类型。</typeparam>
    public sealed class AwaitableEventArgs<T> : IValueTaskSource<T>
    {
        public static readonly ObjectPool<AwaitableEventArgs<T>> _pool
            = ObjectPool.CreateDefaultPool<AwaitableEventArgs<T>>();

        /// <summary>
        /// 从对象池中获取一个 <see cref="AwaitableEventArgs{T}"/> 实例。
        /// </summary>
        /// <returns><see cref="AwaitableEventArgs{T}"/> 实例</returns>
        public static AwaitableEventArgs<T> Get()
            => _pool.Get();

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
        public AwaitableEventArgs<T> SetResult(T response)
        {
            vts.SetResult(response);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并将结果设置为 <typeparamref name="T"/> 的默认值。
        /// </summary>
        public AwaitableEventArgs<T> SetResult()
        {
            vts.SetResult(default!);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并使用指定函数生成的结果进行设置。
        /// 这允许延迟计算结果，直到需要完成操作时才执行。
        /// </summary>
        /// <param name="func">用于生成结果的函数。</param>
        public AwaitableEventArgs<T> SetResult(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            // 将状态打包成元组，以避免闭包分配
            var state = (args: this, func: func);

            ThreadPool.UnsafeQueueUserWorkItem(static s =>
            {
                // 解包状态
                var (args, func) = s;
                try
                {
                    // 在线程池线程上执行函数并设置结果
                    var result = func();
                    args.SetResult(result);
                }
                catch (Exception ex)
                {
                    // 如果函数执行引发异常，则设置异常
                    args.SetException(ex);
                }
            }, state, false);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并使用带一个参数的函数生成结果。
        /// 此重载通过传递状态参数 <paramref name="item1"/> 来避免创建闭包，从而减少内存分配。
        /// </summary>
        /// <typeparam name="T1">传递给函数的参数类型。</typeparam>
        /// <param name="func">用于生成结果的函数。</param>
        /// <param name="item1">要传递给 <paramref name="func"/> 的参数值。</param>
        public AwaitableEventArgs<T> SetResult<T1>(Func<T1, T> func, T1 item1)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            // 将状态打包成元组，以避免闭包分配
            var state = (args: this, func: func, item1: item1);

            ThreadPool.UnsafeQueueUserWorkItem(static s =>
            {
                // 解包状态
                var (args, func, item) = s;
                try
                {
                    // 在线程池线程上执行函数并设置结果
                    var result = func(item);
                    args.SetResult(result);
                }
                catch (Exception ex)
                {
                    // 如果函数执行引发异常，则设置异常
                    args.SetException(ex);
                }
            }, state, false);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并使用带两个参数的函数生成结果。
        /// 此重载通过传递状态参数 <paramref name="item1"/> 和 <paramref name="item2"/> 来避免创建闭包，从而减少内存分配。
        /// </summary>
        /// <typeparam name="T1">传递给函数的第一个参数类型。</typeparam>
        /// <typeparam name="T2">传递给函数的第二个参数类型。</typeparam>
        /// <param name="func">用于生成结果的函数。</param>
        /// <param name="item1">要传递给 <paramref name="func"/> 的第一个参数值。</param>
        /// <param name="item2">要传递给 <paramref name="func"/> 的第二个参数值。</param>
        public AwaitableEventArgs<T> SetResult<T1, T2>(Func<T1, T2, T> func, T1 item1, T2 item2)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            // 将状态打包成元组，以避免闭包分配
            var state = (args: this, func: func, item1: item1, item2: item2);

            ThreadPool.UnsafeQueueUserWorkItem(static s =>
            {
                // 解包状态
                var (args, func, item1, item2) = s;
                try
                {
                    // 在线程池线程上执行函数并设置结果
                    var result = func(item1, item2);
                    args.SetResult(result);
                }
                catch (Exception ex)
                {
                    // 如果函数执行引发异常，则设置异常
                    args.SetException(ex);
                }
            }, state, false);
            return this;
        }

        /// <summary>
        /// 标记操作因异常而失败。
        /// </summary>
        /// <param name="error">导致失败的异常。</param>
        public AwaitableEventArgs<T> SetException(Exception error)
        {
            vts.SetException(error);
            return this;
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
                vts.Reset();
                _pool.Release(this);
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