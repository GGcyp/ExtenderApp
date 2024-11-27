using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 网络客户端扩展类
    /// </summary>
    public static class NetworkClientExtensions
    {
        /// <summary>
        /// 异步获取网络资源
        /// </summary>
        /// <param name="client">网络客户端</param>
        /// <param name="uri">资源的统一资源标识符</param>
        /// <param name="option">HTTP完成选项</param>
        /// <returns>返回网络响应消息</returns>
        public static ValueTask<NetworkResponseMessage> GetAsync(this INetworkClient client, Uri uri, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        {
            return client.SendAsync(new NetworkRequestMessage(HttpMethod.Get, uri));
        }

        /// <summary>
        /// 异步获取网络资源
        /// </summary>
        /// <param name="client">网络客户端</param>
        /// <param name="uri">资源的统一资源标识符字符串</param>
        /// <param name="option">HTTP完成选项</param>
        /// <returns>返回网络响应消息</returns>
        public static ValueTask<NetworkResponseMessage> GetAsync(this INetworkClient client, string uri, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        {
            return client.SendAsync(new NetworkRequestMessage(HttpMethod.Get, uri));
        }

        /// <summary>
        /// 异步发送POST请求
        /// </summary>
        /// <param name="client">网络客户端</param>
        /// <param name="uri">资源的统一资源标识符</param>
        /// <param name="content">HTTP请求内容</param>
        /// <param name="option">HTTP完成选项</param>
        /// <returns>返回网络响应消息</returns>
        public static ValueTask<NetworkResponseMessage> PostAsync(this INetworkClient client, Uri uri, HttpContent content, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        {
            return client.SendAsync(new NetworkRequestMessage(HttpMethod.Get, uri, content));
        }

        /// <summary>
        /// 异步发送POST请求
        /// </summary>
        /// <param name="client">网络客户端</param>
        /// <param name="uri">资源的统一资源标识符字符串</param>
        /// <param name="content">HTTP请求内容</param>
        /// <param name="option">HTTP完成选项</param>
        /// <returns>返回网络响应消息</returns>
        public static ValueTask<NetworkResponseMessage> PostAsync(this INetworkClient client, string uri, HttpContent content, HttpCompletionOption option = HttpCompletionOption.ResponseContentRead)
        {
            return client.SendAsync(new NetworkRequestMessage(HttpMethod.Get, uri, content));
        }

        /// <summary>
        /// 异步发送网络请求
        /// </summary>
        /// <param name="client">网络客户端</param>
        /// <param name="message">网络请求消息</param>
        /// <returns>返回网络响应消息</returns>
        public static ValueTask<NetworkResponseMessage> SendAsync(this INetworkClient client, NetworkRequestMessage message)
        {
            return client.SendAsync(message, HttpCompletionOption.ResponseContentRead);
        }
    }
}
