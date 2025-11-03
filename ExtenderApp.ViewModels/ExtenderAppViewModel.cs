using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Services;
using ExtenderApp.Data;
using ExtenderApp.Common.Error;
using ExtenderApp.Common;

using ExtenderApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.ViewModels
{
    /// <summary>
    /// 扩展应用程序视图模型基类，继承自 <see cref="IViewModel"/>
    /// </summary>
    public abstract class ExtenderAppViewModel : DisposableObject, IViewModel, INotifyPropertyChanged
    {
        /// <summary>
        /// 服务存储接口实例
        /// </summary>
        protected readonly IServiceStore ServiceStore;

        /// <summary>
        /// 视图模型名称（只读）
        /// </summary>
        private readonly string _viewModelName;

        /// <summary>
        /// 当前视图模型的插件详细信息
        /// </summary>
        protected PluginDetails? Details { get; set; }

        /// <summary>
        /// 返回当前的主窗口实例
        /// </summary>
        protected IMainWindow? CurrrentMainWindow => ServiceStore.MainWindowService.CurrentMainWindow;

        /// <summary>
        /// 当属性更改时发生的事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 当属性更改时触发PropertyChanged事件
        /// </summary>
        /// <param name="propertyName">发生更改的属性名称</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// ExtenderAppViewModel构造函数
        /// </summary>
        /// <param name="serviceStore">服务存储对象</param>
        public ExtenderAppViewModel(IServiceStore serviceStore)
        {
            ServiceStore = serviceStore;
            _viewModelName = GetType().Name;
            Details = GetCurrentPluginDetails();
        }

        public virtual void InjectView(IView view)
        {
        }

        public virtual void Enter(ViewInfo oldViewInfo)
        {
        }

        public virtual void Exit(ViewInfo newViewInfo)
        {
        }

        private PluginDetails? GetCurrentPluginDetails()
        {
            if (ServiceStore is IPuginServiceStore store)
                return store.PuginDetails;
            return null;
        }

        public virtual void OnViewloaded()
        {
        }

        public virtual void OnViewUnloaded()
        {
        }

        #region Navigate

        /// <summary>
        /// 导航到指定的视图，没有指定作用域。
        /// </summary>
        /// <typeparam name="TView">目标视图的类型。</typeparam>
        /// <returns>返回目标视图的实例。</returns>
        protected TView? NavigateTo<TView>()
            where TView : class, IView
        {
            string scope = Details is null ? string.Empty : Details.PluginScope;
            return NavigateTo(typeof(TView), scope) as TView;
        }

        /// <summary>
        /// 导航到指定的视图（可指定旧视图），返回目标视图实例。
        /// </summary>
        /// <typeparam name="TView">目标视图类型，必须实现IView接口。</typeparam>
        /// <param name="oldView">旧的视图实例。</param>
        /// <returns>返回目标视图实例，如果导航失败则返回null。</returns>
        protected TView? NavigateTo<TView>(IView oldView)
            where TView : class, IView
        {
            string scope = Details is null ? string.Empty : Details.PluginScope;
            return NavigateTo(typeof(TView), scope, oldView) as TView;
        }

        /// <summary>
        /// 导航到指定作用域下的目标视图类型。
        /// </summary>
        /// <typeparam name="TView">目标视图类型，必须实现IView接口。</typeparam>
        /// <param name="scope">导航作用域标识。</param>
        /// <returns>返回目标视图实例，如果导航失败则返回null。</returns>
        protected TView? NavigateTo<TView>(string scope) where TView : class, IView
        {
            return NavigateTo(typeof(TView), scope) as TView;
        }

        /// <summary>
        /// 根据插件详情导航到目标视图类型。
        /// </summary>
        /// <typeparam name="TView">目标视图类型，必须实现IView接口。</typeparam>
        /// <param name="modDetails">插件详情对象。</param>
        /// <returns>返回目标视图实例，如果导航失败则返回null。</returns>
        protected TView? NavigateTo<TView>(PluginDetails modDetails) where TView : class, IView
        {
            return NavigateTo<TView>(modDetails.PluginScope);
        }

        /// <summary>
        /// 根据插件详情导航到目标视图类型（支持异常处理）。
        /// </summary>
        /// <param name="modDetails">插件详情对象。</param>
        /// <returns>返回目标视图实例，如果导航失败则返回null。</returns>
        protected IView? NavigateTo(PluginDetails modDetails)
        {
            try
            {
                modDetails.ArgumentNull(nameof(modDetails));
                modDetails.StartupType.ArgumentObjectNull(nameof(modDetails));
            }
            catch (ArgumentNullException ex)
            {
                Error("导航参数不能为空！", ex);
                return null;
            }

            return NavigateTo(modDetails.StartupType, modDetails.PluginScope);
        }

        /// <summary>
        /// 导航到指定类型的视图，并指定作用域。
        /// </summary>
        /// <param name="targetView">目标视图类型。</param>
        /// <param name="scope">导航作用域标识。</param>
        /// <returns>返回目标视图实例，如果导航失败则返回null。</returns>
        protected virtual IView? NavigateTo(Type targetView, string scope)
        {
            IView? view = null;
            try
            {
                view = ServiceStore.NavigationService.NavigateTo(targetView, scope);
            }
            catch (Exception ex)
            {
                Error("视图导航出现了问题！", ex);
            }
            return view;
        }

        /// <summary>
        /// 导航到指定类型的视图，并指定作用域和旧视图实例。
        /// </summary>
        /// <param name="targetView">目标视图类型。</param>
        /// <param name="scope">导航作用域标识。</param>
        /// <param name="oldView">旧的视图实例。</param>
        /// <returns>返回目标视图实例，如果导航失败则返回null。</returns>
        protected virtual IView? NavigateTo(Type targetView, string scope, IView oldView)
        {
            IView? view = null;
            try
            {
                view = ServiceStore.NavigationService.NavigateTo(targetView, scope, oldView);
            }
            catch (Exception ex)
            {
                Error("视图导航出现了问题！", ex);
            }
            return view;
        }

        /// <summary>
        /// 导航到指定类型的窗口，并指定作用域。
        /// </summary>
        /// <typeparam name="TView">目标视图类型，必须实现IView接口。</typeparam>
        /// <returns>返回窗口实例，如果导航失败则返回null。</returns>
        protected virtual IWindow? NavigateToWindow<TView>()
            where TView : class, IView
        {
            IWindow window = null;
            try
            {
                string scope = Details is null ? string.Empty : Details.PluginScope;
                window = ServiceStore.NavigationService.NavigateToWindow<TView>(scope, null);
            }
            catch (Exception ex)
            {
                Error("视图导航出现了问题！", ex);
            }
            return window;
        }

        #endregion Navigate

        #region Log

        /// <summary>
        /// 输出信息日志。
        /// </summary>
        /// <param name="message">要输出的信息内容。</param>
        public void Info(object message)
        {
            Info(message?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// 输出调试日志。
        /// </summary>
        /// <param name="message">要输出的调试内容。</param>
        public void Debug(object message)
        {
            Debug(message?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// 输出错误日志。
        /// </summary>
        /// <param name="message">要输出的错误信息内容。</param>
        /// <param name="exception">异常对象。</param>
        public void Error(object message, Exception exception)
        {
            Error(message?.ToString() ?? string.Empty, exception);
        }

        /// <summary>
        /// 输出警告日志。
        /// </summary>
        /// <param name="message">要输出的警告内容。</param>
        public void Warning(object message)
        {
            Warning(message?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// 记录信息级别的日志。
        /// </summary>
        /// <param name="message">要记录的信息内容。</param>
        public void Info(string message)
        {
            ServiceStore.LogingService.Info(message, _viewModelName);
        }

        /// <summary>
        /// 记录调试级别的日志。
        /// </summary>
        /// <param name="message">要记录的调试信息内容。</param>
        public void Debug(string message)
        {
            ServiceStore.LogingService.Debug(message, _viewModelName);
        }

        /// <summary>
        /// 记录错误级别的日志。
        /// </summary>
        /// <param name="message">要记录的错误信息内容。</param>
        /// <param name="exception">引发的异常。</param>
        public void Error(string message, Exception exception)
        {
            ServiceStore.LogingService.Error(message, _viewModelName, exception);
        }

        /// <summary>
        /// 记录警告级别的日志。
        /// </summary>
        /// <param name="message">要记录的警告信息内容。</param>
        public void Warning(string message)
        {
            ServiceStore.LogingService.Warning(message, _viewModelName);
        }

        #endregion Log

        #region LocalData

        /// <summary>
        /// 获取指定类型的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">输出参数，用于存储获取的数据</param>
        /// <returns>如果成功获取到数据则返回true，否则返回false</returns>
        protected bool LoadLocalData<T>(out T? data, Action<LocalData<T>> checkAction = null)
            where T : class
        {
            data = default;
            if (!ServiceStore.LocalDataService.LoadData(Details, out LocalData<T> localData))
                return false;

            data = localData.Data;
            checkAction?.Invoke(localData);
            return true;
        }

        /// <summary>
        /// 设置指定类型的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">要设置的数据</param>
        /// <returns>如果成功设置数据则返回true，否则返回false</returns>
        protected bool SaveLocalData<T>(T? data)
            where T : class
        {
            if (data is null || Details is null)
                return false;

            return ServiceStore.LocalDataService.SaveData(Details, data);
        }

        /// <summary>
        /// 保存本地数据到服务，支持自动解析服务实例（如果数据为null）
        /// </summary>
        /// <typeparam name="T">要保存的数据类型，必须是引用类型</typeparam>
        /// <param name="data">要保存的数据实例，如果为null则会尝试从服务容器解析</param>
        /// <param name="version">要保存的数据版本信息</param>
        /// <returns>返回保存操作是否成功：</returns>
        protected bool SaveLocalData<T>(T? data, Version version)
            where T : class
        {
            if (data is null || Details is null)
                return false;

            return ServiceStore.LocalDataService.SaveData(Details.Title, data, version);
        }

        /// <summary>
        /// 删除当前插件的本地数据。
        /// 根据当前视图模型的插件详情（Details）调用本地数据服务进行数据删除。
        /// 如果插件详情为 null，则不执行删除操作并返回 false。
        /// </summary>
        /// <returns>删除成功返回 true，否则返回 false。</returns>
        protected bool DeleteLocalData()
        {
            if (Details is null)
                return false;
            return ServiceStore.LocalDataService.DeleteData(Details);
        }

        #endregion LocalData

        #region ScheduledTask

        /// <summary>
        /// 启动一个定时任务，该任务在指定的延迟时间后开始执行，并以指定的周期重复执行。
        /// </summary>
        /// <param name="callback">任务执行时的回调函数。</param>
        /// <param name="state">传递给回调函数的状态对象。</param>
        /// <param name="dueTime">任务开始执行前的延迟时间。</param>
        /// <param name="period">任务执行的周期。</param>
        /// <returns>
        /// 返回用于控制任务的 ExtenderCancellationToken 对象。
        /// </returns>
        protected ScheduledTask Start(Action<object> callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            ScheduledTask task = new ScheduledTask();
            task.Start(callback, state, dueTime, period);
            return task;
        }

        /// <summary>
        /// 启动一个循环任务，该任务以指定的周期重复执行。
        /// </summary>
        /// <param name="callback">任务执行时的回调函数。</param>
        /// <param name="period">任务执行的周期。</param>
        /// <returns>
        /// 返回用于控制任务的 ExtenderCancellationToken 对象。
        /// </returns>
        protected ScheduledTask StartCycle(Action<object> callback, TimeSpan period)
        {
            return StartCycle(callback, null, period);
        }

        /// <summary>
        /// 启动一个循环任务，该任务以指定的周期重复执行。
        /// </summary>
        /// <param name="callback">任务执行时的回调函数。</param>
        /// <param name="state">传递给回调函数的状态对象。</param>
        /// <param name="period">任务执行的周期。</param>
        /// <returns>
        /// 返回用于控制任务的 ExtenderCancellationToken 对象。
        /// </returns>
        protected ScheduledTask StartCycle(Action<object> callback, object state, TimeSpan period)
        {
            ScheduledTask task = new ScheduledTask();
            task.StartCycle(callback, state, period);
            return task;
        }

        /// <summary>
        /// 启动一个延迟任务，该任务在指定的延迟时间后开始执行。
        /// </summary>
        /// <param name="callback">任务执行时的回调函数。</param>
        /// <param name="dueTime">任务开始执行前的延迟时间。</param>
        /// <returns>
        /// 返回用于控制任务的 ExtenderCancellationToken 对象。
        /// </returns>
        protected ScheduledTask StartDelay(Action<object> callback, TimeSpan dueTime)
        {
            return StartDelay(callback, null, dueTime);
        }

        /// <summary>
        /// 启动一个延迟任务，该任务在指定的延迟时间后开始执行。
        /// </summary>
        /// <param name="callback">任务执行时的回调函数。</param>
        /// <param name="state">传递给回调函数的状态对象。</param>
        /// <param name="dueTime">任务开始执行前的延迟时间。</param>
        /// <returns>
        /// 返回用于控制任务的 ExtenderCancellationToken 对象。
        /// </returns>
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
        /// <remarks>
        /// 如果传入的操作为null，则方法直接返回而不执行任何操作。 使用DispatcherService的Invoke方法确保操作在UI线程上执行。
        /// </remarks>
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
        /// <remarks>
        /// 如果传入的操作为null，则方法直接返回而不执行任何操作。 使用DispatcherService的BeginInvoke方法异步确保操作在UI线程上执行。
        /// </remarks>
        protected void DispatcherBeginInvoke(Action callback)
        {
            if (callback is null) return;
            ServiceStore.DispatcherService.BeginInvoke(callback);
        }

        protected bool CheckAccess()
        {
            return ServiceStore.DispatcherService.CheckAccess();
        }

        protected ThreadSwitchAwaitable ToMainThreadAsync(CancellationToken token = default)
        {
            return ServiceStore.DispatcherService.ToMainThreadAsync(token);
        }

        protected ThreadSwitchAwaitable AwayMainThreadAsync(CancellationToken token = default)
        {
            return ServiceStore.DispatcherService.AwayMainThreadAsync(token);
        }

        #endregion Dispatcher

        #region MainWindow

        /// <summary>
        /// 创建主窗口实例。
        /// </summary>
        /// <returns>主窗口实例</returns>
        protected IMainWindow CreateMainWindow()
        {
            return ServiceStore.MainWindowService.CreateMainWindow();
        }

        /// <summary>
        /// 临时将主窗口置顶显示（闪烁效果）
        /// </summary>
        /// <remarks>
        /// 该方法通过先设置窗口Topmost=true，短暂延迟后再设置Topmost=false，
        /// 实现窗口短暂置顶的效果，常用于吸引用户注意或窗口激活提示。 注意：该方法会在后台线程启动任务，但通过DispatcherInvoke确保UI操作在UI线程执行。
        /// </remarks>
        protected void MainWindowTopmost()
        {
            Task.Run(async () =>
            {
                if (CurrrentMainWindow == null) return;

                await ToMainThreadAsync();

                CurrrentMainWindow.Topmost = true;
                await Task.Delay(300).ConfigureAwait(true);
                CurrrentMainWindow.Topmost = false;
            });
        }

        #endregion MainWindow

        #region Path

        /// <summary>
        /// 在文件资源管理器中打开指定路径的文件夹
        /// </summary>
        /// <param name="folderPath">要打开的文件夹路径</param>
        protected void OpenFolder(string? path)
        {
            try
            {
                ServiceStore.PathService.OpenFolder(path);
            }
            catch (Exception ex)
            {
                Error($"打开路径失败：{path}", ex);
            }
        }

        /// <summary>
        /// 打开文件选择对话框，允许用户选择指定类型的文件。
        /// </summary>
        /// <param name="filter">
        /// 文件筛选器，例如 "文本文件 (*.txt)|*.txt"
        /// </param>
        /// <param name="targetPath">对话框初始打开的文件夹路径，默认为空（使用系统默认路径）</param>
        /// <returns>
        /// 用户选择的文件完整路径，若未选择则返回空字符串或 null
        /// </returns>
        protected string OpenFile(string filter, string? targetPath = null)
        {
            try
            {
                return ServiceStore.PathService.OpenFile(filter, targetPath);
            }
            catch (Exception ex)
            {
                Error($"打开文件失败：{filter} :{targetPath}", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 在文件根路径下创建指定的文件夹路径
        /// </summary>
        /// <param name="folderPath"></param>
        protected string CreateFolderPathForAppRootFolder(string folderPath)
        {
            try
            {
                return ServiceStore.PathService.CreateFolderPathForAppRootFolder(folderPath);
            }
            catch (Exception ex)
            {
                Error($"创建路径失败：{folderPath}", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 打开插件目录下的指定文件夹
        /// </summary>
        /// <param name="folderPath">指定文件目录</param>
        /// <returns>返回指定目录在插件文件夹下的地址</returns>
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

        protected async Task LoadPluginAsync(PluginDetails plugin)
        {
            await ServiceStore.PluginService.LoadPluginAsync(plugin);
        }

        protected void UnLoadPlugin(PluginDetails plugin)
        {
            ServiceStore.PluginService.UnloadPlugin(plugin);
        }

        #endregion Plugin

        #region System

        /// <summary>
        /// 向剪切板写入文本
        /// </summary>
        /// <param name="text">需要写入的文本.</param>
        protected void ClipboardSetText(string text)
        {
            ServiceStore.SystemService.Clipboard.SetText(text);
        }

        #region KeyCapture

        /// <summary>
        /// 启动键盘捕获服务。 调用后实现应安装底层钩子或开始监听系统键盘事件。
        /// </summary>
        /// <remarks>
        /// - 若服务已启动，应为幂等操作，不应抛出异常。
        /// - 回调可能在非 UI 线程触发，请在事件处理器内自行调度到 UI 线程。
        /// </remarks>
        protected void StartKeyCapture()
        {
            ServiceStore.SystemService.KeyCapture.Start();
        }

        /// <summary>
        /// 停止键盘捕获服务。 调用后实现应卸载底层钩子或停止监听，但通常保留已注册的回调以便后续再次启动。
        /// </summary>
        /// <remarks>若服务未启动，应为幂等操作，不产生副作用。</remarks>
        protected void StopKeyCapture()
        {
            ServiceStore.SystemService.KeyCapture.Stop();
        }

        /// <summary>
        /// 注册指定键与修饰键组合的“按下/抬起”事件处理器。
        /// </summary>
        /// <param name="key">要监听的主键。</param>
        /// <param name="downHandle">按下事件处理器。</param>
        /// <param name="upHandler">抬起事件处理器。</param>
        /// <param name="modifier">匹配所需的修饰键组合（默认无）。</param>
        /// <remarks>
        /// - 使用当前 ViewModel 作为订阅者标识，用于后续注销。
        /// - 同一键与修饰键组合可多次注册，处理器将累加至委托链。
        /// </remarks>
        protected void RegisterKeyCapture(Key key, EventHandler<KeyEvent> downHandle, EventHandler<KeyEvent> upHandler, ModifierKeys modifier = ModifierKeys.None)
        {
            ServiceStore.SystemService.KeyCapture.Register(key, this, downHandle, upHandler, modifier);
        }

        /// <summary>
        /// 仅为指定键注册“按下（KeyDown）”事件处理器。
        /// </summary>
        /// <param name="key">要监听的主键。</param>
        /// <param name="downHandle">按下事件处理器。</param>
        /// <param name="modifier">匹配所需的修饰键组合（默认无）。</param>
        protected void RegisterKeyCaptureDown(Key key, EventHandler<KeyEvent> downHandle, ModifierKeys modifier = ModifierKeys.None)
        {
            ServiceStore.SystemService.KeyCapture.Register(key, this, downHandle, null, modifier);
        }

        /// <summary>
        /// 仅为指定键注册“抬起（KeyUp）”事件处理器。
        /// </summary>
        /// <param name="key">要监听的主键。</param>
        /// <param name="upHandler">抬起事件处理器。</param>
        /// <param name="modifier">匹配所需的修饰键组合（默认无）。</param>
        protected void RegisterKeyCaptureUp(Key key, EventHandler<KeyEvent> upHandler, ModifierKeys modifier = ModifierKeys.None)
        {
            ServiceStore.SystemService.KeyCapture.Register(key, this, null, upHandler, modifier);
        }

        /// <summary>
        /// 注销当前 ViewModel 在某主键上的所有回调（无论修饰键）。
        /// </summary>
        /// <param name="key">目标主键。</param>
        protected void UnRegisterKeyCapture(Key key)
        {
            ServiceStore.SystemService.KeyCapture.UnRegister(key, this);
        }

        /// <summary>
        /// 注销当前 ViewModel 在所有主键上的所有回调。
        /// </summary>
        protected void UnRegisterKeyCapture()
        {
            ServiceStore.SystemService.KeyCapture.UnRegister(this);
        }

        #endregion KeyCapture

        #endregion System

        #region Message

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TMessage">订阅消息类型</typeparam>
        /// <param name="handleMessage">订阅消息委托</param>
        protected void SubscribeMessage<TMessage>(EventHandler<TMessage> handleMessage)
        {
            ServiceStore.MessageService.Subscribe(this, handleMessage);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="TMessage">发布消息类型</typeparam>
        /// <param name="message">发布的消息</param>
        protected void PublishMessage<TMessage>(TMessage message)
        {
            ServiceStore.MessageService.Publish(this, message);
        }

        /// <summary>
        /// 取消订阅消息
        /// </summary>
        /// <typeparam name="TMessage">被取消的消息类型</typeparam>
        /// <param name="handleMessage">取消的委托</param>
        protected void UnsubscribeMessage<TMessage>(EventHandler<TMessage> handleMessage)
        {
            ServiceStore.MessageService.Unsubscribe(this, handleMessage);
        }

        #endregion Message
    }

    /// <summary>
    /// 泛型扩展应用程序视图模型基类，继承自 <see cref="ExtenderAppViewModel"/>
    /// </summary>
    /// <typeparam name="TView">视图接口</typeparam>
    public abstract class ExtenderAppViewModel<TView> : ExtenderAppViewModel, IViewModel<TView> where TView : class, IView
    {
        /// <summary>
        /// 视图接口实例
        /// </summary>
        protected TView View { get; set; }

        public ExtenderAppViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }

        public override void InjectView(IView view)
        {
            if (view is TView tview)
                InjectView(tview);
        }

        /// <summary>
        /// 注入视图
        /// </summary>
        /// <param name="view">要注入的视图</param>
        public virtual void InjectView(TView view)
        {
            View = view;
        }

        /// <summary>
        /// 导航到指定的视图类型。
        /// </summary>
        /// <param name="targetView">目标视图的类型。</param>
        /// <param name="scope">目标作用域</param>
        protected override IView NavigateTo(Type targetView, string scope)
        {
            IView view = null;
            try
            {
                view = ServiceStore.NavigationService.NavigateTo(targetView, scope, View);
            }
            catch (Exception ex)
            {
                Error("视图导航出现了问题！", ex);
            }
            return view;
        }

        protected override IWindow? NavigateToWindow<T>()
        {
            IWindow? window = null;
            try
            {
                string scope = Details is null ? string.Empty : Details.PluginScope;
                window = ServiceStore.NavigationService.NavigateToWindow<T>(scope, View);
            }
            catch (Exception ex)
            {
                Error("视图导航出现了问题！", ex);
            }
            return window;
        }
    }

    /// <summary>
    /// 泛型扩展应用程序视图模型基类，继承自 <see cref="ExtenderAppViewModel{TView}"/>
    /// </summary>
    /// <typeparam name="TView">视图接口</typeparam>
    /// <typeparam name="TModle">模型类型</typeparam>
    public abstract class ExtenderAppViewModel<TView, TModle> : ExtenderAppViewModel<TView>
        where TView : class, IView
        where TModle : ExtenderAppModel
    {
        /// <summary>
        /// 模型实例
        /// </summary>
        private TModle? model;

        /// <summary>
        /// 获取模型实例
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
        /// 初始化 <see
        /// cref="ExtenderAppViewModel{TView,
        /// TModle}"/> 的新实例
        /// </summary>
        /// <param name="serviceStore">服务存储</param>
        protected ExtenderAppViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }

        protected ExtenderAppViewModel(TModle model, IServiceStore serviceStore) : base(serviceStore)
        {
            this.model = model;
        }

        public override void Enter(ViewInfo oldViewInfo)
        {
        }

        public override void Exit(ViewInfo newViewInfo)
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
        /// 向对象注入模型
        /// </summary>
        /// <param name="model">要注入的模型</param>
        public virtual void InjectModle(TModle model)
        {
            this.model = model;
            SaveModel();
        }

        /// <summary>
        /// 获取本地数据并赋值给模型
        /// </summary>
        /// <returns>是否成功获取数据</returns>
        protected bool LoadModel()
        {
            var loadState = LoadLocalData(out model);
            model.Initialize(ServiceStore as IPuginServiceStore);
            return loadState;
        }

        /// <summary>
        /// 将模型数据保存到本地
        /// </summary>
        /// <returns>是否成功保存数据</returns>
        protected bool SaveModel()
        {
            return SaveLocalData(model);
        }

        /// <summary>
        /// 将模型数据保存到本地，使用指定名称保存
        /// </summary>
        /// <param name="saveName">保存的数据名称</param>
        /// <returns>是否成功保存数据</returns>
        protected bool SaveModel(string saveName)
        {
            // 调用本地数据服务保存数据，版本参数传null表示使用默认版本
            return ServiceStore.LocalDataService.SaveData(saveName, model, null);
        }

        /// <summary>
        /// 将模型数据保存到本地，使用指定版本保存
        /// </summary>
        /// <param name="version">数据保存目标版本</param>
        /// <returns>是否成功保存数据</returns>
        protected bool SaveModel(Version version)
        {
            // 调用内部方法保存数据，使用默认名称和指定版本
            return SaveLocalData(model, version);
        }

        /// <summary>
        /// 将模型数据保存到本地，使用指定名称和版本保存
        /// </summary>
        /// <param name="saveName">保存的数据名称</param>
        /// <param name="version">数据保存目标版本</param>
        /// <returns>是否成功保存数据</returns>
        protected bool SaveModel(string saveName, Version version)
        {
            // 调用本地数据服务保存数据，同时指定名称和版本
            return ServiceStore.LocalDataService.SaveData(saveName, model, version);
        }
    }
}