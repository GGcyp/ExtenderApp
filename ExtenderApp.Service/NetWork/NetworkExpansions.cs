using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 网络扩展类，提供对INetWorkService接口的扩展方法
    /// </summary>
    public static class NetworkExpansions
    {
        #region Http

        /// <summary>
        /// 通过网络服务的异步方式获取字符串。
        /// </summary>
        /// <param name="service">网络服务接口。</param>
        /// <param name="uri">资源的统一资源标识符。</param>
        /// <returns>异步任务，任务结果为获取到的字符串。</returns>
        public static async Task<string> GetStringAsync(this INetWorkService service, string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException(nameof(uri));

            return await service.GetStringAsync(new Uri(uri));
        }

        /// <summary>
        /// 通过网络服务的异步方式获取字符串。
        /// </summary>
        /// <param name="service">网络服务接口。</param>
        /// <param name="uri">资源的统一资源标识符。</param>
        /// <returns>异步任务，任务结果为获取到的字符串。</returns>
        public static async Task<string> GetStringAsync(this INetWorkService service, Uri uri)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));

            HttpResponseMessage httpResponse = await HttpSendAsync(service, new HttpRequestMessage(HttpMethod.Get, uri));
            return await httpResponse.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 以异步方式获取指定URI的字符串内容，并调用回调函数处理结果。
        /// </summary>
        /// <param name="service">网络服务接口实例。</param>
        /// <param name="uri">要获取的字符串内容的URI。</param>
        /// <param name="callback">处理结果的回调函数，参数为获取的字符串内容。</param>
        public static async void GetStringAsync(this INetWorkService service, string uri, Action<string> callback)
        {
            var result = await service.GetStringAsync(uri);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 以异步方式获取指定URI的字符串内容，并调用回调函数处理结果。
        /// </summary>
        /// <param name="service">网络服务接口实例。</param>
        /// <param name="uri">要获取的字符串内容的Uri对象。</param>
        /// <param name="callback">处理结果的回调函数，参数为获取的字符串内容。</param>
        public static async void GetStringAsync(this INetWorkService service, Uri uri, Action<string> callback)
        {
            var result = await service.GetStringAsync(uri);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步获取网络流。
        /// </summary>
        /// <param name="service">网络服务对象。</param>
        /// <param name="uri">资源的统一资源标识符。</param>
        /// <param name="callback">获取到流后的回调函数。</param>
        public static async void GetStreamAsync(this INetWorkService service, string uri, Action<Stream> callback)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException(nameof(uri));

            var result = await service.GetStreamAsync(new Uri(uri));
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步获取网络流。
        /// </summary>
        /// <param name="service">网络服务对象。</param>
        /// <param name="uri">资源的统一资源标识符。</param>
        /// <param name="callback">获取到流后的回调函数。</param>
        public static async void GetStreamAsync(this INetWorkService service, Uri uri, Action<Stream> callback)
        {
            var result = await service.GetStreamAsync(uri);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步获取流
        /// </summary>
        /// <param name="service">网络服务对象</param>
        /// <param name="uri">统一资源标识符</param>
        /// <returns>返回异步流</returns>
        public static async Task<Stream> GetStreamAsync(this INetWorkService service, string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException(nameof(uri));

            return await service.GetStreamAsync(new Uri(uri));
        }

        /// <summary>
        /// 异步获取流
        /// </summary>
        /// <param name="service">网络服务对象</param>
        /// <param name="uri">统一资源标识符</param>
        /// <returns>返回异步流</returns>
        public static async Task<Stream> GetStreamAsync(this INetWorkService service, Uri uri)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));

            HttpResponseMessage httpResponse = await HttpSendAsync(service, new HttpRequestMessage(HttpMethod.Get, uri));
            return await httpResponse.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// 异步发送HTTP请求，并调用回调函数处理HTTP响应
        /// </summary>
        /// <param name="service">INetWorkService接口实例</param>
        /// <param name="httpRequest">HTTP请求消息</param>
        /// <param name="callback">处理HTTP响应的回调函数</param>
        public static async void HttpSendAsync(this INetWorkService service, HttpRequestMessage httpRequest, Action<HttpResponseMessage> callback)
        {
            var httpResponse = await service.HttpSendAsync(httpRequest);
            callback?.Invoke(httpResponse);
        }

        /// <summary>
        /// 异步发送HTTP请求
        /// </summary>
        /// <param name="service">INetWorkService接口实例</param>
        /// <param name="httpRequest">HTTP请求消息</param>
        /// <returns>异步任务，返回HTTP响应消息</returns>
        public static async Task<HttpResponseMessage> HttpSendAsync(this INetWorkService service, HttpRequestMessage httpRequest)
        {
            if (service is null)
                throw new ArgumentNullException(nameof(INetWorkService));

            if (httpRequest is null)
                throw new ArgumentNullException(nameof(HttpRequestMessage));

            return (HttpResponseMessage)await service.SendAsync(httpRequest);
        }

        #endregion

        #region Tcp

        /// <summary>
        /// 异步发送TCP请求，并调用回调函数处理NetworkStream
        /// </summary>
        /// <param name="service">INetWorkService接口实例</param>
        /// <param name="ip">目标IP地址和端口</param>
        /// <param name="callback">处理NetworkStream的回调函数</param>
        public static async void TcpSendAsync(this INetWorkService service, IPEndPoint ip, Action<NetworkStream> callback)
        {
            var result = await service.TcpSendAsync(ip);
            callback?.Invoke(result);
        }

        /// <summary>
        /// 异步发送TCP请求
        /// </summary>
        /// <param name="service">INetWorkService接口实例</param>
        /// <param name="ip">目标IP地址和端口</param>
        /// <returns>异步任务，返回NetworkStream</returns>
        public static async Task<NetworkStream> TcpSendAsync(this INetWorkService service, IPEndPoint ip)
        {
            if (service is null)
                throw new ArgumentNullException(nameof(INetWorkService));

            if (ip is null)
                throw new ArgumentNullException(nameof(IPEndPoint));

            return (NetworkStream)await service.SendAsync(ip);
        }

        #endregion
    }
}
