using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Services;
using ExtenderApp.Data;
using ExtenderApp.Common.Error;
using ExtenderApp.Common;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Models;

namespace ExtenderApp.ViewModels
{
    public abstract class ExtenderAppViewModel : IViewModel, INotifyPropertyChanged
    {
        /// <summary>
        /// 服务存储接口实例
        /// </summary>
        protected readonly IServiceStore _serviceStore;

        /// <summary>
        /// 视图模型名称（只读）
        /// </summary>
        private readonly string _viewModelName;

        protected PluginDetails? ModDetails { get; set; }

        /// <summary>
        /// 返回当前的主窗口实例
        /// </summary>
        protected IMainWindow? CurrrentMainWindow => _serviceStore.MainWindowService.CurrentMainWindow;

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
            _serviceStore = serviceStore;
            _viewModelName = GetType().Name;
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

        public virtual void Close()
        {

        }

        #region Navigate

        /// <summary>
        /// 导航到指定的视图，没有指定作用域。
        /// </summary>
        /// <typeparam name="TView">目标视图的类型。</typeparam>
        /// <returns>返回目标视图的实例。</returns>
        protected TView NavigateTo<TView>() where TView : class, IView
        {
            var details = GetCurrentModDetails();
            string scope = details is null ? string.Empty : details.ModScope;
            return (TView)NavigateTo(typeof(TView), scope);
        }

        /// <summary>
        /// 导航到指定的视图，并指定作用域。
        /// </summary>
        /// <typeparam name="TView">目标视图的类型。</typeparam>
        /// <param name="scope">作用域。</param>
        /// <returns>返回目标视图的实例。</returns>
        protected TView NavigateTo<TView>(string scope) where TView : class, IView
        {
            return (TView)NavigateTo(typeof(TView), scope);
        }

        protected TView NavigateTo<TView>(PluginDetails modDetails) where TView : class, IView
        {
            return NavigateTo<TView>(modDetails.ModScope);
        }

        protected IView NavigateTo(PluginDetails modDetails)
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

            return NavigateTo(modDetails.StartupType, modDetails.ModScope);
        }

        /// <summary>
        /// 导航到指定的视图类型，并指定作用域。
        /// </summary>
        /// <param name="targetView">目标视图的类型。</param>
        /// <param name="scope">作用域。</param>
        /// <returns>返回目标视图的实例。</returns>
        protected virtual IView NavigateTo(Type targetView, string scope)
        {
            IView view = null;
            try
            {
                view = _serviceStore.NavigationService.NavigateTo(targetView, scope);
            }
            catch (Exception ex)
            {
                Error("视图导航出现了问题！", ex);
            }
            return view;
        }

        protected virtual IWindow NavigateToWindow<TView>()
            where TView : class, IView
        {
            IWindow window = null;
            try
            {
                var details = GetCurrentModDetails();
                string scope = details is null ? string.Empty : details.ModScope;
                window = _serviceStore.NavigationService.NavigateToWindow<TView>(scope, null);
            }
            catch (Exception ex)
            {
                Error("视图导航出现了问题！", ex);
            }
            return window;
        }

        #endregion

        #region Log

        /// <summary>
        /// 输出信息日志。
        /// </summary>
        /// <param name="message">要输出的信息内容。</param>
        protected void Info(object message)
        {
            Info(message?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// 输出调试日志。
        /// </summary>
        /// <param name="message">要输出的调试内容。</param>
        protected void Debug(object message)
        {
            Debug(message?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// 输出错误日志。
        /// </summary>
        /// <param name="message">要输出的错误信息内容。</param>
        /// <param name="exception">异常对象。</param>
        protected void Error(object message, Exception exception)
        {
            Error(message?.ToString() ?? string.Empty, exception);
        }

        /// <summary>
        /// 输出警告日志。
        /// </summary>
        /// <param name="message">要输出的警告内容。</param>
        protected void Warning(object message)
        {
            Warning(message?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// 记录信息级别的日志。
        /// </summary>
        /// <param name="message">要记录的信息内容。</param>
        protected void Info(string message)
        {
            _serviceStore.LogingService.Info(message, _viewModelName);
        }

        /// <summary>
        /// 记录调试级别的日志。
        /// </summary>
        /// <param name="message">要记录的调试信息内容。</param>
        protected void Debug(string message)
        {
            _serviceStore.LogingService.Debug(message, _viewModelName);
        }

        /// <summary>
        /// 记录错误级别的日志。
        /// </summary>
        /// <param name="message">要记录的错误信息内容。</param>
        /// <param name="exception">引发的异常。</param>
        protected void Error(string message, Exception exception)
        {
            _serviceStore.LogingService.Error(message, _viewModelName, exception);
        }

        /// <summary>
        /// 记录警告级别的日志。
        /// </summary>
        /// <param name="message">要记录的警告信息内容。</param>
        protected void Warning(string message)
        {
            _serviceStore.LogingService.Warning(message, _viewModelName);
        }

        #endregion

        #region LocalData

        /// <summary>
        /// 获取指定类型的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">输出参数，用于存储获取的数据</param>
        /// <returns>如果成功获取到数据则返回true，否则返回false</returns>
        protected bool LoadLocalData<T>(out T? data, Action<LocalData<T>> checkAction = null) where T : class
        {
            data = default;
            if (!_serviceStore.LocalDataService.LoadData(GetCurrentModDetails(), out LocalData<T> localData))
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
        protected bool SaveLocalData<T>(T? data) where T : class
        {
            if (data is null)
                data = _serviceStore.ServiceProvider.GetService<T>();

            if (data is null)
                return false;

            return _serviceStore.LocalDataService.SaveData(GetCurrentModDetails(), data);
        }

        /// <summary>
        /// 保存本地数据到服务，支持自动解析服务实例（如果数据为null）
        /// </summary>
        /// <typeparam name="T">要保存的数据类型，必须是引用类型</typeparam>
        /// <param name="data">要保存的数据实例，如果为null则会尝试从服务容器解析</param>
        /// <param name="version">要保存的数据版本信息</param>
        /// <returns>
        /// 返回保存操作是否成功：
        /// </returns>
        protected bool SaveLocalData<T>(T? data, Version version)
            where T : class
        {
            if (data is null)
                data = _serviceStore.ServiceProvider.GetService<T>();

            if (data is null)
                return false;

            var details = GetCurrentModDetails();
            if (details is null)
                return false;

            return _serviceStore.LocalDataService.SaveData(details.Title, data, version);
        }

        protected PluginDetails? GetCurrentModDetails()
        {
            if (_serviceStore is IPuginServiceStore store)
                return store.PuginDetails;
            return null;
        }

        #endregion

        #region ScheduledTask

        /// <summary>
        /// 启动一个定时任务，该任务在指定的延迟时间后开始执行，并以指定的周期重复执行。
        /// </summary>
        /// <param name="callback">任务执行时的回调函数。</param>
        /// <param name="state">传递给回调函数的状态对象。</param>
        /// <param name="dueTime">任务开始执行前的延迟时间。</param>
        /// <param name="period">任务执行的周期。</param>
        /// <returns>返回用于控制任务的 ExtenderCancellationToken 对象。</returns>
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
        /// <returns>返回用于控制任务的 ExtenderCancellationToken 对象。</returns>
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
        /// <returns>返回用于控制任务的 ExtenderCancellationToken 对象。</returns>
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
        /// <returns>返回用于控制任务的 ExtenderCancellationToken 对象。</returns>
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
        /// <returns>返回用于控制任务的 ExtenderCancellationToken 对象。</returns>
        protected ScheduledTask StartDelay(Action<object> callback, object state, TimeSpan dueTime)
        {
            ScheduledTask task = new ScheduledTask();
            task.StartDelay(callback, state, dueTime);
            return task;
        }

        #endregion

        #region Dispatcher

        protected void DispatcherInvoke(Action callback)
        {
            if (callback is null) return;

            _serviceStore.DispatcherService.Invoke(callback);
        }

        #endregion

        #region MainWindow

        /// <summary>
        /// 创建主窗口实例。
        /// </summary>
        /// <returns>主窗口实例</returns>
        protected IMainWindow CreateMainWindow()
        {
            return _serviceStore.MainWindowService.CreateMainWindow();
        }

        /// <summary>
        /// 临时将主窗口置顶显示（闪烁效果）
        /// </summary>
        /// <remarks>
        /// 该方法通过先设置窗口Topmost=true，短暂延迟后再设置Topmost=false，
        /// 实现窗口短暂置顶的效果，常用于吸引用户注意或窗口激活提示。
        /// 注意：该方法会在后台线程启动任务，但通过DispatcherInvoke确保UI操作在UI线程执行。
        /// </remarks>
        protected void MainWindowTopmost()
        {
            Task.Run(async () =>
            {
                if (CurrrentMainWindow == null) return;
                DispatcherInvoke(() =>
                {
                    CurrrentMainWindow.Topmost = true;
                });
                await Task.Delay(300);
                DispatcherInvoke(() =>
                {
                    CurrrentMainWindow.Topmost = false;
                });
            });
        }

        #endregion

        #region Path

        /// <summary>
        /// 在文件资源管理器中打开指定路径的文件夹
        /// </summary>
        /// <param name="folderPath">要打开的文件夹路径</param>
        protected void OpenFolder(string? path)
        {
            try
            {
                _serviceStore.PathService.OpenFolderInExplorer(path);
            }
            catch (Exception ex)
            {
                Error($"打开路径失败：{path}", ex);
            }
        }

        #endregion

        #region  Plugin

        protected void LoadPlugin(PluginDetails plugin)
        {
            _serviceStore.PluginService.LoadPlugin(plugin);
        }

        protected async Task LoadPluginAsync(PluginDetails plugin)
        {
            await _serviceStore.PluginService.LoadPluginAsync(plugin);
        }

        #endregion
    }

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
            TView targetView = view as TView;
            if (targetView != null)
                InjectView(targetView);
        }

        /// <summary>
        /// 注入视图
        /// </summary>
        /// <param name="view">要注入的视图</param>
        /// <exception cref="ArgumentNullException">如果传入的视图为空，则抛出此异常</exception>
        /// <exception cref="InvalidDataException">如果当前视图不可更改且已存在视图，则抛出此异常</exception>
        public virtual void InjectView(TView view)
        {
            if (view is null)
                throw new ArgumentNullException(nameof(view));

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
                view = _serviceStore.NavigationService.NavigateTo(targetView, scope, view);
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
                var details = GetCurrentModDetails();
                string scope = details is null ? string.Empty : details.ModScope;
                window = _serviceStore.NavigationService.NavigateToWindow<T>(scope, View);
            }
            catch (Exception ex)
            {
                Error("视图导航出现了问题！", ex);
            }
            return window;
        }
    }

    /// <summary>
    /// 泛型扩展应用程序视图模型基类，继承自<see cref="ExtenderAppViewModel{TView}"/>
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
                        model = _serviceStore.ServiceProvider.GetRequiredService<TModle>();
                }
                return model;
            }
        }

        /// <summary>
        /// 初始化<see cref="ExtenderAppViewModel{TView, TModle}"/>的新实例
        /// </summary>
        /// <param name="serviceStore">服务存储</param>
        protected ExtenderAppViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }

        protected ExtenderAppViewModel(TModle model, IServiceStore serviceStore) : base(serviceStore)
        {
            this.model = model;
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
            return LoadLocalData(out model);
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
        /// 将模型数据保存到本地
        /// </summary>
        /// <param name="version">数据保存目标版本</param>
        /// <returns>是否成功保存数据</returns>
        protected bool SaveModel(Version version)
        {
            return SaveLocalData(model, version);
        }

        public override void Close()
        {
            SaveModel();
        }
    }
}
