using System.Windows.Threading;
using ExtenderApp.Abstract;

namespace ExtenderApp.Views
{
    /// <summary>
    /// WPF调度器服务实现类，用于在UI线程上执行操作
    /// 实现IDispatcherService接口，提供同步/异步调用方法
    /// </summary>
    internal class Dispatcher_WPF : IDispatcherService
    {
        /// <summary>
        /// WPF调度器实例，用于线程间操作调度
        /// </summary>
        private readonly Dispatcher _dispatcher;

        /// <summary>
        /// 日志服务接口，用于记录异常信息
        /// </summary>
        private readonly ILogingService _logingService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logingService">日志服务实例</param>
        public Dispatcher_WPF(ILogingService logingService)
        {
            // 获取当前UI线程的调度器
            _dispatcher = Dispatcher.CurrentDispatcher;
            _logingService = logingService;
        }

        /// <summary>
        /// 同步执行指定操作（在UI线程上）
        /// </summary>
        /// <param name="action">要在UI线程上执行的操作</param>
        /// <remarks>
        /// 如果当前线程不是UI线程，操作会被调度到UI线程执行并等待完成
        /// 执行过程中发生的异常会被捕获并记录到日志系统
        /// </remarks>
        public void Invoke(Action action)
        {
            try
            {
                _dispatcher.Invoke(action);
            }
            catch (Exception ex)
            {
                // 记录错误日志，包含异常信息和堆栈跟踪
                _logingService.Print(new Data.LogInfo(Data.LogLevel.ERROR,
                    nameof(IDispatcherService),
                    "调度器错误", ex));
            }
        }

        /// <summary>
        /// 异步执行指定操作（在UI线程上）
        /// </summary>
        /// <param name="action">要在UI线程上执行的操作</param>
        /// <remarks>
        /// 操作会被调度到UI线程执行但不等待完成
        /// 调用后会立即返回，适合不需要等待结果的场景
        /// 不捕获异常，异常可能由DispatcherUnhandledException处理
        /// </remarks>
        public void BeginInvoke(Action action)
        {
            try
            {
                _dispatcher.BeginInvoke(action);
            }
            catch (Exception ex)
            {
                // 记录错误日志，包含异常信息和堆栈跟踪
                _logingService.Print(new Data.LogInfo(Data.LogLevel.ERROR,
                    nameof(IDispatcherService),
                    "调度器错误", ex));
            }
        }

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            return _dispatcher.InvokeAsync(callback).Task;
        }

        public bool CheckAccess()
        {
            return _dispatcher.CheckAccess();
        }
    }
}
