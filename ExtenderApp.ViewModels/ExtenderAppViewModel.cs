using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.ViewModels
{
    /// <summary>
    /// 提供视图模型核心功能的抽象基类。 包含服务访问、生命周期管理、导航、日志、数据持久化、调度等通用功能，供具体 ViewModel 继承使用。
    /// </summary>
    public abstract class ExtenderAppViewModel : DisposableObject, IViewModel, INotifyPropertyChanged, ILogger
    {
        /// <summary>
        /// 注入的服务提供者（可为插件作用域的 IServiceProvider）。 通过 <see cref="Inject"/> 设置。
        /// </summary>
        private IServiceProvider? serviceProvider;

        /// <summary>
        /// 内部持有的日志记录器实例，按 ViewModel 类型延迟解析。
        /// </summary>
        private ILogger? logger;

        /// <summary>
        /// 获取当前视图模型专用的日志记录器。如果尚未创建，会通过服务提供者按当前类型解析 <see cref="ILogger{T}"/>。
        /// </summary>
        protected ILogger Logger
        {
            get
            {
                if (logger is null)
                {
                    var loggerType = typeof(ILogger<>).MakeGenericType(GetType());
                    logger = GetService(loggerType) as ILogger;
                }
                return logger!;
            }
        }

        /// <summary>
        /// 与当前视图模型关联的插件详细信息（若此视图模型在插件作用域内解析则非空）。
        /// </summary>
        protected PluginDetails? Details { get; set; }

        /// <summary>
        /// 获取当前应用主窗口的引用（如果存在）。 从 <see cref="IMainWindowService"/> 的 <see cref="IMainWindowService.CurrentMainWindow"/> 获取。
        /// </summary>
        protected IMainWindow? MainWindow => GetRequiredService<IMainWindowService>().CurrentMainWindow;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发 <see cref="PropertyChanged"/> 事件，通知UI属性已更新。
        /// </summary>
        /// <param name="propertyName">已更改的属性的名称。该参数是可选的，可由调用方自动提供。</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 将运行时的 <see cref="IServiceProvider"/> 注入到当前视图模型。 此方法通常由视图/视图工厂在创建视图模型实例后调用以设置上下文服务提供者。
        /// </summary>
        /// <param name="serviceProvider">要注入的服务提供者，不能为空。</param>
        public virtual void Inject(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            Details = GetService<PluginDetails>();
        }

        public void Inject(object serviceProvider)
        {
            if (serviceProvider is IServiceProvider sp)
            {
                Inject(sp);
            }
        }

        #region Log

        private const string InfoEmptyMessage = "输出空信息日志";
        private const string DebugEmptyMessage = "输出空调试日志";
        private const string WarningEmptyMessage = "输出空警告日志";
        private const string ErrorEmptyMessage = "输出空错误日志";

        /// <summary>
        /// 记录信息级别日志。
        /// </summary>
        /// <param name="message">日志内容对象，可为 null。</param>
        public void LogInformation(object message)
        {
            Logger.LogInformation(message?.ToString() ?? InfoEmptyMessage);
        }

        /// <summary>
        /// 记录调试级别日志。
        /// </summary>
        /// <param name="message">日志内容对象，可为 null。</param>
        public void LogDebug(object message)
        {
            Logger.LogDebug(message?.ToString() ?? DebugEmptyMessage);
        }

        /// <summary>
        /// 记录错误级别日志，并附带异常。
        /// </summary>
        /// <param name="exception">相关异常。</param>
        /// <param name="message">错误消息对象，可为 null。</param>
        public void LogError(Exception exception, object message)
        {
            Logger.LogError(exception, message?.ToString() ?? ErrorEmptyMessage);
        }

        /// <summary>
        /// 记录警告级别日志。
        /// </summary>
        /// <param name="message">日志内容对象，可为 null。</param>
        public void LogWarning(object message)
        {
            Logger.LogWarning(message?.ToString() ?? WarningEmptyMessage);
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logger.Log(logLevel, eventId, state, exception, formatter);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return Logger.IsEnabled(logLevel);
        }

        /// <summary>
        /// 开始一个日志作用域。
        /// </summary>
        /// <typeparam name="TState">作用域状态类型。</typeparam>
        /// <param name="state">作用域状态。</param>
        /// <returns>返回 IDisposable，在 Dispose 时结束作用域。</returns>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return Logger.BeginScope(state);
        }

        #endregion Log

        #region Dispatcher

        /// <summary>
        /// 在 UI 线程上同步执行指定回调。
        /// </summary>
        /// <param name="callback">要执行的回调。</param>
        protected void DispatcherInvoke(Action callback)
        {
            if (callback is null)
                return;

            GetRequiredService<IDispatcherService>().Invoke(callback);
        }

        /// <summary>
        /// 在 UI 线程上异步执行指定回调。
        /// </summary>
        /// <param name="callback">要执行的回调。</param>
        protected void DispatcherInvokeAsync(Action callback)
        {
            if (callback is null)
                return;

            GetRequiredService<IDispatcherService>().InvokeAsync(callback);
        }

        /// <summary>
        /// 检查当前线程是否可访问 UI Dispatcher。
        /// </summary>
        /// <returns>如果可访问主线程 Dispatcher 则返回 true。</returns>
        protected bool CheckAccess()
        {
            return GetRequiredService<IDispatcherService>().CheckAccess();
        }

        /// <summary>
        /// 创建一个 awaitable，用于将执行切换到主 UI 线程。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>返回一个 <see cref="ThreadSwitchAwaitable"/>。</returns>
        protected ThreadSwitchAwaitable ToMainThreadAsync(CancellationToken token = default)
        {
            return GetRequiredService<IDispatcherService>().ToMainThreadAsync(token);
        }

        /// <summary>
        /// 创建一个 awaitable，用于将执行切换到后台线程。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>返回一个 <see cref="ThreadSwitchAwaitable"/>。</returns>
        protected ThreadSwitchAwaitable AwayMainThreadAsync(CancellationToken token = default)
        {
            return GetRequiredService<IDispatcherService>().AwayMainThreadAsync(token);
        }

        #endregion Dispatcher

        #region MainWindow

        /// <summary>
        /// 将主窗口临时置顶以吸引用户注意。 实现为异步短暂置顶（300ms）。
        /// </summary>
        protected void MainWindowTopmost()
        {
            Task.Run(async () =>
            {
                if (MainWindow == null) return;

                await ToMainThreadAsync();

                MainWindow.Topmost = true;
                await Task.Delay(300).ConfigureAwait(true);
                MainWindow.Topmost = false;
            });
        }

        #endregion MainWindow

        #region Plugin

        /// <summary>
        /// 异步加载插件（委托给 IPluginService）。
        /// </summary>
        /// <param name="plugin">要加载的插件信息。</param>
        protected async Task LoadPluginAsync(PluginDetails plugin)
        {
            await GetRequiredService<IPluginService>().LoadPluginAsync(plugin);
        }

        /// <summary>
        /// 卸载插件（委托给 IPluginService）。
        /// </summary>
        /// <param name="plugin">要卸载的插件信息。</param>
        protected void UnLoadPlugin(PluginDetails plugin)
        {
            GetRequiredService<IPluginService>().UnloadPlugin(plugin);
        }

        #endregion Plugin

        #region System

        /// <summary>
        /// 将指定文本写入系统剪贴板（通过 ISystemService）。
        /// </summary>
        /// <param name="text">要复制的文本。</param>
        protected void ClipboardSetText(string text)
        {
            GetRequiredService<ISystemService>().Clipboard.SetText(text);
        }

        #endregion System

        #region Message

        /// <summary>
        /// 订阅指定类型的消息，消息分发由 IMessageService 负责。
        /// </summary>
        /// <typeparam name="TMessage">消息类型。</typeparam>
        /// <param name="handleMessage">处理回调。</param>
        protected void SubscribeMessage<TMessage>(EventHandler<TMessage> handleMessage)
        {
            GetRequiredService<IMessageService>().Subscribe(this, handleMessage);
        }

        /// <summary>
        /// 使用消息名称订阅消息，支持弱命名或自定义消息通道。
        /// </summary>
        /// <param name="messageName">消息名称。</param>
        /// <param name="handleMessage">处理回调。</param>
        protected void SubscribeMessage(string messageName, EventHandler<object> handleMessage)
        {
            GetRequiredService<IMessageService>().Subscribe(messageName, this, handleMessage);
        }

        /// <summary>
        /// 发布一条消息到消息系统。
        /// </summary>
        /// <typeparam name="TMessage">消息类型。</typeparam>
        /// <param name="message">消息实例。</param>
        protected void PublishMessage<TMessage>(TMessage message)
        {
            GetRequiredService<IMessageService>().Publish(this, message);
        }

        /// <summary>
        /// 取消订阅指定类型的消息。
        /// </summary>
        /// <typeparam name="TMessage">消息类型。</typeparam>
        /// <param name="handleMessage">之前注册的处理回调。</param>
        protected void UnsubscribeMessage<TMessage>(EventHandler<TMessage> handleMessage)
        {
            GetRequiredService<IMessageService>().Unsubscribe(this, handleMessage);
        }

        #endregion Message

        #region Service

        /// <summary>
        /// 通过类型从注入的 <see cref="IServiceProvider"/> 获取服务（可返回 null）。
        /// </summary>
        /// <param name="serviceType">要获取的服务类型。</param>
        /// <returns>服务实例或 null（若未注册或未注入 serviceProvider）。</returns>
        protected object? GetService(Type serviceType)
        {
            return serviceProvider?.GetService(serviceType);
        }

        /// <summary>
        /// 通过泛型类型解析并返回一个 ViewModel 实例（使用容器解析，推荐尽量使用构造注入代替）。
        /// </summary>
        /// <typeparam name="T">要解析的视图模型类型。</typeparam>
        /// <returns>解析得到的视图模型实例。</returns>
        protected T GetViewModel<T>() where T : class, IViewModel
        {
            return GetRequiredService<T>();
        }

        /// <summary>
        /// 通过泛型类型解析并返回服务实例（可为 null）。
        /// </summary>
        /// <typeparam name="T">要解析的服务类型。</typeparam>
        /// <returns>解析得到的服务实例或 null。</returns>
        protected T? GetService<T>() where T : class
        {
            return serviceProvider?.GetService<T>();
        }

        /// <summary>
        /// 获取必需服务实例（若未注册或未注入服务提供者则抛出 <see cref="ArgumentNullException"/>）。
        /// </summary>
        /// <typeparam name="T">要解析的服务类型。</typeparam>
        /// <returns>解析得到的服务实例。</returns>
        /// <exception cref="ArgumentNullException">当未注入 <see cref="IServiceProvider"/> 时抛出。</exception>
        protected T GetRequiredService<T>() where T : class
        {
            return serviceProvider?.GetRequiredService<T>() ??
                throw new ArgumentNullException("当前未设置服务提供者，如果需要获取服务请使用AddViewModel来添加视图模型");
        }

        #endregion Service

        #region Navigation

        /// <summary>
        /// 导航到指定视图类型，scope 用于标识目标解析作用域。
        /// </summary>
        /// <param name="viewType">目标视图类型。</param>
        /// <param name="scope">解析作用域名称（可为空）。</param>
        protected void NavigateTo(Type viewType, string scope)
        {
            GetRequiredService<INavigationService>().NavigateTo(viewType, scope);
        }

        #endregion Navigation
    }
}