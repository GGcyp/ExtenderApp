using System.Net;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 常用的 HttpResponse 选项标识集合，用于在响应对象中注册和获取类型安全的选项值。
    /// </summary>
    public static class HttpResponseOptions
    {
        /// <summary>
        /// 请求使用的 URI 模式（Scheme），例如 "http" 或 "https"。该选项仅供参考，实际协议可能由上下文决定。
        /// </summary>
        public static readonly OptionIdentifier<string> SchemeOption = new("Scheme", Uri.UriSchemeHttp);

        /// <summary>
        /// 与该响应关联的请求消息（若有）。
        /// </summary>
        public static readonly OptionIdentifier<HttpRequestMessage?> RequestMessageOption = new("RequestMessage");

        /// <summary>
        /// 响应的状态码（如 200、404 等）。默认值为 <see cref="HttpStatusCode.OK"/>。
        /// </summary>
        public static readonly OptionIdentifier<HttpStatusCode> StatusCodeOption = new("StatusCode", HttpStatusCode.OK);

        /// <summary>
        /// 状态短语（Reason-Phrase），例如 "OK" 或 "Not Found"。
        /// </summary>
        public static readonly OptionIdentifier<string> ReasonPhraseOption = new("ReasonPhrase", string.Empty);

        /// <summary>
        /// HTTP 协议版本（例如 HTTP/1.1）。默认值为 <see cref="HttpVersion.Version11"/>。
        /// </summary>
        public static readonly OptionIdentifier<Version> VersionOption = new("Version", HttpVersion.Version11);

        /// <summary>
        /// 响应头集合（不区分大小写的键名比较）。
        /// </summary>
        public static readonly OptionIdentifier<HttpHeader> HeadersOption = new("Headers", defaultValue: new());

        /// <summary>
        /// 响应主体的字节块。注意：该字段持有资源，使用完毕请释放底层缓冲。
        /// </summary>
        public static readonly OptionIdentifier<AbstractBuffer<byte>> BodyOption = new("Body");
    }
}