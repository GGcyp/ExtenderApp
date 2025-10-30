using System.Net;
using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 请求头静态类
    /// </summary>
    public static class HttpHeaders
    {
        #region Http报文常量

        /// <summary>
        /// 空格字符常量，常用于构造/解析 HTTP 报文时作为分隔符。
        /// </summary>
        public const char SpaceChar = ' ';

        /// <summary>
        /// 点字符常量，常用于文件扩展名、主机名或版本号等的拼接与解析。
        /// </summary>
        public const char DotChar = '.';

        /// <summary>
        /// HTTP 报文行结束符（CRLF），用于分隔头部行与行尾。
        /// </summary>
        public const string NextLine = "\r\n";

        #endregion

        #region HttpHeaders Constants

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

        #endregion HttpHeaders Constants

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

        /// <summary>
        /// 将 <see cref="StringBuilder"/> 的字符内容按指定 <see cref="Encoding"/> 编码并与指定的 <see cref="ByteBlock"/> 合并，
        /// 写入一个新创建的 <see cref="ByteBuffer"/> 中。
        /// </summary>
        /// <param name="builder">要编码写入的 <see cref="StringBuilder"/> 实例（按块读取以避免中间拷贝）。</param>
        /// <param name="body">要追加到缓冲末尾的字节块（可为空块）。</param>
        /// <param name="buffer">输出参数：包含已写入数据的新建 <see cref="ByteBuffer"/>。调用方需在适当时机释放/归还其资源（若实现了 Dispose）。</param>
        /// <param name="encoding">用于将字符编码为字节的编码；若为 <c>null</c>，默认使用 <see cref="Encoding.ASCII"/>。</param>
        /// <remarks>
        /// 实现细节：
        /// - 先根据编码和字符串长度估算所需字节数（使用 <see cref="Encoding.GetMaxByteCount"/>），并加上 body.Length 作为申请大小提示；
        /// - 通过 <see cref="ByteBuffer.CreateBuffer"/> 创建目标缓冲并调用 <see cref="ByteBuffer.GetSpan(int)"/> 获取可写 <see cref="Span{T}"/>；
        /// - 使用 <see cref="Encoder"/> 对 <see cref="StringBuilder"/> 的每个块逐段编码到目标 span，最后调用一次带 flush 的 Convert 将编码器状态刷新干净；
        /// - 调用 <see cref="ByteBuffer.WriteAdvance(int)"/> 提交已写入字节数，然后将 body 追加到缓冲末尾。
        /// 注意：<see cref="ByteBuffer"/> 为 ref struct，使用时请注意其生命周期与释放语义。
        /// </remarks>
        public static void BuildByteBuffer(this StringBuilder builder, ByteBlock body, out ByteBuffer buffer, Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            int length = encoding.GetMaxByteCount(builder.Length) + body.Length;
            buffer = ByteBuffer.CreateBuffer();
            Span<byte> span = buffer.GetSpan(length);

            Encoder encoder = encoding.GetEncoder();
            int spanPos = 0;

            foreach (var chunkMem in builder.GetChunks())
            {
                ReadOnlySpan<char> chars = chunkMem.Span;
                while (!chars.IsEmpty)
                {
                    // 将一段 chars 编码到 dest.Slice(spanPos)
                    encoder.Convert(chars, span.Slice(spanPos), false, out int charsUsed, out int bytesUsed, out bool completed);
                    chars = chars.Slice(charsUsed);
                    spanPos += bytesUsed;
                }
            }

            // flush any encoder state
            encoder.Convert(ReadOnlySpan<char>.Empty, span.Slice(spanPos), true, out int _, out int bytesFlushed, out bool _);
            spanPos += bytesFlushed;

            // 提交写入的字节数
            buffer.WriteAdvance(spanPos);

            buffer.Write(body);
            return;
        }
    }
}