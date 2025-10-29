using System.Diagnostics;
using System.Transactions;
using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    /// <summary>
    /// WPF调度器服务实现类，用于在UI线程上执行操作
    /// 实现IDispatcherService接口，提供同步/异步调用方法
    /// </summary>
    internal class DispatcherService : IDispatcherService
    {
        /// <summary>
        /// 日志服务接口，用于记录异常信息
        /// </summary>
        private readonly ILogingService _logingService;
        private readonly IMainThreadContext _mainThreadContext;
        private readonly Action<Action> _toMainThread;
        private readonly Action<Action> _awayMainThread;

        public Thread? MainThread => _mainThreadContext.MainThread;
        public SynchronizationContext? MainThreadContext => _mainThreadContext.Context;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logingService">日志服务实例</param>
        public DispatcherService(ILogingService logingService, IMainThreadContext mainThreadContext)
        {
            _logingService = logingService;
            _mainThreadContext = mainThreadContext;

            _toMainThread = BeginInvoke;
            _awayMainThread = AwayMainThread;
        }

        #region Invoke

        public void Invoke(Action action)
        {
            if (action == null)
                return;
            Invoke(new SendOrPostCallback(_ => action()), null);
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
                _logingService.Error("调度器错误", nameof(IDispatcherService), ex);
            }
        }

        public void BeginInvoke(Action action)
        {
            if (action == null)
                return;
            BeginInvoke(new SendOrPostCallback(_ => action()), null);
        }

        public void BeginInvoke<T>(Action<T> action, T send)
        {
            if (action == null)
                return;
            BeginInvoke(CreateCallback(action), send);
        }

        public void BeginInvoke(SendOrPostCallback callback, object? obj)
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
                _logingService.Error("调度器错误", nameof(IDispatcherService), ex);
            }
        }

        public async Task<TResult?> InvokeAsync<TResult>(Func<TResult> callback)
        {
            if (callback == null)
                return default;

            await ToMainThreadAsync();
            TResult result = callback.Invoke();
            await AwayMainThreadAsync();
            return result;
        }

        #endregion

        #region SwitchThreads

        public ThreadSwitchAwaitable ToMainThreadAsync(CancellationToken token = default)
        {
            return new ThreadSwitchAwaitable(_toMainThread, null, CheckAccess(), token);
        }

        public ThreadSwitchAwaitable AwayMainThreadAsync(CancellationToken token = default)
        {
            return new ThreadSwitchAwaitable(_awayMainThread, null, !CheckAccess(), token);
        }

        #endregion

        private SendOrPostCallback CreateCallback<T>(Action<T> action)
        {
            return new SendOrPostCallback(CallbackMethod(action)!);
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
                    _logingService.Error("调度器错误", nameof(IDispatcherService), ex);
                }
            };
        }

        private void AwayMainThread(Action action)
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ => action(), null);
        }

        public bool CheckAccess()
        {
            return Thread.CurrentThread == MainThread;
        }
    }
}
