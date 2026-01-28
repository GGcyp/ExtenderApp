using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.Services
{
    /// <summary>
    /// WPF调度器服务实现类，用于在UI线程上执行操作 实现IDispatcherService接口，提供同步/异步调用方法
    /// </summary>
    internal class DispatcherService : IDispatcherService
    {
        /// <summary>
        /// 日志服务接口，用于记录异常信息
        /// </summary>
        private readonly ILogger<IDispatcherService> _logeer;

        private readonly IMainThreadContext _mainThreadContext;
        private readonly Action<Action> _toMainThread;
        private readonly Action<Action> _awayMainThread;

        public Thread? MainThread => _mainThreadContext.MainThread;
        public SynchronizationContext? MainThreadContext => _mainThreadContext.Context;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logingService">日志服务实例</param>
        public DispatcherService(ILogger<IDispatcherService> logger, IMainThreadContext mainThreadContext)
        {
            _logeer = logger;
            _mainThreadContext = mainThreadContext;

            _toMainThread = InvokeAsync;
            _awayMainThread = ToBackgroundThread;
        }

        #region Invoke

        public void Invoke(Action action)
        {
            if (action == null)
                return;
            Invoke(CreateCallback(action), null);
        }

        public void Invoke<T>(Action<T> action, T send)
        {
            if (action == null)
                return;
            Invoke(CreateCallback(action), send);
        }

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

        public void InvokeAsync(Action action)
        {
            if (action == null)
                return;
            InvokeAsync(CreateCallback(action), null);
        }

        public void InvokeAsync<T>(Action<T> action, T send)
        {
            if (action == null)
                return;
            InvokeAsync(CreateCallback(action), send);
        }

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

        public async Task<TResult?> InvokeAsync<TResult>(Func<TResult> callback)
        {
            if (callback == null)
                return default;

            await SwitchToMainThreadAsync();
            TResult result = callback.Invoke();
            await SwitchToBackgroundThreadAsync();
            return result;
        }

        #endregion Invoke

        #region SwitchThreads

        public ThreadSwitchAwaitable SwitchToMainThreadAsync(CancellationToken token = default)
        {
            return new ThreadSwitchAwaitable(_toMainThread, null, CheckAccess(), token);
        }

        public ThreadSwitchAwaitable SwitchToBackgroundThreadAsync(CancellationToken token = default)
        {
            return new ThreadSwitchAwaitable(_awayMainThread, null, !CheckAccess(), token);
        }

        #endregion SwitchThreads

        private SendOrPostCallback CreateCallback<T>(Action<T> action)
        {
            return new SendOrPostCallback(CallbackMethod(action)!);
        }

        private SendOrPostCallback CreateCallback(Action action)
        {
            return new SendOrPostCallback(CallbackMethod(action)!);
        }

        private Action<object> CallbackMethod(Action action)
        {
            return (obj) =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    _logeer.LogError(ex, "调度器错误");
                }
            };
        }

        private Action<object> CallbackMethod<T>(Action<T> action)
        {
            return (obj) =>
            {
                try
                {
                    action.Invoke((T)obj);
                }
                catch (Exception ex)
                {
                    _logeer.LogError(ex, "调度器错误");
                }
            };
        }

        private void ToBackgroundThread(Action action)
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ => action(), null);
        }

        public bool CheckAccess()
        {
            return Thread.CurrentThread == MainThread;
        }
    }
}