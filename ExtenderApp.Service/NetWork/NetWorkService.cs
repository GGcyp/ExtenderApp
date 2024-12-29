using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Services.NetWork
{
    /// <summary>
    /// 网络服务类，实现了 INetWorkService 接口。
    /// </summary>
    internal class NetWorkService : INetWorkService
    {
        /// <summary>
        /// 日志服务接口。
        /// </summary>
        private readonly ILogingService _logingService;

        /// <summary>
        /// HttpClient 对象池。
        /// </summary>
        private readonly ObjectPool<HttpClient> _httpPool;

        /// <summary>
        /// TcpClient 对象池。
        /// </summary>
        private readonly ObjectPool<TcpClient> _tcpPool;

        /// <summary>
        /// 初始化 NetWorkService 类的新实例。
        /// </summary>
        /// <param name="logingService">日志服务接口。</param>
        public NetWorkService(ILogingService logingService)
        {
            _logingService = logingService;
            _httpPool = ObjectPool.Create<HttpClient>();
            _tcpPool = ObjectPool.Create<TcpClient>();
        }

        /// <summary>
        /// 异步发送消息。
        /// </summary>
        /// <param name="message">要发送的消息。</param>
        /// <returns>发送结果。</returns>
        /// <exception cref="NotImplementedException">如果消息类型未实现发送功能。</exception>
        public async Task<object> SendAsync(object message)
        {
            return message switch
            {
                HttpRequestMessage httpRequestMessage => await SendAsync(httpRequestMessage),
                IPEndPoint iPEndPoint => await SendAsync(iPEndPoint),
                _ => throw new NotImplementedException(message.GetType().Name)
            };
        }

        /// <summary>
        /// 异步发送 HttpRequestMessage 请求。
        /// </summary>
        /// <param name="message">要发送的 HttpRequestMessage 请求。</param>
        /// <returns>HttpResponseMessage</returns>
        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
        {
            HttpResponseMessage result = null;
            try
            {
                var httpClient = _httpPool.Get();
                result = await httpClient.SendAsync(message);
                // 检查响应是否成功
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logingService.Error("网络请求发生了错误", nameof(INetWorkService), ex);
            }
            return result;
        }

        /// <summary>
        /// 异步发送 IPEndPoint 请求。
        /// </summary>
        /// <param name="iPEndPoint">要发送的 IPEndPoint。</param>
        /// <returns>网络数据流</returns>
        public async Task<NetworkStream> SendAsync(IPEndPoint iPEndPoint)
        {
            NetworkStream result = null;
            try
            {
                var tcpClient = _tcpPool.Get();
                await tcpClient.ConnectAsync(iPEndPoint);
                result = tcpClient.GetStream();
            }
            catch (Exception ex)
            {
                _logingService.Error("网络请求发生了错误", nameof(INetWorkService), ex);
            }
            return result;
        }
    }
}
