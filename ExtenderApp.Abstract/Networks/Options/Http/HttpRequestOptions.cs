using System.Net;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 常用的 HttpRequest 选项标识集合，用于在请求对象中注册和获取类型安全的选项值。
    /// </summary>
    public static class HttpRequestOptions
    {
        /// <summary>
        /// 请求目标 URI。
        /// </summary>
        public static readonly OptionIdentifier<Uri?> RequestUriOption = new("RequestUri");

        /// <summary>
        /// HTTP 方法（GET/POST/PUT 等）。
        /// </summary>
        public static readonly OptionIdentifier<HttpMethod> MethodOption = new("Method");

        /// <summary>
        /// HTTP 头集合。
        /// </summary>
        public static readonly OptionIdentifier<HttpHeader> HeadersOption = new("Headers", defaultValue: new());

        /// <summary>
        /// HTTP 查询参数集合。
        /// </summary>
        public static readonly OptionIdentifier<HttpParameters?> ParametersOption = new("Parameters");

        /// <summary>
        /// 请求体（字节块）。
        /// </summary>
        public static readonly OptionIdentifier<AbstractBuffer<byte>?> BodyOption = new("Body");

        /// <summary>
        /// HTTP 版本（如 HTTP/1.1）。
        /// </summary>
        public static readonly OptionIdentifier<Version> VersionOption = new("Version", HttpVersion.Version11);

        /// <summary>
        /// 请求的最大重定向次数。
        /// </summary>
        public static readonly OptionIdentifier<int> MaxRedirectsOption = new("MaxRedirects", 5);

        /// <summary>
        /// 请求超时时间。
        /// </summary>
        public static readonly OptionIdentifier<TimeSpan> TimeoutOption = new("Timeout");

        /// <summary>
        /// 请求Scheme（默认为 "http"）。如果 RequestUri 已设置且包含 Scheme，则以 RequestUri 的 Scheme 为准。此选项主要用于在未设置 RequestUri 时指定默认 Scheme。
        /// </summary>
        public static readonly OptionIdentifier<string> SchemeOption = new("Scheme", Uri.UriSchemeHttp);
    }
}