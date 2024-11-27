using System.Net;
using System.Net.Http.Headers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 网络响应消息结构体
    /// </summary>
    public struct NetworkResponseMessage
    {
        /// <summary>
        /// HTTP状态码
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 获取或设置表示HTTP响应状态码是否为成功的布尔值。
        /// </summary>
        public bool IsSuccessStatusCode => StatusCode == HttpStatusCode.OK;

        /// <summary>
        /// HTTP响应头
        /// </summary>
        public HttpResponseHeaders Headers { get; set; }

        /// <summary>
        /// HTTP响应内容
        /// </summary>
        public HttpContent Content { get; set; }

        /// <summary>
        /// HTTP版本
        /// </summary>
        public Version Version { get; set; }
    }
}
