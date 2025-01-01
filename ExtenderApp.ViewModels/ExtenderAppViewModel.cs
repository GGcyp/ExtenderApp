using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Services;
using ExtenderApp.Data;
using ExtenderApp.Common.Error;

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

        protected TView NavigateTo<TView>(ModDetails modDetails) where TView : class, IView
        {
            return NavigateTo<TView>(modDetails.ModScope);
        }

        protected IView NavigateTo(ModDetails modDetails)
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

        #region NetWork

        /// <summary>
        /// 异步获取字符串内容
        /// </summary>
        /// <param name="uri">目标资源的统一资源标识符（URI）</param>
        /// <returns>返回字符串内容，如果获取失败则返回null</returns>
        protected async Task<string> GetStringAsync(string uri)
        {
            string result = null;
            try
            {
                result = await _serviceStore.NetWorkService.GetStringAsync(uri);
            }
            catch (Exception ex)
            {
                Error($"{ex.Message}", ex);
            }
            return result;
        }

        /// <summary>
        /// 异步获取字符串内容
        /// </summary>
        /// <param name="uri">目标资源的统一资源标识符（URI）</param>
        /// <param name="callback">回调方法，用于处理获取到的字符串</param>
        protected void GetStringAsync(string uri, Action<string> callback)
        {
            try
            {
                _serviceStore.NetWorkService.GetStringAsync(uri, callback);
            }
            catch (Exception ex)
            {
                Error($"{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 异步获取流内容
        /// </summary>
        /// <param name="uri">目标资源的统一资源标识符（URI）</param>
        /// <returns>返回流对象，如果获取失败则返回null</returns>
        protected async Task<Stream> GetStreamAsync(Uri uri)
        {
            Stream result = null;
            try
            {
                result = await _serviceStore.NetWorkService.GetStreamAsync(uri);
            }
            catch (Exception ex)
            {
                Error($"{ex.Message}", ex);
            }
            return result;
        }

        /// <summary>
        /// 异步获取流内容
        /// </summary>
        /// <param name="uri">目标资源的统一资源标识符（URI）</param>
        /// <param name="callback">回调方法，用于处理获取到的流</param>
        protected void GetStreamAsync(Uri uri, Action<Stream> callback)
        {
            try
            {
                _serviceStore.NetWorkService.GetStreamAsync(uri, callback);
            }
            catch (Exception ex)
            {
                Error($"{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 通过TCP发送数据到指定IP和端口的异步方法。
        /// </summary>
        /// <param name="ip">目标IP地址。</param>
        /// <param name="port">目标端口号。</param>
        /// <returns>返回一个包含网络流的Task对象。</returns>
        /// <exception cref="ArgumentNullException">当传入的IP地址为null或空字符串时抛出此异常。</exception>
        protected async Task<NetworkStream> TcpSendAsync(string ip, int port)
        {
            if (string.IsNullOrEmpty(ip))
            {
                throw new ArgumentNullException(nameof(ip));
            }

            NetworkStream result = null;
            try
            {
                IPEndPoint iPEndPoint = IPEndPoint.Parse(ip);
                iPEndPoint.Port = port;
                result = await _serviceStore.NetWorkService.TcpSendAsync(iPEndPoint);
            }
            catch (Exception ex)
            {
                Error($"{ex.Message}", ex);
            }
            return result;
        }

        /// <summary>
        /// 异步发送TCP请求
        /// </summary>
        /// <param name="iP">目标IP地址和端口</param>
        /// <returns>返回NetworkStream对象，如果发送失败则返回null</returns>
        protected async Task<NetworkStream> TcpSendAsync(IPEndPoint ip)
        {
            NetworkStream result = null;
            try
            {
                result = await _serviceStore.NetWorkService.TcpSendAsync(ip);
            }
            catch (Exception ex)
            {
                Error($"{ex.Message}", ex);
            }
            return result;
        }

        #endregion

        #region Log

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
        protected ModDetails? GetCurrentModDetails()
        {
            if (_serviceStore is IModServiceStore service)
                return service.ModDetails;
            return null;
        }

        #endregion
    }

    public abstract class ExtenderAppViewModel<TView> : ExtenderAppViewModel, IViewModel<TView> where TView : IView
    {
        /// <summary>
        /// 视图接口实例
        /// </summary>
        protected TView? View { get; set; }

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
}
