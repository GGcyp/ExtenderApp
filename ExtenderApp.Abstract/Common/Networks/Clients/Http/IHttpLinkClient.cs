using System.Net.Security;
using HttpRequestMessage = ExtenderApp.Data.HttpRequestMessage;
using HttpResponseMessage = ExtenderApp.Data.HttpResponseMessage;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// Http 链路客户端抽象，扩展自 <see cref="ILinkClient"/>，提供发送 HTTP 请求的能力。
    /// </summary>
    public interface IHttpLinkClient : ILinkClient
    {
        /// <summary>
        /// 发送一个 HTTP 请求并异步等待解析完成的响应。
        /// </summary>
        /// <param name="request">要发送的请求，必须包含 RequestUri。</param>
        /// <param name="options">SSL 配置（当请求为 HTTPS 时使用），若为 null 则使用默认 AuthenticationOptions。</param>
        /// <param name="token">可选取消令牌。</param>
        /// <returns>解析完成的 HttpResponseMessage。</returns>
        ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, SslClientAuthenticationOptions? options = null, CancellationToken token = default);

        /// <summary>
        /// 放入新的 HTTP 解析器实例。
        /// </summary>
        /// <param name="httpParser">解析器实例</param>
        /// <param name="token">解除令牌</param>
        /// <returns>结构体线程任务</returns>
        ValueTask SetHttpParser(IHttpParser httpParser, CancellationToken token = default);
    }
}