

namespace ExtenderApp.Data
{
    /// <summary>
    /// HttpRequest 类用于处理 HTTP 请求。
    /// </summary>
    public class HttpRequest : NetworkRequest
    {
        /// <summary>
        /// 获取或设置HTTP请求消息
        /// </summary>
        public HttpRequestMessage HttpRequestMessage { get; set; }

        public HttpRequest(HttpRequestMessage message)
        {
            HttpRequestMessage = message;
        }
    }
}
