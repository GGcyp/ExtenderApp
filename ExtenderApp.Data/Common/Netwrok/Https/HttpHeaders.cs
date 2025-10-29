using System.Net;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 请求头静态类
    /// </summary>
    public static class HttpHeaders
    {
        /// <summary>
        /// Cache-Control 标头，指定请求/响应链上所有缓存控制机制必须服从的指令。
        /// </summary>
        public const string CacheControl = "Cache-Control";

        /// <summary>
        /// Connection 标头，指定特定连接需要的选项。
        /// </summary>
        public const string Connection = "Connection";

        /// <summary>
        /// Date 标头，指定开始创建请求的日期和时间。
        /// </summary>
        public const string Date = "Date";

        /// <summary>
        /// Keep-Alive 标头，指定用以维护持久性连接的参数。
        /// </summary>
        public const string KeepAlive = "Keep-Alive";

        /// <summary>
        /// Pragma 标头，指定可应用于请求/响应链上的任何代理的特定于实现的指令。
        /// </summary>
        public const string Pragma = "Pragma";

        /// <summary>
        /// Trailer 标头，指定标头字段显示在以 chunked 传输编码方式编码的消息的尾部。
        /// </summary>
        public const string Trailer = "Trailer";

        /// <summary>
        /// Transfer-Encoding 标头，指定对消息正文应用的转换的类型（如果有）。
        /// </summary>
        public const string TransferEncoding = "Transfer-Encoding";

        /// <summary>
        /// Upgrade 标头，指定客户端支持的附加通信协议。
        /// </summary>
        public const string Upgrade = "Upgrade";

        /// <summary>
        /// Via 标头，指定网关和代理程序要使用的中间协议。
        /// </summary>
        public const string Via = "Via";

        /// <summary>
        /// Warning 标头，指定关于可能未在消息中反映的消息的状态或转换的附加信息。
        /// </summary>
        public const string Warning = "Warning";

        /// <summary>
        /// Allow 标头，指定支持的 HTTP 方法集。
        /// </summary>
        public const string Allow = "Allow";

        /// <summary>
        /// Content-Length 标头，指定伴随正文数据的长度（以字节为单位）。
        /// </summary>
        public const string ContentLength = "Content-Length";

        /// <summary>
        /// Content-Type 标头，指定伴随正文数据的 MIME 类型。
        /// </summary>
        public const string ContentType = "Content-Type";

        /// <summary>
        /// Content-Encoding 标头，指定已应用于伴随正文数据的编码。
        /// </summary>
        public const string ContentEncoding = "Content-Encoding";

        /// <summary>
        /// Content-Langauge 标头，指定伴随正文数据的自然语言。
        /// </summary>
        public const string ContentLanguage = "Content-Langauge";

        /// <summary>
        /// Content-Location 标头，指定可从其中获得伴随正文的 URI。
        /// </summary>
        public const string ContentLocation = "Content-Location";

        /// <summary>
        /// Content-MD5 标头，指定伴随正文数据的 MD5 摘要，用于提供端到端消息完整性检查。
        /// </summary>
        public const string ContentMd5 = "Content-MD5";

        /// <summary>
        /// Content-Range 标头，指定在完整正文中应用伴随部分正文数据的位置。
        /// </summary>
        public const string ContentRange = "Content-Range";

        /// <summary>
        /// Expires 标头，指定日期和时间，在此之后伴随的正文数据应视为陈旧的。
        /// </summary>
        public const string Expires = "Expires";

        /// <summary>
        /// Last-Modified 标头，指定上次修改伴随的正文数据的日期和时间。
        /// </summary>
        public const string LastModified = "Last-Modified";

        /// <summary>
        /// Accept 标头，指定响应可接受的 MIME 类型。
        /// </summary>
        public const string Accept = "Accept";

        /// <summary>
        /// Accept-Charset 标头，指定响应可接受的字符集。
        /// </summary>
        public const string AcceptCharset = "Accept-Charset";

        /// <summary>
        /// Accept-Encoding 标头，指定响应可接受的内容编码。
        /// </summary>
        public const string AcceptEncoding = "Accept-Encoding";

        /// <summary>
        /// Accept-Langauge 标头，指定响应首选的自然语言。
        /// </summary>
        public const string AcceptLanguage = "Accept-Langauge";

        /// <summary>
        /// Authorization 标头，指定客户端为向服务器验证自身身份而出示的凭据。
        /// </summary>
        public const string Authorization = "Authorization";

        /// <summary>
        /// Cookie 标头，指定向服务器提供的 Cookie 数据。
        /// </summary>
        public const string Cookie = "Cookie";

        /// <summary>
        /// Expect 标头，指定客户端要求的特定服务器行为。
        /// </summary>
        public const string Expect = "Expect";

        /// <summary>
        /// From 标头，指定控制请求用户代理的用户的 Internet 电子邮件地址。
        /// </summary>
        public const string From = "From";

        /// <summary>
        /// Host 标头，指定所请求资源的主机名和端口号。
        /// </summary>
        public const string Host = "Host";

        /// <summary>
        /// If-Match 标头，指定仅当客户端的指示资源的缓存副本是最新的时，才执行请求的操作。
        /// </summary>
        public const string IfMatch = "If-Match";

        /// <summary>
        /// If-Modified-Since 标头，指定仅当自指示的数据和时间之后修改了请求的资源时，才执行请求的操作。
        /// </summary>
        public const string IfModifiedSince = "If-Modified-Since";

        /// <summary>
        /// If-None-Match 标头，指定仅当客户端的指示资源的缓存副本都不是最新的时，才执行请求的操作。
        /// </summary>
        public const string IfNoneMatch = "If-None-Match";

        /// <summary>
        /// If-Range 标头，指定如果客户端的缓存副本是最新的，仅发送指定范围的请求资源。
        /// </summary>
        public const string IfRange = "If-Range";

        /// <summary>
        /// If-Unmodified-Since 标头，指定仅当自指示的日期和时间之后修改了请求的资源时，才执行请求的操作。
        /// </summary>
        public const string IfUnmodifiedSince = "If-Unmodified-Since";

        /// <summary>
        /// Max-Forwards 标头，指定一个整数，表示此请求还可转发的次数。
        /// </summary>
        public const string MaxForwards = "Max-Forwards";

        /// <summary>
        /// Proxy-Authorization 标头，指定客户端为向代理验证自身身份而出示的凭据。
        /// </summary>
        public const string ProxyAuthorization = "Proxy-Authorization";

        /// <summary>
        /// Referer 标头，指定从中获得请求 URI 的资源的 URI。
        /// </summary>
        public const string Referer = "Referer";

        /// <summary>
        /// Range 标头，指定代替整个响应返回的客户端请求的响应的子范围。
        /// </summary>
        public const string Range = "Range";

        /// <summary>
        /// TE 标头，指定响应可接受的传输编码方式。
        /// </summary>
        public const string Te = "TE";

        /// <summary>
        /// Translate 标头，与 WebDAV 功能一起使用的 HTTP 规范的 Microsoft 扩展。
        /// </summary>
        public const string Translate = "Translate";

        /// <summary>
        /// User-Agent 标头，指定有关客户端代理的信息。
        /// </summary>
        public const string UserAgent = "User-Agent";

        /// <summary>
        /// Accept-Ranges 标头，指定服务器接受的范围。
        /// </summary>
        public const string AcceptRanges = "Accept-Ranges";

        /// <summary>
        /// Age 标头，指定自起始服务器生成响应以来的时间长度（以秒为单位）。
        /// </summary>
        public const string Age = "Age";

        /// <summary>
        /// Etag 标头，指定请求的变量的当前值。
        /// </summary>
        public const string ETag = "Etag";

        /// <summary>
        /// Location 标头，指定为获取请求的资源而将客户端重定向到的 URI。
        /// </summary>
        public const string Location = "Location";

        /// <summary>
        /// Proxy-Authenticate 标头，指定客户端必须对代理验证其自身。
        /// </summary>
        public const string ProxyAuthenticate = "Proxy-Authenticate";

        /// <summary>
        /// Retry-After 标头，指定某个时间（以秒为单位）或日期和时间，在此时间之后客户端可以重试其请求。
        /// </summary>
        public const string RetryAfter = "Retry-After";

        /// <summary>
        /// Server 标头，指定关于起始服务器代理的信息。
        /// </summary>
        public const string Server = "Server";

        /// <summary>
        /// Set-Cookie 标头，指定提供给客户端的 Cookie 数据。
        /// </summary>
        public const string SetCookie = "Set-Cookie";

        /// <summary>
        /// Vary 标头，指定用于确定缓存的响应是否为新响应的请求标头。
        /// </summary>
        public const string Vary = "Vary";

        /// <summary>
        /// WWW-Authenticate 标头，指定客户端必须对服务器验证其自身。
        /// </summary>
        public const string WwwAuthenticate = "WWW-Authenticate";

        /// <summary>
        /// Origin。
        /// </summary>
        public const string Origin = "Origin";

        /// <summary>
        /// Content-Disposition
        /// </summary>
        public const string ContentDisposition = "Content-Disposition";

        /// <summary>
        /// 将 HttpRequestHeader 枚举转换为对应的字符串表示。
        /// </summary>
        /// <param name="header">需要被转换的枚举</param>
        /// <returns>转换后的字符串</returns>
        public static string HeaderToString(this HttpRequestHeader header)
        {
            return header switch
            {
                HttpRequestHeader.CacheControl => CacheControl,
                HttpRequestHeader.Connection => Connection,
                HttpRequestHeader.Date => Date,
                HttpRequestHeader.KeepAlive => KeepAlive,
                HttpRequestHeader.Pragma => Pragma,
                HttpRequestHeader.Trailer => Trailer,
                HttpRequestHeader.TransferEncoding => TransferEncoding,
                HttpRequestHeader.Upgrade => Upgrade,
                HttpRequestHeader.Via => Via,
                HttpRequestHeader.Warning => Warning,
                HttpRequestHeader.Allow => Allow,
                HttpRequestHeader.ContentLength => ContentLength,
                HttpRequestHeader.ContentType => ContentType,
                HttpRequestHeader.ContentEncoding => ContentEncoding,
                HttpRequestHeader.ContentLanguage => ContentLanguage,
                HttpRequestHeader.ContentLocation => ContentLocation,
                HttpRequestHeader.ContentMd5 => ContentMd5,
                HttpRequestHeader.ContentRange => ContentRange,
                HttpRequestHeader.Expires => Expires,
                HttpRequestHeader.LastModified => LastModified,
                HttpRequestHeader.Accept => Accept,
                HttpRequestHeader.AcceptCharset => AcceptCharset,
                HttpRequestHeader.AcceptEncoding => AcceptEncoding,
                HttpRequestHeader.AcceptLanguage => AcceptLanguage,
                HttpRequestHeader.Authorization => Authorization,
                HttpRequestHeader.Cookie => Cookie,
                HttpRequestHeader.Expect => Expect,
                HttpRequestHeader.From => From,
                HttpRequestHeader.Host => Host,
                HttpRequestHeader.IfMatch => IfMatch,
                HttpRequestHeader.IfModifiedSince => IfModifiedSince,
                HttpRequestHeader.IfNoneMatch => IfNoneMatch,
                HttpRequestHeader.IfRange => IfRange,
                HttpRequestHeader.IfUnmodifiedSince => IfUnmodifiedSince,
                HttpRequestHeader.MaxForwards => MaxForwards,
                HttpRequestHeader.ProxyAuthorization => ProxyAuthorization,
                HttpRequestHeader.Referer => Referer,
                HttpRequestHeader.Range => Range,
                HttpRequestHeader.Te => Te,
                HttpRequestHeader.Translate => Translate,
                HttpRequestHeader.UserAgent => UserAgent,
                _ => string.Empty
            };
        }

        /// <summary>
        /// 将 HttpResponseHeader 枚举转换为对应的字符串表示。
        /// </summary>
        /// <param name="header">需要被转化的枚举</param>
        /// <returns>转换后的字符串</returns>
        public static string HeaderToString(this HttpResponseHeader header)
        {
            return header switch
            {
                HttpResponseHeader.CacheControl => CacheControl,
                HttpResponseHeader.Connection => Connection,
                HttpResponseHeader.Date => Date,
                HttpResponseHeader.KeepAlive => KeepAlive,
                HttpResponseHeader.Pragma => Pragma,
                HttpResponseHeader.Trailer => Trailer,
                HttpResponseHeader.TransferEncoding => TransferEncoding,
                HttpResponseHeader.Upgrade => Upgrade,
                HttpResponseHeader.Via => Via,
                HttpResponseHeader.Warning => Warning,
                HttpResponseHeader.Allow => Allow,
                HttpResponseHeader.ContentLength => ContentLength,
                HttpResponseHeader.ContentType => ContentType,
                HttpResponseHeader.ContentEncoding => ContentEncoding,
                HttpResponseHeader.ContentLanguage => ContentLanguage,
                HttpResponseHeader.ContentLocation => ContentLocation,
                HttpResponseHeader.ContentMd5 => ContentMd5,
                HttpResponseHeader.ContentRange => ContentRange,
                HttpResponseHeader.Expires => Expires,
                HttpResponseHeader.LastModified => LastModified,
                HttpResponseHeader.AcceptRanges => AcceptRanges,
                HttpResponseHeader.Age => Age,
                HttpResponseHeader.ETag => ETag,
                HttpResponseHeader.Location => Location,
                HttpResponseHeader.ProxyAuthenticate => ProxyAuthenticate,
                HttpResponseHeader.RetryAfter => RetryAfter,
                HttpResponseHeader.Server => Server,
                HttpResponseHeader.SetCookie => SetCookie,
                HttpResponseHeader.Vary => Vary,
                HttpResponseHeader.WwwAuthenticate => WwwAuthenticate,
                _ => string.Empty
            };
        }
    }
}
