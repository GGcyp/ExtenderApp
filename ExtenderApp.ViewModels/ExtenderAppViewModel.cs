using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Services;
using ExtenderApp.Data;
using ExtenderApp.Common.Error;
using ExtenderApp.Common;

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

        /// <summary>
        /// 关闭资源或连接。
        /// </summary>
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
            modDetails.ArgumentNull(nameof(modDetails));
            modDetails.StartupType.ArgumentObjectNull(nameof(modDetails));
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

        #region Mod

        /// <summary>
        /// 获取当前模块的详细信息。
        /// </summary>
        /// <returns>当前模块的详细信息，如果无法获取则返回null。</returns>
        protected PluginDetails? GetCurrentModDetails()
        {
            if (_serviceStore is IModServiceStore service)
                return service.ModDetails;
            return null;
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
        protected bool SaveLocalData<T>(T? data) where T : class, new()
        {
            if (data is null)
                data = new T();
            return _serviceStore.LocalDataService.SaveData(GetCurrentModDetails(), data);
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
    }

    public abstract class ExtenderAppViewModel<TView> : ExtenderAppViewModel, IViewModel<TView> where TView : IView
    {
        /// <summary>
        /// 视图接口实例
        /// </summary>
        protected TView View { get; set; }

        public ExtenderAppViewModel(IServiceStore serviceStore) : base(serviceStore)
        {

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
    }

    /// <summary>
    /// 泛型扩展应用程序视图模型基类，继承自<see cref="ExtenderAppViewModel{TView}"/>
    /// </summary>
    /// <typeparam name="TView">视图接口</typeparam>
    /// <typeparam name="TModle">模型类型</typeparam>
    public abstract class ExtenderAppViewModel<TView, TModle> : ExtenderAppViewModel<TView> where TView : IView where TModle : class, new()
    {
        /// <summary>
        /// 模型实例
        /// </summary>
        private TModle model;

        /// <summary>
        /// 获取模型实例
        /// </summary>
        protected TModle Model
        {
            get
            {
                if (model is null)
                {
                    if (!LoadModel() || model is null)
                        model = new TModle();
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

        public virtual void InjectViewModel(TModle model)
        {
            this.model = model;
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

        public override void Close()
        {
            SaveModel();
        }
    }
}
