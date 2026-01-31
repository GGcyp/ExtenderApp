using System.Collections.Concurrent;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.Services
{
    /// <summary>
    /// WPF 调度器服务实现类。用于在主（UI）线程与后台线程之间切换并在主线程上同步/异步执行操作。 实现 <see cref="IDispatcherService"/>，提供多种 Invoke/InvokeAsync 和线程切换方法。
    /// </summary>
    internal class DispatcherService : IDispatcherService
    {
        /// <summary>
        /// 日志服务接口，用于记录异常信息
        /// </summary>
        private readonly ILogger<IDispatcherService> _logeer;

        private readonly ConcurrentDictionary<DataBuffer, Action<DataBuffer>> _callbacks;
        private readonly SendOrPostCallback _postCallback;
        private readonly IMainThreadContext _mainThreadContext;
        private readonly Action<Action> _toMainThread;
        private readonly Action<Action> _awayMainThread;

        /// <summary>
        /// 获取主线程（UI 线程），如果尚未设置则为 <c>null</c>。
        /// </summary>
        public Thread? MainThread => _mainThreadContext.MainThread;

        /// <summary>
        /// 获取主线程的 <see cref="SynchronizationContext"/>，如果尚未设置则为 <c>null</c>。
        /// </summary>
        public SynchronizationContext? MainThreadContext => _mainThreadContext.Context;

        /// <summary>
        /// 初始化一个 <see cref="DispatcherService"/> 的新实例。
        /// </summary>
        /// <param name="logger">用于记录日志的 <see cref="ILogger{IDispatcherService}"/> 实例。</param>
        /// <param name="mainThreadContext">提供主线程和主线程同步上下文的 <see cref="IMainThreadContext"/> 实例。</param>
        public DispatcherService(ILogger<IDispatcherService> logger, IMainThreadContext mainThreadContext)
        {
            _logeer = logger;
            _mainThreadContext = mainThreadContext;
            _callbacks = new();
            _postCallback = new(InvokeDataBuffer);

            _toMainThread = InvokeAsync;
            _awayMainThread = InvokeBackgroundThread;
        }

        #region Invoke

        /// <inheritdoc/>
        public void Invoke(Action action)
        {
            if (action == null)
                return;

            var buffer = GetDataBuffer(action);
            Invoke(_postCallback, buffer);
        }

        /// <inheritdoc/>
        public void Invoke<T>(Action<T> action, T send)
        {
            if (action == null)
                return;

            var buffer = GetDataBuffer(action, send);
            Invoke(_postCallback, buffer);
        }

        /// <inheritdoc/>
        public void Invoke(SendOrPostCallback callback, object? obj)
        {
            try
            {
                if (MainThreadContext != null)
                {
                    MainThreadContext.Send(callback, obj);
                }
                else
                {
                    callback?.Invoke(obj);
                }
            }
            catch (Exception ex)
            {
                // 记录错误日志，包含异常信息和堆栈跟踪
                _logeer.LogError(ex, "调度器错误");
            }
        }

        /// <inheritdoc/>
        public void InvokeAsync(Action action)
        {
            if (action == null)
                return;

            var buffer = GetDataBuffer(action);
            InvokeAsync(_postCallback, buffer);
        }

        /// <inheritdoc/>
        public void InvokeAsync<T>(Action<T> action, T send)
        {
            if (action == null)
                return;

            var buffer = GetDataBuffer(action, send);
            InvokeAsync(_postCallback, buffer);
        }

        /// <inheritdoc/>
        public void InvokeAsync(SendOrPostCallback callback, object? obj)
        {
            try
            {
                if (MainThreadContext != null)
                {
                    MainThreadContext.Post(callback, obj);
                }
                else
                {
                    callback?.Invoke(obj);
                }
            }
            catch (Exception ex)
            {
                // 记录错误日志，包含异常信息和堆栈跟踪
                _logeer.LogError(ex, "调度器错误");
            }
        }

        /// <inheritdoc/>
        public async Task<TResult?> InvokeAsync<TResult>(Func<TResult> callback)
        {
            if (callback == null)
                return default;

            await SwitchToMainThreadAsync();
            TResult result = callback.Invoke();
            return result;
        }

        #endregion Invoke

        #region SwitchThreads

        /// <inheritdoc/>
        public ThreadSwitchAwaitable SwitchToMainThreadAsync(CancellationToken token = default)
        {
            return new ThreadSwitchAwaitable(_toMainThread, null, CheckAccess(), token);
        }

        /// <inheritdoc/>
        public ThreadSwitchAwaitable SwitchToBackgroundThreadAsync(CancellationToken token = default)
        {
            return new ThreadSwitchAwaitable(_awayMainThread, null, !CheckAccess(), token);
        }

        #endregion SwitchThreads

        /// <summary>
        /// 将指定操作排入线程池在后台线程执行。
        /// </summary>
        /// <param name="action">要执行的操作。</param>
        private void InvokeBackgroundThread(Action action)
        {
            ThreadPool.UnsafeQueueUserWorkItem(static state => ((Action)state!)(), action);
        }

        /// <summary>
        /// 为无参 <see cref="Action"/> 获取一个可复用的 <see cref="DataBuffer"/>，并注册其对应的执行回调。
        /// </summary>
        /// <param name="action">要封装到缓冲区中的操作。</param>
        /// <returns>封装了操作的 <see cref="DataBuffer"/> 实例。</returns>
        private DataBuffer GetDataBuffer(Action action)
        {
            var buffer = DataBuffer<Action>.Get(action);
            _callbacks.TryAdd(buffer, Invoke);
            return buffer;
        }

        /// <summary>
        /// 为带参 <see cref="Action{T}"/> 获取一个可复用的 <see cref="DataBuffer"/>，并注册其对应的执行回调。
        /// </summary>
        /// <typeparam name="T">参数类型。</typeparam>
        /// <param name="action">要封装到缓冲区中的操作。</param>
        /// <param name="send">要传递给操作的参数。</param>
        /// <returns>封装了操作与参数的 <see cref="DataBuffer"/> 实例。</returns>
        private DataBuffer GetDataBuffer<T>(Action<T> action, T send)
        {
            var buffer = DataBuffer<Action<T>, T>.Get(action, send);
            _callbacks.TryAdd(buffer, Invoke<T>);
            return buffer;
        }

        /// <summary>
        /// 从参数中解析 <see cref="DataBuffer"/>，查找并执行已注册的回调，然后释放缓冲区。
        /// </summary>
        /// <param name="obj">通过 <see cref="SynchronizationContext"/> 传递的对象，期望为 <see cref="DataBuffer"/>。</param>
        private void InvokeDataBuffer(object? obj)
        {
            if (obj is not DataBuffer buffer)
                return;

            if (_callbacks.Remove(buffer, out var callback))
                callback.Invoke(buffer);

            buffer.Release();
        }

        /// <summary>
        /// 执行无参缓冲区中的操作。
        /// </summary>
        /// <param name="buffer">包含 <see cref="Action"/> 的缓冲区。</param>
        private static void Invoke(DataBuffer buffer)
        {
            if (buffer is DataBuffer<Action> dataBuffer)
            {
                dataBuffer.Item1?.Invoke();
            }
        }

        /// <summary>
        /// 执行带参缓冲区中的操作。
        /// </summary>
        /// <typeparam name="T">参数类型。</typeparam>
        /// <param name="buffer">包含 <see cref="Action{T}"/> 与参数的缓冲区。</param>
        private static void Invoke<T>(DataBuffer buffer)
        {
            if (buffer is DataBuffer<Action<T>, T> dataBuffer)
            {
                dataBuffer.Item1?.Invoke(dataBuffer.Item2!);
            }
        }

        /// <inheritdoc/>
        public bool CheckAccess()
        {
            return Thread.CurrentThread == MainThread;
        }
    }
}