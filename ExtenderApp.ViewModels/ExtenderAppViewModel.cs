using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Common.Error;
using ExtenderApp.Data;
using ExtenderApp.Models;
using ExtenderApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.ViewModels
{
    /// <summary>
    /// 提供视图模型核心功能的抽象基类。 包含服务访问、生命周期管理、导航、日志、数据持久化等通用功能。
    /// </summary>
    public abstract class ExtenderAppViewModel : DisposableObject, IViewModel, INotifyPropertyChanged, ILogger
    {
        /// <summary>
        /// 提供对应用程序核心服务的访问，如导航、日志、本地数据等。
        /// </summary>
        protected readonly IServiceStore ServiceStore;

        private bool hasSubscribeMessage;

        /// <summary>
        /// 内部持有的日志记录器实例。
        /// </summary>
        private ILogger? logger;

        /// <summary>
        /// 获取当前视图模型专用的日志记录器。 如果尚未初始化，则会通过服务提供程序动态创建一个与当前视图模型类型关联的记录器。
        /// </summary>
        protected ILogger Logger
        {
            get
            {
                if (logger is null)
                {
                    var loggerType = typeof(ILogger<>).MakeGenericType(GetType());
                    logger = ServiceStore.ServiceProvider.GetService(loggerType) as ILogger;
                }
                return logger!;
            }
        }

        /// <summary>
        /// 获取或设置与当前视图模型关联的插件的详细信息。 如果视图模型不属于插件，则此属性可能为 null。
        /// </summary>
        protected PluginDetails? Details { get; set; }

        /// <summary>
        /// 获取对应用程序主窗口的引用。
        /// </summary>
        protected IMainWindow? MainWindow => ServiceStore.MainWindowService.CurrentMainWindow;

        /// <summary>
        /// 当视图模型的属性值更改时发生。
        /// </summary>
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
        /// 初始化 <see cref="ExtenderAppViewModel"/> 的新实例，并注入指定的日志记录器。
        /// </summary>
        /// <param name="serviceStore">服务存储实例。</param>
        /// <param name="logger">日志记录器实例。</param>
        public ExtenderAppViewModel(IServiceStore serviceStore, ILogger logger) : this(serviceStore)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 初始化 <see cref="ExtenderAppViewModel"/> 的新实例。
        /// </summary>
        /// <param name="serviceStore">服务存储实例。</param>
        public ExtenderAppViewModel(IServiceStore serviceStore)
        {
            ServiceStore = serviceStore;
            Details = GetCurrentPluginDetails();
        }

        /// <summary>
        /// 当视图被注入时调用。派生类必须实现此方法以接收其关联的视图。
        /// </summary>
        /// <param name="view">被注入的视图实例。</param>
        public virtual void InjectView(IView view)
        {
        }

        /// <summary>
        /// 当导航进入此视图模型关联的视图时由框架调用。
        /// </summary>
        /// <param name="oldViewInfo">上一个视图的信息。</param>
        public void Enter(ViewInfo oldViewInfo)
        {
            EnterProtected(oldViewInfo);
        }

        /// <summary>
        /// 当导航离开此视图模型关联的视图时由框架调用。
        /// </summary>
        /// <param name="newViewInfo">将要导航到的新视图的信息。</param>
        public void Exit(ViewInfo newViewInfo)
        {
            ExitProtected(newViewInfo);

            if (hasSubscribeMessage)
                ServiceStore.MessageService.UnsubscribeAll(this);
        }

        /// <summary>
        /// 为派生类提供一个可重写的入口点，用于处理视图进入逻辑。
        /// </summary>
        /// <param name="oldViewInfo">上一个视图的信息。</param>
        protected virtual void EnterProtected(ViewInfo oldViewInfo)
        {
        }

        /// <summary>
        /// 为派生类提供一个可重写的入口点，用于处理视图退出逻辑。
        /// </summary>
        /// <param name="newViewInfo">将要导航到的新视图的信息。</param>
        protected virtual void ExitProtected(ViewInfo newViewInfo)
        {
        }

        /// <summary>
        /// 从服务存储中获取当前插件的详细信息。
        /// </summary>
        /// <returns>如果服务存储是插件服务存储，则返回插件详情；否则返回 null。</returns>
        private PluginDetails? GetCurrentPluginDetails()
        {
            if (ServiceStore is IPuginServiceStore store)
                return store.PuginDetails;
            return null;
        }

        /// <summary>
        /// 当关联的视图加载完成时调用。
        /// </summary>
        public virtual void OnViewloaded()
        {
        }

        /// <summary>
        /// 当关联的视图被卸载时调用。
        /// </summary>
        public virtual void OnViewUnloaded()
        {
        }

        #region Navigate

        /// <summary>
        /// 导航到指定的视图类型。作用域将根据当前插件上下文自动确定。
        /// </summary>
        /// <typeparam name="TView">目标视图的类型，必须实现 <see cref="IView"/>。</typeparam>
        /// <returns>导航成功则返回目标视图的实例，否则返回 null。</returns>
        protected TView? NavigateTo<TView>()
            where TView : class, IView
        {
            string scope = Details is null ? string.Empty : Details.PluginScopeName;
            return NavigateTo(typeof(TView), scope) as TView;
        }

        /// <summary>
        /// 从指定的旧视图导航到新的视图类型。
        /// </summary>
        /// <typeparam name="TView">目标视图类型，必须实现 <see cref="IView"/>。</typeparam>
        /// <param name="oldView">从中发起导航的旧视图实例。</param>
        /// <returns>导航成功则返回目标视图的实例，否则返回 null。</returns>
        protected TView? NavigateTo<TView>(IView oldView)
            where TView : class, IView
        {
            string scope = Details is null ? string.Empty : Details.PluginScopeName;
            return NavigateTo(typeof(TView), scope, oldView) as TView;
        }

        /// <summary>
        /// 导航到指定作用域下的目标视图类型。
        /// </summary>
        /// <typeparam name="TView">目标视图类型，必须实现 <see cref="IView"/>。</typeparam>
        /// <param name="scope">导航的目标作用域标识。</param>
        /// <returns>导航成功则返回目标视图实例，否则返回 null。</returns>
        protected TView? NavigateTo<TView>(string scope) where TView : class, IView
        {
            return NavigateTo(typeof(TView), scope) as TView;
        }

        /// <summary>
        /// 根据插件详情导航到其启动视图类型。
        /// </summary>
        /// <typeparam name="TView">目标视图类型，必须实现 <see cref="IView"/>。</typeparam>
        /// <param name="modDetails">包含作用域和启动视图信息的插件详情对象。</param>
        /// <returns>导航成功则返回目标视图实例，否则返回 null。</returns>
        protected TView? NavigateTo<TView>(PluginDetails modDetails) where TView : class, IView
        {
            return NavigateTo<TView>(modDetails.PluginScopeName);
        }

        /// <summary>
        /// 根据插件详情导航到其启动视图。
        /// </summary>
        /// <param name="modDetails">包含作用域和启动视图信息的插件详情对象。</param>
        /// <returns>导航成功则返回目标视图实例，如果参数无效或导航失败则返回 null。</returns>
        protected IView? NavigateTo(PluginDetails modDetails)
        {
            try
            {
                modDetails.ArgumentNull(nameof(modDetails));
                modDetails.StartupType.ArgumentObjectNull(nameof(modDetails));
            }
            catch (ArgumentNullException ex)
            {
                LogError(ex, "导航参数不能为空！");
                return null;
            }

            return NavigateTo(modDetails.StartupType, modDetails.PluginScopeName);
        }

        /// <summary>
        /// 导航到指定类型和作用域的视图。
        /// </summary>
        /// <param name="targetView">目标视图的 <see cref="Type"/>。</param>
        /// <param name="scope">导航的目标作用域标识。</param>
        /// <returns>导航成功则返回目标视图实例，否则返回 null。</returns>
        protected virtual IView? NavigateTo(Type targetView, string scope)
        {
            IView? view = null;
            try
            {
                view = ServiceStore.NavigationService.NavigateTo(targetView, scope);
            }
            catch (Exception ex)
            {
                LogError(ex, "视图导航出现了问题！");
            }
            return view;
        }

        /// <summary>
        /// 从指定的旧视图导航到新的视图。
        /// </summary>
        /// <param name="targetView">目标视图的 <see cref="Type"/>。</param>
        /// <param name="scope">导航的目标作用域标识。</param>
        /// <param name="oldView">从中发起导航的旧视图实例。</param>
        /// <returns>导航成功则返回目标视图实例，否则返回 null。</returns>
        protected virtual IView? NavigateTo(Type targetView, string scope, IView oldView)
        {
            IView? view = null;
            try
            {
                view = ServiceStore.NavigationService.NavigateTo(targetView, scope, oldView);
            }
            catch (Exception ex)
            {
                LogError(ex, "视图导航出现了问题！");
            }
            return view;
        }

        /// <summary>
        /// 导航到一个新的窗口，并显示指定的视图类型。
        /// </summary>
        /// <typeparam name="TView">要在新窗口中显示的目标视图类型。</typeparam>
        /// <returns>导航成功则返回新创建的窗口实例，否则返回 null。</returns>
        protected virtual IWindow? NavigateToWindow<TView>()
            where TView : class, IView
        {
            IWindow window = null;
            try
            {
                string scope = Details is null ? string.Empty : Details.PluginScopeName;
                window = ServiceStore.NavigationService.NavigateToWindow<TView>(scope, null);
            }
            catch (Exception ex)
            {
                LogError(ex, "视图导航出现了问题！");
            }
            return window;
        }

        #endregion Navigate

        #region Log

        private const string InfoEmptyMessage = "输出空信息日志";
        private const string DebugEmptyMessage = "输出空调试日志";
        private const string WarningEmptyMessage = "输出空警告日志";
        private const string ErrorEmptyMessage = "输出空错误日志";

        /// <summary>
        /// 记录一条信息级别的日志。
        /// </summary>
        /// <param name="message">要记录的日志消息。</param>
        public void LogInformation(object message)
        {
            Logger.LogInformation(message?.ToString() ?? InfoEmptyMessage);
        }

        /// <summary>
        /// 记录一条调试级别的日志。
        /// </summary>
        /// <param name="message">要记录的调试消息。</param>
        public void LogDebug(object message)
        {
            Logger.LogDebug(message?.ToString() ?? DebugEmptyMessage);
        }

        /// <summary>
        /// 记录一条错误级别的日志，并包含异常信息。
        /// </summary>
        /// <param name="exception">要记录的异常。</param>
        /// <param name="message">要记录的错误消息。</param>
        public void LogError(Exception exception, object message)
        {
            Logger.LogError(exception, message?.ToString() ?? ErrorEmptyMessage);
        }

        /// <summary>
        /// 记录一条警告级别的日志。
        /// </summary>
        /// <param name="message">要记录的警告消息。</param>
        public void LogWarning(object message)
        {
            Logger.LogWarning(message?.ToString() ?? WarningEmptyMessage);
        }

        /// <summary>
        /// 使用指定的日志级别写入日志条目。
        /// </summary>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logger.Log(logLevel, eventId, state, exception, formatter);
        }

        /// <summary>
        /// 检查是否启用了给定的 <paramref name="logLevel"/>。
        /// </summary>
        /// <param name="logLevel">要检查的日志级别。</param>
        /// <returns>如果启用了 <paramref name="logLevel"/>，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return Logger.IsEnabled(logLevel);
        }

        /// <summary>
        /// 开始一个逻辑操作作用域。
        /// </summary>
        /// <typeparam name="TState">状态的类型。</typeparam>
        /// <param name="state">要开始的作用域的状态。</param>
        /// <returns>一个 <see cref="IDisposable"/> 对象，在释放时会结束逻辑操作作用域。</returns>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return Logger.BeginScope(state);
        }

        #endregion Log

        #region LocalData

        /// <summary>
        /// 从本地存储加载指定类型的数据。
        /// </summary>
        /// <typeparam name="T">要加载的数据的类型。</typeparam>
        /// <param name="data">加载成功时，此参数将包含加载的数据。</param>
        /// <param name="checkAction">加载后对 <see cref="LocalData{T}"/> 执行的额外检查或操作。</param>
        /// <returns>如果数据加载成功，则返回 true；否则返回 false。</returns>
        protected bool LoadLocalData<T>(out T? data, Action<LocalData<T>> checkAction = null)
            where T : class, new()
        {
            data = default;
            if (!ServiceStore.LocalDataService.LoadData(Details, out LocalData<T> localData))
                return false;

            data = localData.Data;
            checkAction?.Invoke(localData);
            return true;
        }

        /// <summary>
        /// 将指定类型的数据保存到本地存储。
        /// </summary>
        /// <typeparam name="T">要保存的数据的类型。</typeparam>
        /// <param name="data">要保存的数据实例。</param>
        /// <returns>如果数据保存成功，则返回 true；否则返回 false。</returns>
        protected bool SaveLocalData<T>(T? data)
            where T : class
        {
            if (data is null || Details is null)
                return false;

            return ServiceStore.LocalDataService.SaveData(Details, data);
        }

        /// <summary>
        /// 将指定版本的数据保存到本地存储。
        /// </summary>
        /// <typeparam name="T">要保存的数据的类型。</typeparam>
        /// <param name="data">要保存的数据实例。</param>
        /// <param name="version">要保存的数据的版本。</param>
        /// <returns>如果数据保存成功，则返回 true；否则返回 false。</returns>
        protected bool SaveLocalData<T>(T? data, Version version)
            where T : class
        {
            if (data is null || Details is null)
                return false;

            return ServiceStore.LocalDataService.SaveData(Details.Title, data, version);
        }

        /// <summary>
        /// 删除当前插件的本地数据。
        /// </summary>
        /// <returns>如果删除成功或无需删除，则返回 true；否则返回 false。</returns>
        protected bool DeleteLocalData()
        {
            if (Details is null)
                return false;
            return ServiceStore.LocalDataService.DeleteData(Details);
        }

        #endregion LocalData

        #region ScheduledTask

        /// <summary>
        /// 启动一个定时任务，在指定的延迟后开始，并按指定的周期重复。
        /// </summary>
        /// <param name="callback">要执行的回调操作。</param>
        /// <param name="state">传递给回调的状态对象。</param>
        /// <param name="dueTime">任务首次执行前的延迟时间。</param>
        /// <param name="period">任务执行之间的间隔时间。</param>
        /// <returns>一个 <see cref="ScheduledTask"/> 实例，可用于控制任务。</returns>
        protected ScheduledTask Start(Action<object> callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            ScheduledTask task = new ScheduledTask();
            task.Start(callback, state, dueTime, period);
            return task;
        }

        /// <summary>
        /// 启动一个按指定周期重复的循环任务。
        /// </summary>
        /// <param name="callback">每次周期要执行的回调操作。</param>
        /// <param name="period">任务执行之间的间隔时间。</param>
        /// <returns>一个 <see cref="ScheduledTask"/> 实例，可用于控制任务。</returns>
        protected ScheduledTask StartCycle(Action<object> callback, TimeSpan period)
        {
            return StartCycle(callback, null, period);
        }

        /// <summary>
        /// 启动一个按指定周期重复的循环任务，并传递状态对象。
        /// </summary>
        /// <param name="callback">每次周期要执行的回调操作。</param>
        /// <param name="state">传递给回调的状态对象。</param>
        /// <param name="period">任务执行之间的间隔时间。</param>
        /// <returns>一个 <see cref="ScheduledTask"/> 实例，可用于控制任务。</returns>
        protected ScheduledTask StartCycle(Action<object> callback, object state, TimeSpan period)
        {
            ScheduledTask task = new ScheduledTask();
            task.StartCycle(callback, state, period);
            return task;
        }

        /// <summary>
        /// 启动一个在指定延迟后执行一次的延迟任务。
        /// </summary>
        /// <param name="callback">要执行的回调操作。</param>
        /// <param name="dueTime">任务执行前的延迟时间。</param>
        /// <returns>一个 <see cref="ScheduledTask"/> 实例，可用于控制任务。</returns>
        protected ScheduledTask StartDelay(Action<object> callback, TimeSpan dueTime)
        {
            return StartDelay(callback, null, dueTime);
        }

        /// <summary>
        /// 启动一个在指定延迟后执行一次的延迟任务，并传递状态对象。
        /// </summary>
        /// <param name="callback">要执行的回调操作。</param>
        /// <param name="state">传递给回调的状态对象。</param>
        /// <param name="dueTime">任务执行前的延迟时间。</param>
        /// <returns>一个 <see cref="ScheduledTask"/> 实例，可用于控制任务。</returns>
        protected ScheduledTask StartDelay(Action<object> callback, object state, TimeSpan dueTime)
        {
            ScheduledTask task = new ScheduledTask();
            task.StartDelay(callback, state, dueTime);
            return task;
        }

        #endregion ScheduledTask

        #region Dispatcher

        /// <summary>
        /// 在UI线程上同步执行指定的操作。
        /// </summary>
        /// <param name="callback">要在UI线程上执行的操作。</param>
        protected void DispatcherInvoke(Action callback)
        {
            if (callback is null)
                return;

            ServiceStore.DispatcherService.Invoke(callback);
        }

        /// <summary>
        /// 在UI线程上异步执行指定的操作。
        /// </summary>
        /// <param name="callback">要在UI线程上执行的操作。</param>
        protected void DispatcherBeginInvoke(Action callback)
        {
            if (callback is null) return;
            ServiceStore.DispatcherService.InvokeAsync(callback);
        }

        /// <summary>
        /// 确定调用线程是否可以访问此 <see cref="Dispatcher"/> 对象。
        /// </summary>
        /// <returns>如果调用线程可以访问此对象，则为 true；否则为 false。</returns>
        protected bool CheckAccess()
        {
            return ServiceStore.DispatcherService.CheckAccess();
        }

        /// <summary>
        /// 创建一个可等待对象，将执行切换到主UI线程。
        /// </summary>
        /// <param name="token">用于取消操作的取消标记。</param>
        /// <returns>一个可等待的上下文切换对象。</returns>
        protected ThreadSwitchAwaitable ToMainThreadAsync(CancellationToken token = default)
        {
            return ServiceStore.DispatcherService.ToMainThreadAsync(token);
        }

        /// <summary>
        /// 创建一个可等待对象，将执行切换到后台线程池线程。
        /// </summary>
        /// <param name="token">用于取消操作的取消标记。</param>
        /// <returns>一个可等待的上下文切换对象。</returns>
        protected ThreadSwitchAwaitable AwayMainThreadAsync(CancellationToken token = default)
        {
            return ServiceStore.DispatcherService.AwayMainThreadAsync(token);
        }

        #endregion Dispatcher

        #region MainWindow

        /// <summary>
        /// 创建并返回一个新的主窗口实例。
        /// </summary>
        /// <returns>一个新的 <see cref="IMainWindow"/> 实例。</returns>
        protected IMainWindow CreateMainWindow()
        {
            return ServiceStore.MainWindowService.CreateMainWindow();
        }

        /// <summary>
        /// 临时将主窗口置于顶层，以吸引用户注意。
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

        #region Path

        /// <summary>
        /// 在文件资源管理器中打开指定的文件夹路径。
        /// </summary>
        /// <param name="path">要打开的文件夹的完整路径。</param>
        protected void OpenFolder(string? path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    try
                    {
                        // 启动资源管理器并打开指定目录
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = path,
                            UseShellExecute = true,
                            Verb = "open" // 确保以打开方式启动
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("无法打开指定的文件夹路径。", ex);
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException("指定的文件夹路径不存在。");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "打开路径失败：{path}", path);
            }
        }

        /// <summary>
        /// 在应用程序根目录下检查并创建指定的文件夹。
        /// </summary>
        /// <param name="folderPath">相对于应用程序根目录的文件夹路径。</param>
        /// <returns>创建的文件夹的完整路径，如果创建失败则返回空字符串。</returns>
        protected string CreateFolderPathForAppRootFolder(string folderPath)
        {
            try
            {
                return ProgramDirectory.ChekAndCreateFolder(folderPath);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "创建路径失败：{folderPath}", folderPath);
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取当前插件目录下指定子文件夹的完整路径。
        /// </summary>
        /// <param name="folderPath">插件目录下的子文件夹名称或相对路径。</param>
        /// <returns>完整的文件夹路径，如果插件信息不可用则返回空字符串。</returns>
        protected string? GetPluginFolder(string folderPath)
        {
            if (Details == null)
                return string.Empty;

            string path = Details.PluginFolderPath;
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return Path.Combine(path, folderPath);
        }

        #endregion Path

        #region Plugin

        /// <summary>
        /// 异步加载指定的插件。
        /// </summary>
        /// <param name="plugin">要加载的插件的详细信息。</param>
        protected async Task LoadPluginAsync(PluginDetails plugin)
        {
            await ServiceStore.PluginService.LoadPluginAsync(plugin);
        }

        /// <summary>
        /// 卸载指定的插件。
        /// </summary>
        /// <param name="plugin">要卸载的插件的详细信息。</param>
        protected void UnLoadPlugin(PluginDetails plugin)
        {
            ServiceStore.PluginService.UnloadPlugin(plugin);
        }

        #endregion Plugin

        #region System

        /// <summary>
        /// 将指定的文本设置到系统剪贴板。
        /// </summary>
        /// <param name="text">要设置到剪贴板的文本。</param>
        protected void ClipboardSetText(string text)
        {
            ServiceStore.SystemService.Clipboard.SetText(text);
        }

        #endregion System

        #region Message

        /// <summary>
        /// 订阅指定类型的消息。
        /// </summary>
        /// <typeparam name="TMessage">要订阅的消息的类型。</typeparam>
        /// <param name="handleMessage">处理消息的回调委托。</param>
        protected void SubscribeMessage<TMessage>(EventHandler<TMessage> handleMessage)
        {
            ServiceStore.MessageService.Subscribe(this, handleMessage);
            hasSubscribeMessage = true;
        }

        /// <summary>
        /// 订阅指定类型名称的消息。
        /// </summary>
        /// <param name="messageName">消息名称。</param>
        /// <param name="handleMessage">处理消息的回调委托。</param>
        protected void SubscribeMessage(string messageName, EventHandler<object> handleMessage)
        {
            ServiceStore.MessageService.Subscribe(messageName, this, handleMessage);
            hasSubscribeMessage = true;
        }

        /// <summary>
        /// 发布一条消息，所有订阅者都将收到通知。
        /// </summary>
        /// <typeparam name="TMessage">要发布的消息的类型。</typeparam>
        /// <param name="message">要发布的消息实例。</param>
        protected void PublishMessage<TMessage>(TMessage message)
        {
            ServiceStore.MessageService.Publish(this, message);
        }

        /// <summary>
        /// 取消订阅指定类型的消息。
        /// </summary>
        /// <typeparam name="TMessage">要取消订阅的消息的类型。</typeparam>
        /// <param name="handleMessage">之前用于订阅的委托。</param>
        protected void UnsubscribeMessage<TMessage>(EventHandler<TMessage> handleMessage)
        {
            ServiceStore.MessageService.Unsubscribe(this, handleMessage);
        }

        #endregion Message
    }

    /// <summary>
    /// 一个泛型视图模型基类，强类型关联一个视图接口。
    /// </summary>
    /// <typeparam name="TView">与此视图模型关联的视图的类型，必须实现 <see cref="IView"/>。</typeparam>
    public abstract class ExtenderAppViewModel<TView> : ExtenderAppViewModel, IViewModel<TView> where TView : class, IView
    {
        /// <summary>
        /// 获取或设置与此视图模型关联的强类型视图实例。
        /// </summary>
        protected TView View { get; set; }

        /// <summary>
        /// 初始化 <see cref="ExtenderAppViewModel{TView}"/> 的新实例。
        /// </summary>
        /// <param name="serviceStore">服务存储实例。</param>
        public ExtenderAppViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            View = default!;
        }

        /// <summary>
        /// 初始化 <see cref="ExtenderAppViewModel{TView}"/> 的新实例，并注入日志记录器。
        /// </summary>
        /// <param name="serviceStore">服务存储实例。</param>
        /// <param name="logger">日志记录器实例。</param>
        public ExtenderAppViewModel(IServiceStore serviceStore, ILogger logger) : base(serviceStore, logger)
        {
            View = default!;
        }

        /// <summary>
        /// 密封基类的 <c>InjectView</c> 方法，确保类型安全的注入。
        /// </summary>
        /// <param name="view">要注入的视图实例。</param>
        public override sealed void InjectView(IView view)
        {
            if (view is TView tview)
                InjectView(tview);
        }

        /// <summary>
        /// 注入强类型的视图实例，并调用受保护的虚方法以供派生类扩展。
        /// </summary>
        /// <param name="view">要注入的强类型视图实例。</param>
        public void InjectView(TView view)
        {
            View = view;

            ProtectedInjectView(view);
        }

        /// <summary>
        /// 为派生类提供一个可重写的入口点，用于在视图注入后执行附加逻辑。
        /// </summary>
        /// <param name="view">被注入的强类型视图实例。</param>
        protected virtual void ProtectedInjectView(TView view)
        {
        }

        /// <summary>
        /// 从当前视图导航到指定类型和作用域的视图。
        /// </summary>
        /// <param name="targetView">目标视图的类型。</param>
        /// <param name="scope">目标作用域。</param>
        /// <returns>导航成功则返回目标视图实例，否则返回 null。</returns>
        protected override IView NavigateTo(Type targetView, string scope)
        {
            IView view = null;
            try
            {
                view = ServiceStore.NavigationService.NavigateTo(targetView, scope, View);
            }
            catch (Exception ex)
            {
                LogError(ex, "视图导航出现了问题！");
            }
            return view;
        }

        /// <summary>
        /// 导航到一个新的窗口，并将当前视图作为其所有者。
        /// </summary>
        /// <typeparam name="T">要在新窗口中显示的视图类型。</typeparam>
        /// <returns>新创建的窗口实例，如果导航失败则返回 null。</returns>
        protected override IWindow? NavigateToWindow<T>()
        {
            IWindow? window = null;
            try
            {
                string scope = Details is null ? string.Empty : Details.PluginScopeName;
                window = ServiceStore.NavigationService.NavigateToWindow<T>(scope, View);
            }
            catch (Exception ex)
            {
                LogError(ex, "视图导航出现了问题！");
            }
            return window;
        }
    }

    /// <summary>
    /// 一个泛型视图模型基类，强类型关联一个视图接口和一个模型类。
    /// </summary>
    /// <typeparam name="TView">视图的类型。</typeparam>
    /// <typeparam name="TModle">模型的类型。</typeparam>
    public abstract class ExtenderAppViewModel<TView, TModle> : ExtenderAppViewModel<TView>
        where TView : class, IView
        where TModle : ExtenderAppModel, new()
    {
        /// <summary>
        /// 内部持有的模型实例。
        /// </summary>
        private TModle? model;

        /// <summary>
        /// 获取与此视图模型关联的模型实例。 如果模型尚未加载，它将尝试从本地数据加载，如果失败则从服务容器创建一个新实例。
        /// </summary>
        public TModle Model
        {
            get
            {
                if (model is null)
                {
                    if (!LoadModel() || model is null)
                        model = ServiceStore.ServiceProvider.GetRequiredService<TModle>();
                }
                return model;
            }
        }

        /// <summary>
        /// 初始化 <see cref="ExtenderAppViewModel{TView, TModle}"/> 的新实例。
        /// </summary>
        /// <param name="serviceStore">服务存储实例。</param>
        protected ExtenderAppViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }

        /// <summary>
        /// 初始化 <see cref="ExtenderAppViewModel{TView, TModle}"/> 的新实例，并注入日志记录器。
        /// </summary>
        /// <param name="serviceStore">服务存储实例。</param>
        /// <param name="logger">日志记录器实例。</param>
        protected ExtenderAppViewModel(IServiceStore serviceStore, ILogger logger) : base(serviceStore, logger)
        {
        }

        /// <summary>
        /// 初始化 <see cref="ExtenderAppViewModel{TView, TModle}"/> 的新实例，并使用提供的模型。
        /// </summary>
        /// <param name="model">要使用的模型实例。</param>
        /// <param name="serviceStore">服务存储实例。</param>
        protected ExtenderAppViewModel(TModle model, IServiceStore serviceStore) : base(serviceStore)
        {
            this.model = model;
        }

        /// <summary>
        /// 初始化 <see cref="ExtenderAppViewModel{TView, TModle}"/> 的新实例，并使用提供的模型和日志记录器。
        /// </summary>
        /// <param name="model">要使用的模型实例。</param>
        /// <param name="serviceStore">服务存储实例。</param>
        /// <param name="logger">日志记录器实例。</param>
        protected ExtenderAppViewModel(TModle model, IServiceStore serviceStore, ILogger logger) : base(serviceStore, logger)
        {
            this.model = model;
        }

        /// <summary>
        /// 重写以在视图进入时执行特定于模型的逻辑。
        /// </summary>
        /// <param name="newViewInfo">上一个视图的信息。</param>
        protected override void EnterProtected(ViewInfo newViewInfo)
        {
        }

        /// <summary>
        /// 重写以在视图退出时保存模型并根据配置清理资源。
        /// </summary>
        /// <param name="newViewInfo">将要导航到的新视图的信息。</param>
        protected override void ExitProtected(ViewInfo newViewInfo)
        {
            SaveModel();
            if (Details != null && !Details.IsStandingModel)
            {
                DeleteLocalData();
                if (model is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        /// <summary>
        /// 向视图模型注入一个新的模型实例，并保存它。
        /// </summary>
        /// <param name="model">要注入的模型实例。</param>
        public virtual void InjectModle(TModle model)
        {
            this.model = model;
            SaveModel();
        }

        /// <summary>
        /// 从本地存储加载模型数据。
        /// </summary>
        /// <returns>如果加载成功，则返回 true；否则返回 false。</returns>
        protected bool LoadModel()
        {
            var loadState = LoadLocalData(out model);
            model?.Initialize(ServiceStore as IPuginServiceStore);
            return loadState;
        }

        /// <summary>
        /// 将当前模型数据保存到本地存储。
        /// </summary>
        /// <returns>如果保存成功，则返回 true；否则返回 false。</returns>
        protected bool SaveModel()
        {
            return SaveLocalData(model);
        }

        /// <summary>
        /// 将当前模型数据以指定的名称保存到本地存储。
        /// </summary>
        /// <param name="saveName">用于保存数据的文件或键名。</param>
        /// <returns>如果保存成功，则返回 true；否则返回 false。</returns>
        protected bool SaveModel(string saveName)
        {
            // 调用本地数据服务保存数据，版本参数传null表示使用默认版本
            return ServiceStore.LocalDataService.SaveData(saveName, model, null);
        }

        /// <summary>
        /// 将当前模型数据以指定的版本保存到本地存储。
        /// </summary>
        /// <param name="version">要保存的数据的版本。</param>
        /// <returns>如果保存成功，则返回 true；否则返回 false。</returns>
        protected bool SaveModel(Version version)
        {
            // 调用内部方法保存数据，使用默认名称和指定版本
            return SaveLocalData(model, version);
        }

        /// <summary>
        /// 将当前模型数据以指定的名称和版本保存到本地存储。
        /// </summary>
        /// <param name="saveName">用于保存数据的文件或键名。</param>
        /// <param name="version">要保存的数据的版本。</param>
        /// <returns>如果保存成功，则返回 true；否则返回 false。</returns>
        protected bool SaveModel(string saveName, Version version)
        {
            // 调用本地数据服务保存数据，同时指定名称和版本
            return ServiceStore.LocalDataService.SaveData(saveName, model, version);
        }
    }
}