using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using ExtenderApp.Buffer;

namespace ExtenderApp.Common.Threads
{
    /// <summary>
    /// 可等待的事件参数，支持对象池复用并实现 <see cref="IValueTaskSource"/>.
    /// </summary>
    public sealed class AwaitableEventArgs : IValueTaskSource, IThreadPoolWorkItem
    {
        /// <summary>
        /// 可等待事件参数对象池。
        /// </summary>
        private static readonly ObjectPool<AwaitableEventArgs> _pool
            = ObjectPool.Create<AwaitableEventArgs>(() => new());

        /// <summary>
        /// 从对象池中获取一个 <see cref="AwaitableEventArgs"/> 实例。
        /// </summary>
        /// <returns><see cref="AwaitableEventArgs"/> 实例</returns>
        public static AwaitableEventArgs GetAwaitable()
        {
            var args = _pool.Get();
            args.IsActive = true;
            return args;
        }

        /// <summary>
        /// 创建并设置无参数回调的可等待实例。
        /// </summary>
        /// <param name="action">要执行的回调。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs FromResult(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return GetAwaitable().SetResult(action);
        }

        /// <summary>
        /// 创建并设置带单参数回调的可等待实例。
        /// </summary>
        /// <typeparam name="T">参数类型。</typeparam>
        /// <param name="action">要执行的回调。</param>
        /// <param name="item">回调参数。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs FromResult<T>(Action<T> action, T item)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return GetAwaitable().SetResult(action, item);
        }

        /// <summary>
        /// 创建并设置带双参数回调的可等待实例。
        /// </summary>
        /// <typeparam name="T1">第一个参数类型。</typeparam>
        /// <typeparam name="T2">第二个参数类型。</typeparam>
        /// <param name="action">要执行的回调。</param>
        /// <param name="item1">第一个参数。</param>
        /// <param name="item2">第二个参数。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs FromResult<T1, T2>(Action<T1, T2> action, T1 item1, T2 item2)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return GetAwaitable().SetResult(action, item1, item2);
        }

        /// <summary>
        /// 创建并设置异常结果的可等待实例。
        /// </summary>
        /// <param name="error">异常信息。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs FromException(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            return GetAwaitable().SetException(error);
        }

        /// <summary>
        /// 创建并设置结果的可等待实例。
        /// </summary>
        /// <typeparam name="T">结果类型。</typeparam>
        /// <param name="response">结果值。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs<T> FromResult<T>(T response)
        {
            return AwaitableEventArgs<T>.GetAwaitable().SetResult(response);
        }

        /// <summary>
        /// 创建并设置用于生成结果的函数回调。
        /// </summary>
        /// <typeparam name="T">结果类型。</typeparam>
        /// <param name="func">生成结果的函数。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs<T> FromResult<T>(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return AwaitableEventArgs<T>.GetAwaitable().SetResult(func);
        }

        /// <summary>
        /// 创建并设置带单参数函数回调。
        /// </summary>
        /// <typeparam name="T">结果类型。</typeparam>
        /// <typeparam name="T1">参数类型。</typeparam>
        /// <param name="func">生成结果的函数。</param>
        /// <param name="item1">函数参数。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs<T> FromResult<T1, T>(Func<T1, T> func, T1 item1)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return AwaitableEventArgs<T>.GetAwaitable().SetResult(func, item1);
        }

        /// <summary>
        /// 创建并设置带双参数函数回调。
        /// </summary>
        /// <typeparam name="T">结果类型。</typeparam>
        /// <typeparam name="T1">第一个参数类型。</typeparam>
        /// <typeparam name="T2">第二个参数类型。</typeparam>
        /// <param name="func">生成结果的函数。</param>
        /// <param name="item1">第一个参数。</param>
        /// <param name="item2">第二个参数。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs<T> FromResult<T1, T2, T>(Func<T1, T2, T> func, T1 item1, T2 item2)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return AwaitableEventArgs<T>.GetAwaitable().SetResult(func, item1, item2);
        }

        /// <summary>
        /// 创建并设置异常结果的可等待实例。
        /// </summary>
        /// <typeparam name="T">结果类型。</typeparam>
        /// <param name="error">异常信息。</param>
        /// <returns>可等待实例。</returns>
        public static AwaitableEventArgs<T> FromException<T>(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            return AwaitableEventArgs<T>.GetAwaitable().SetException(error);
        }

        /// <summary>
        /// 多线程重置值任务源的核心实现。
        /// </summary>
        private ManualResetValueTaskSourceCore<bool> vts;

        /// <summary>
        /// 数据缓冲区，用于在线程池线程中传递状态。
        /// </summary>
        private ValueCache? buffer;

        /// <summary>
        /// 数据缓冲回调，用于在线程池线程中执行操作。
        /// </summary>
        private Action<ValueCache>? callback;

        /// <summary>
        /// 标记当前实例是否已作为 awaiter 使用。
        /// </summary>
        private bool hasAwaiter;

        /// <summary>
        /// 表示当前实例是否处于激活（可等待）状态。由池获取时设置为 <c>true</c>，在获取结果后重置为 <c>false</c>。
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// 获取当前操作的版本令牌，用于向 <see cref="ValueTask"/> 验证此源的有效性。
        /// </summary>
        public short Version => vts.Version;

        /// <summary>
        /// 获取当前操作是否已完成。
        /// </summary>
        public bool IsCompleted => GetStatus(Version) != ValueTaskSourceStatus.Pending;

        /// <summary>
        /// 等待链表的下一个节点。
        /// </summary>
        private AwaitableEventArgs? next;

        /// <summary>
        /// 初始化 <see cref="AwaitableEventArgs"/> 的新实例。
        /// </summary>
        private AwaitableEventArgs()
        {
            vts = new();
            hasAwaiter = false;
            IsActive = false;
        }

        /// <summary>
        /// 标记操作成功完成。
        /// </summary>
        public AwaitableEventArgs SetResult()
        {
            next?.SetResult();
            vts.SetResult(true);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个无参数的委托。 这允许将工作卸载到后台线程。
        /// </summary>
        /// <param name="action">要在线程池中执行的委托。</param>
        public AwaitableEventArgs SetResult(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            callback = Execute;
            buffer = ValueCache.FromValue(this, action);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs args) ||
                    !buffer.TryGetValue(out Action action))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    action.Invoke();
                    args.SetResult();
                    buffer.Release();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个带有一个参数的委托。 此重载通过传递状态参数来避免创建闭包，从而减少内存分配。
        /// </summary>
        /// <typeparam name="T">传递给委托的参数类型。</typeparam>
        /// <param name="action">要在线程池中执行的委托。</param>
        /// <param name="item">要传递给委托的参数值。</param>
        public AwaitableEventArgs SetResult<T>(Action<T> action, T item)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            callback = Execute;
            buffer = ValueCache.FromValue(this, action, item);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs args) ||
                    !buffer.TryGetValue(out Action<T> action) ||
                    !buffer.TryGetValue(out T item))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    action.Invoke(item);
                    args.SetResult();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个带有两个参数的委托。 此重载通过传递状态参数来避免创建闭包，从而减少内存分配。
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

            callback = Execute;
            buffer = ValueCache.FromValue(this, action, item1, item2);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs args) ||
                    !buffer.TryGetValue(out Action<T1, T2> action) ||
                    !buffer.TryGetValue(out T1 item1) ||
                    !buffer.TryGetValue(out T2 item2))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    action.Invoke(item1, item2);
                    args.SetResult();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个无参数的委托。 这允许将工作卸载到后台线程。
        /// </summary>
        /// <param name="action">要在线程池中执行的委托。</param>
        /// <param name="token">传递给委托的取消令牌。</param>
        public AwaitableEventArgs SetResult(Action<CancellationToken> action, CancellationToken token)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            callback = Execute;
            buffer = ValueCache.FromValue(this, action, token);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs args) ||
                    !buffer.TryGetValue(out Action<CancellationToken> action) ||
                    !buffer.TryGetValue(out CancellationToken token))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    action.Invoke(token);
                    args.SetResult();
                    buffer.Release();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个带有一个参数的委托。 此重载通过传递状态参数来避免创建闭包，从而减少内存分配。
        /// </summary>
        /// <typeparam name="T">传递给委托的参数类型。</typeparam>
        /// <param name="action">要在线程池中执行的委托。</param>
        /// <param name="item">要传递给委托的参数值。</param>
        /// <param name="token">传递给委托的取消令牌。</param>
        public AwaitableEventArgs SetResult<T>(Action<T, CancellationToken> action, T item, CancellationToken token)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            callback = Execute;
            buffer = ValueCache.FromValue(this, action, item, token);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs args) ||
                    !buffer.TryGetValue(out Action<T, CancellationToken> action) ||
                    !buffer.TryGetValue(out T item) ||
                    !buffer.TryGetValue(out CancellationToken token))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    action.Invoke(item, token);
                    args.SetResult();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作成功完成，并在线程池中执行一个带有两个参数的委托。 此重载通过传递状态参数来避免创建闭包，从而减少内存分配。
        /// </summary>
        /// <typeparam name="T1">传递给委托的第一个参数类型。</typeparam>
        /// <typeparam name="T2">传递给委托的第二个参数类型。</typeparam>
        /// <param name="action">要在线程池中执行的委托。</param>
        /// <param name="item1">要传递给委托的第一个参数值。</param>
        /// <param name="item2">要传递给委托的第二个参数值。</param>
        /// <param name="token">传递给委托的取消令牌。</param>
        public AwaitableEventArgs SetResult<T1, T2>(Action<T1, T2, CancellationToken> action, T1 item1, T2 item2, CancellationToken token)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            callback = Execute;
            buffer = ValueCache.FromValue(this, action, item1, item2, token);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs args) ||
                    !buffer.TryGetValue(out Action<T1, T2, CancellationToken> action) ||
                    !buffer.TryGetValue(out T1 item1) ||
                    !buffer.TryGetValue(out T2 item2) ||
                    !buffer.TryGetValue(out CancellationToken token))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    action.Invoke(item1, item2, token);
                    args.SetResult();
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作因异常而失败。
        /// </summary>
        /// <param name="error">导致失败的异常。</param>
        public AwaitableEventArgs SetException(Exception error)
        {
            next?.SetException(error);
            vts.SetException(error);
            return this;
        }

        /// <summary>
        /// 重置当前实例并释放到对象池。
        /// </summary>
        private void Release()
        {
            buffer?.Release();
            buffer = null;
            callback = null;

            next = null;
            hasAwaiter = false;

            IsActive = false;
            vts.Reset();
            _pool.Release(this);
        }

        /// <summary>
        /// 追加等待节点到链尾。
        /// </summary>
        /// <param name="newArgs">要追加的等待节点。</param>
        private void AppendNext(AwaitableEventArgs newArgs)
        {
            var current = this;
            while (true)
            {
                var currentNext = current.next;
                if (currentNext == null)
                {
                    if (Interlocked.CompareExchange(ref current.next, newArgs, null) == null)
                        return;
                }
                else
                {
                    current = currentNext;
                }
            }
        }

        /// <summary>
        /// 获取链式等待节点。
        /// </summary>
        /// <returns>追加的等待节点。</returns>
        private AwaitableEventArgs GetChainedAwaitable()
        {
            var newArgs = GetAwaitable();

            if (!IsCompleted && IsActive)
                AppendNext(newArgs);
            else
                newArgs.SetResult();

            return newArgs;
        }

        /// <summary>
        /// 获取可等待对象的 awaiter。
        /// </summary>
        /// <returns>当前实例或追加节点的 awaiter。</returns>
        public ValueTaskAwaiter GetAwaiter()
            => GetValueTask().GetAwaiter();

        /// <summary>
        /// 获取用于 await 的 <see cref="ValueTask"/>。 如果当前实例已完成或未激活，则返回已完成的任务。
        /// </summary>
        public ValueTask GetValueTask()
        {
            if (IsCompleted)
            {
                // 已完成时立即读取结果并释放实例，保持与泛型行为一致
                GetResult(Version);
                return ValueTask.CompletedTask;
            }

            if (!IsActive)
                return ValueTask.CompletedTask;

            if (!hasAwaiter)
            {
                hasAwaiter = true;
                return new(this, Version);
            }

            return GetChainedAwaitable();
        }

        /// <summary>
        /// 获取操作结果并在完成后重置实例。
        /// </summary>
        /// <param name="token">用于验证任务源的版本令牌。</param>
        public void GetResult(short token)
        {
            try
            {
                vts.GetResult(token);
            }
            finally
            {
                // 获取结果后立即重置，为下一次操作做准备
                Release();
            }
        }

        /// <summary>
        /// 获取当前操作状态。
        /// </summary>
        /// <param name="token">用于验证任务源的版本令牌。</param>
        /// <returns>任务源状态。</returns>
        public ValueTaskSourceStatus GetStatus(short token)
        {
            return vts.GetStatus(token);
        }

        /// <summary>
        /// 注册完成回调。
        /// </summary>
        /// <param name="continuation">完成后执行的回调。</param>
        /// <param name="state">回调状态。</param>
        /// <param name="token">用于验证任务源的版本令牌。</param>
        /// <param name="flags">完成回调配置。</param>
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            vts.OnCompleted(continuation, state, token, flags);
        }

        /// <summary>
        /// 在线程池中执行回调。
        /// </summary>
        public void Execute()
        {
            callback?.Invoke(buffer!);
        }

        /// <summary>
        /// 将当前实例转换为可等待的 <see cref="ValueTask"/>.
        /// </summary>
        /// <param name="args">要转换的实例。</param>
        public static implicit operator ValueTask(AwaitableEventArgs args)
            => args.GetValueTask();
    }

    /// <summary>
    /// 可等待的事件参数，支持对象池复用并实现 <see cref="IValueTaskSource{TResult}"/>.
    /// </summary>
    /// <typeparam name="T">异步操作返回的结果类型。</typeparam>
    public sealed class AwaitableEventArgs<T> : IValueTaskSource<T>, IThreadPoolWorkItem
    {
        /// <summary>
        /// 可等待事件参数对象池。
        /// </summary>
        private static readonly ObjectPool<AwaitableEventArgs<T>> _pool
             = ObjectPool.Create<AwaitableEventArgs<T>>(() => new());

        /// <summary>
        /// 从对象池中获取一个 <see cref="AwaitableEventArgs{T}"/> 实例。
        /// </summary>
        /// <returns><see cref="AwaitableEventArgs{T}"/> 实例</returns>
        public static AwaitableEventArgs<T> GetAwaitable()
        {
            var args = _pool.Get();
            // 标记为激活状态，表示该实例当前可用于 await
            args.IsActive = true;
            return args;
        }

        /// <summary>
        /// 多线程重置值任务源的核心实现。
        /// </summary>
        private ManualResetValueTaskSourceCore<T> vts;

        /// <summary>
        /// 数据缓冲区，用于在线程池线程中传递状态。
        /// </summary>
        private ValueCache? buffer;

        /// <summary>
        /// 数据缓冲回调，用于在线程池线程中执行操作。
        /// </summary>
        private Action<ValueCache>? callback;

        /// <summary>
        /// 标记当前实例是否已作为 awaiter 使用。
        /// </summary>
        private bool hasAwaiter;

        /// <summary>
        /// 等待链表的下一个节点。
        /// </summary>
        private AwaitableEventArgs<T>? next;

        /// <summary>
        /// 表示当前实例是否处于激活（可等待）状态。由池获取时设置为 <c>true</c>，在获取结果后重置为 <c>false</c>。
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// 获取当前操作是否已完成。
        /// </summary>
        public bool IsCompleted => GetStatus(Version) != ValueTaskSourceStatus.Pending;

        /// <summary>
        /// 获取当前操作的版本令牌，用于向 <see cref="ValueTask{TResult}"/> 验证此源的有效性。
        /// </summary>
        public short Version => vts.Version;

        /// <summary>
        /// 初始化 <see cref="AwaitableEventArgs{T}"/> 的新实例。
        /// </summary>
        private AwaitableEventArgs()
        {
            vts = new();
            hasAwaiter = false;
            IsActive = false;
        }

        /// <summary>
        /// 标记操作成功完成并设置结果。
        /// </summary>
        /// <param name="response">操作的结果。</param>
        public AwaitableEventArgs<T> SetResult(T response)
        {
            next?.SetResult(response);
            vts.SetResult(response);
            return this;
        }

        /// <summary>
        /// 标记操作成功完成，并将结果设置为 <typeparamref name="T"/> 的默认值。
        /// </summary>
        public AwaitableEventArgs<T> SetResult()
        {
            return SetResult(default(T)!);
        }

        /// <summary>
        /// 标记操作成功完成，并使用指定函数生成的结果进行设置。 这允许延迟计算结果，直到需要完成操作时才执行。
        /// </summary>
        /// <param name="func">用于生成结果的函数。</param>
        public AwaitableEventArgs<T> SetResult(Func<T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            callback = Execute;
            buffer = ValueCache.FromValue(this, func);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs<T> args) ||
                    !buffer.TryGetValue(out Func<T> func))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    T result = func();
                    args.SetResult(result);
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作成功完成，并使用带一个参数的函数生成结果。 此重载通过传递状态参数 <paramref name="item1"/> 来避免创建闭包，从而减少内存分配。
        /// </summary>
        /// <typeparam name="T1">传递给函数的参数类型。</typeparam>
        /// <param name="func">用于生成结果的函数。</param>
        /// <param name="item1">要传递给 <paramref name="func"/> 的参数值。</param>
        public AwaitableEventArgs<T> SetResult<T1>(Func<T1, T> func, T1 item1)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            callback = Execute;
            buffer = ValueCache.FromValue(this, func, item1);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs<T> args) ||
                    !buffer.TryGetValue(out Func<T1, T> func) ||
                    !buffer.TryGetValue(out T1 item1))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    T result = func(item1);
                    args.SetResult(result);
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作成功完成，并使用带两个参数的函数生成结果。 此重载通过传递状态参数 <paramref name="item1"/> 和 <paramref name="item2"/> 来避免创建闭包，从而减少内存分配。
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

            callback = Execute;
            buffer = ValueCache.FromValue(this, func, item1, item2);

            ThreadPool.UnsafeQueueUserWorkItem(this, false);
            return this;

            static void Execute(ValueCache buffer)
            {
                if (!buffer.TryGetValue(out AwaitableEventArgs<T> args) ||
                    !buffer.TryGetValue(out Func<T1, T2, T> func) ||
                    !buffer.TryGetValue(out T1 item1) ||
                    !buffer.TryGetValue(out T2 item2))
                {
                    buffer.Release();
                    return;
                }

                try
                {
                    T result = func(item1, item2);
                    args.SetResult(result);
                }
                catch (Exception ex)
                {
                    args.SetException(ex);
                }
            }
        }

        /// <summary>
        /// 标记操作因异常而失败。
        /// </summary>
        /// <param name="error">导致失败的异常。</param>
        public AwaitableEventArgs<T> SetException(Exception error)
        {
            vts.SetException(error);
            next?.SetException(error);
            return this;
        }

        /// <summary>
        /// 重置当前实例的所有状态并将其释放回对象池。
        /// </summary>
        private void Release()
        {
            buffer?.Release();
            buffer = null;
            callback = null;

            next = null;
            hasAwaiter = false;

            IsActive = false;
            vts.Reset();
            _pool.Release(this);
        }

        /// <summary>
        /// 追加等待节点到链尾。
        /// </summary>
        /// <param name="newArgs">要追加的等待节点。</param>
        private void AppendNext(AwaitableEventArgs<T> newArgs)
        {
            var current = this;
            while (true)
            {
                var currentNext = current.next;
                if (currentNext == null)
                {
                    if (Interlocked.CompareExchange(ref current.next, newArgs, null) == null)
                        return;
                }
                else
                {
                    current = currentNext;
                }
            }
        }

        /// <summary>
        /// 获取链式等待节点。
        /// </summary>
        /// <returns>追加的等待节点。</returns>
        private AwaitableEventArgs<T> GetChainedAwaitable()
        {
            var newArgs = GetAwaitable();

            if (!IsCompleted && IsActive)
                AppendNext(newArgs);
            else
                newArgs.SetResult();

            return newArgs;
        }

        /// <summary>
        /// 获取用于 await 的 <see cref="ValueTask{T}"/>。 如果当前实例已完成或未激活，则返回已完成的结果（默认值）。
        /// </summary>
        public ValueTask<T> GetValueTask()
        {
            if (IsCompleted)
                return ValueTask.FromResult(GetResult(Version));

            if (!IsActive)
                return ValueTask.FromResult(default(T)!);

            if (!hasAwaiter)
            {
                hasAwaiter = true;
                return new(this, Version);
            }

            return GetChainedAwaitable();
        }

        /// <summary>
        /// 获取可等待对象的 awaiter。
        /// </summary>
        /// <returns>当前实例或追加节点的 awaiter。</returns>
        public ValueTaskAwaiter<T> GetAwaiter()
            => GetValueTask().GetAwaiter();

        /// <summary>
        /// 获取操作结果并在完成后重置实例。
        /// </summary>
        /// <param name="token">用于验证任务源的版本令牌。</param>
        /// <returns>异步操作结果。</returns>
        public T GetResult(short token)
        {
            try
            {
                return vts.GetResult(token);
            }
            finally
            {
                // 获取结果后立即重置，为下一次操作做准备
                Release();
            }
        }

        /// <summary>
        /// 获取当前操作状态。
        /// </summary>
        /// <param name="token">用于验证任务源的版本令牌。</param>
        /// <returns>任务源状态。</returns>
        public ValueTaskSourceStatus GetStatus(short token)
        {
            return vts.GetStatus(token);
        }

        /// <summary>
        /// 注册完成回调。
        /// </summary>
        /// <param name="continuation">完成后执行的回调。</param>
        /// <param name="state">回调状态。</param>
        /// <param name="token">用于验证任务源的版本令牌。</param>
        /// <param name="flags">完成回调配置。</param>
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            vts.OnCompleted(continuation, state, token, flags);
        }

        /// <summary>
        /// 在线程池中执行回调。
        /// </summary>
        public void Execute()
        {
            callback?.Invoke(buffer!);
        }

        /// <summary>
        /// 将当前实例转换为可等待的 <see cref="ValueTask{TResult}"/>.
        /// </summary>
        /// <param name="args">要转换的实例。</param>
        public static implicit operator ValueTask<T>(AwaitableEventArgs<T> args)
            => args.GetValueTask();
    }
}