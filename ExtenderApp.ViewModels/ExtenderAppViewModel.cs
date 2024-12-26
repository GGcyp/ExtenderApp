using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Service;

namespace ExtenderApp.ViewModels
{
    public abstract class ExtenderAppViewModel : IViewModel
    {
        /// <summary>
        /// 服务存储接口实例
        /// </summary>
        protected readonly IServiceStore _serviceStore;

        private readonly string _viewModelName;

        /// <summary>
        /// 视图接口实例
        /// </summary>
        protected readonly IView _view;

        /// <summary>
        /// 获取指定类型的视图实例
        /// </summary>
        /// <typeparam name="TView">视图类型，需要继承自IView接口</typeparam>
        /// <returns>返回指定类型的视图实例</returns>
        /// <exception cref="ArgumentNullException">如果_view为null，则抛出此异常</exception>
        /// <exception cref="InvalidCastException">如果_view无法转换为指定类型，则抛出此异常</exception>
        protected TView GetView<TView>() where TView : class, IView
        {
            ArgumentNullException.ThrowIfNull(_view, string.Format("视图为空 : {0}", GetType().Name));

            var view = _view as TView;
            if (view is null)
                throw new InvalidCastException(string.Format("类型无效，{0}无法转换为{1}", _view.GetType().Name, typeof(TView)));

            return view;
        }

        protected ExtenderAppViewModel(IServiceStore serviceStore)
        {
            _serviceStore = serviceStore;
            _viewModelName = GetType().Name;
        }

        #region Navigate

        /// <summary>
        /// 导航到指定的视图。
        /// </summary>
        /// <typeparam name="TView">目标视图的类型，必须实现 IView 接口。</typeparam>
        public TView NavigateTo<TView>() where TView : class, IView
        {
            return NavigateTo(typeof(TView)) as TView;
        }

        /// <summary>
        /// 导航到指定的视图类型。
        /// </summary>
        /// <param name="targetView">目标视图的类型。</param>
        public IView NavigateTo(Type targetView)
        {
            IView view = null;
            try
            {
                view = _serviceStore.NavigationService.NavigateTo(targetView, _view);
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
    }
}
