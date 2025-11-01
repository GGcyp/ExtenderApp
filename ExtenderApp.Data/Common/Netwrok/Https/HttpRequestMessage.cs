using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个可序列化为 HTTP 请求字节流的消息对象。
    /// - 支持 Method / RequestUri / Version / Headers / Params / Body。
    /// - 使用 <see cref="ToBuffer"/> 将请求序列化为可发送的 <see cref="ByteBuffer"/>。
    /// - 本类型继承自 <see cref="DisposableObject"/>, 使用完毕请调用 <see cref="Dispose"/> 以释放 Body。
    /// </summary>
    public class HttpRequestMessage : DisposableObject
    {
        private readonly Lazy<HttpParams> _paramsLazy;
        private readonly Lazy<HttpHeader> _headerLazy;

        /// <summary>
        /// 请求方法（如 GET/POST）。
        /// </summary>
        public HttpMethod Method { get; set; }

        private Uri? requestUri;
        /// <summary>
        /// 请求 URI（可以为 null，但序列化时需非空）。
        /// </summary>
        public Uri? RequestUri
        {
            get => requestUri;
            set
            {
                requestUri = value;
                if (requestUri is not null)
                    Scheme = requestUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ? 
                        HttpHeaders.HttpsChars : 
                        HttpHeaders.HttpChars;
            }
        }

        /// <summary>
        /// HTTP 版本（默认 HTTP/1.1）。
        /// </summary>
        public Version Version { get; set; }

        public string? Scheme { get; private set; }

        /// <summary>
        /// 请求头集合（不区分大小写）。
        /// </summary>
        public HttpHeader Headers => _headerLazy.Value;

        /// <summary>
        /// 查询/表单参数集合（延迟创建）。
        /// </summary>
        public HttpParams Params => _paramsLazy.Value;

        /// <summary>
        /// 检查请求是否使用 HTTPS。
        /// </summary>
        public bool IsHttps => RequestUri != null && RequestUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 请求体字节块（结构体），注意需要在不再使用时释放。
        /// </summary>
        public ByteBlock Body { get; private set; }

        /// <summary>
        /// 创建一个默认的请求（Method = GET, RequestUri = 空字符串 -> new Uri("") 会抛，使用重载构造）。
        /// 注意：这里不再做额外副作用操作（例如不再创建 TcpClient）。
        /// </summary>
        public HttpRequestMessage() : this(HttpMethod.Get, string.Empty)
        {
        }

        public HttpRequestMessage(HttpMethod method, [StringSyntax("Uri")] string requestUri) : this(method, string.IsNullOrEmpty(requestUri) ? null : new Uri(requestUri))
        {
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="method">HTTP 方法。</param>
        /// <param name="requestUri">可选的请求 URI。</param>
        public HttpRequestMessage(HttpMethod method, Uri? requestUri)
        {
            if (method.IsEmpty)
                throw new ArgumentException("HTTP 方法不能为空", nameof(method));

            _paramsLazy = new Lazy<HttpParams>(static () => new HttpParams());
            _headerLazy = new Lazy<HttpHeader>(static () => new HttpHeader());
            Method = method;
            RequestUri = requestUri;
            Version = HttpVersion.Version11;
            Body = default;
        }

        /// <summary>
        /// 设置文本内容（只写入 Body，并可设置 Content-Type；Content-Length 不在此处设置，由序列化前统一补齐）。
        /// </summary>
        /// <param name="text">文本内容。</param>
        /// <param name="encoding">可选编码（默认 UTF-8）。</param>
        /// <param name="contentType">可选 Content-Type（不含 charset 时会使用编码的 charset）。</param>
        public void SetContent(string text, Encoding? encoding = null, string? contentType = null)
        {
            encoding ??= Encoding.UTF8;
            WriteToBody(text, encoding);

            // 仅在 caller 提供 contentType 时设置；Content-Length 由 EnsureRequestHeaders 在序列化前统一设置
            if (!string.IsNullOrEmpty(contentType))
            {
                Headers.SetValue(HttpHeaders.ContentType, contentType);
            }
            else
            {
                // 保留 convenience：若未传 contentType，设置合理的 text/plain charset（可被 EnsureRequestHeaders 覆盖/补齐）
                Headers.SetValue(HttpHeaders.ContentType, $"text/plain; charset={encoding.WebName}");
            }
        }

        /// <summary>
        /// 设置二进制内容（写入 Body，仅设置 Content-Type，不设置 Content-Length）。
        /// </summary>
        /// <param name="span">要写入的字节切片（拷贝到内部 ByteBlock）。</param>
        /// <param name="contentType">Content-Type，默认 application/octet-stream。</param>
        public void SetContent(Span<byte> span, string? contentType = "application/octet-stream")
        {
            if (span.IsEmpty)
            {
                return;
            }

            Body.Dispose();
            Body = new ByteBlock(span.Length);
            Body.Write(span);

            if (!string.IsNullOrEmpty(contentType))
                Headers.SetValue(HttpHeaders.ContentType, contentType);
        }

        /// <summary>
        /// 设置二进制内容（写入 Body，仅设置 Content-Type，不设置 Content-Length）。
        /// </summary>
        /// <param name="block">要写入的字节块（拷贝到内部 ByteBlock）。</param>
        /// <param name="contentType">Content-Type，默认 application/octet-stream。</param>
        public void SetContent(ByteBlock block, string? contentType = "application/octet-stream")
        {
            if (block.IsEmpty)
            {
                return;
            }

            Body.Dispose();
            Body = new ByteBlock(block.Remaining);
            Body.Write(block);

            if (!string.IsNullOrEmpty(contentType))
                Headers.SetValue(HttpHeaders.ContentType, contentType);
        }

        /// <summary>
        /// 将当前 Params 以 application/x-www-form-urlencoded 作为请求体设置（覆盖现有 Body）。
        /// 只设置 Content-Type；Content-Length 由序列化前统一补齐。
        /// </summary>
        public void SetFormContentFromParams(Encoding? encoding = null)
        {
            if (!_paramsLazy.IsValueCreated)
                throw new InvalidOperationException("Params 未创建，无法设置表单内容");

            encoding ??= Encoding.UTF8;
            var form = Params.ToFormUrlEncodedString();
            WriteToBody(form, encoding);

            Headers.SetValue(HttpHeaders.ContentType, string.Format("application/x-www-form-urlencoded; charset={0}", encoding.WebName));
        }

        /// <summary>
        /// 序列化文本内容到 Body。
        /// </summary>
        /// <param name="bodyText">需要被序列化的文本。</param>
        /// <param name="encoding">字符编码方式。</param>
        private void WriteToBody(string bodyText, Encoding encoding)
        {
            Body.Dispose();
            int length = encoding.GetMaxByteCount(bodyText.Length);
            Body = new ByteBlock(length);
            Span<byte> span = Body.GetSpan(length);
            int actualLength = encoding.GetBytes(bodyText ?? string.Empty, span);
            Body.WriteAdvance(actualLength);
        }

        /// <summary>
        /// 序列化请求（start-line + headers + CRLF + body）为 ByteBuffer，可直接发送到网络流。
        /// - 在写出头部前通过 EnsureRequestHeaders 统一补齐 Host / Content-Length 等。
        /// </summary>
        /// <param name="encoding">用于将 headers/start-line 编码为字节的编码，默认 ASCII（HTTP 头通常用 ASCII 或 ISO-8859-1）。</param>
        public ByteBuffer ToBuffer(Encoding? encoding = null)
        {
            if (RequestUri is null)
                throw new InvalidOperationException("请求URI不能为空");

            StringBuilder sb = new();
            sb.Append(Method);
            sb.Append(HttpHeaders.SpaceChar);
            sb.Append(RequestUri.AbsolutePath ?? "/");
            sb.Append(string.IsNullOrEmpty(RequestUri.Query) ? '?' : RequestUri.Query);
            // 如果是 GET 请求且有 Params，则把 Params 追加为查询字符串
            if (Method.Equals(HttpMethod.Get) && _paramsLazy.IsValueCreated)
            {
                Params.BuildQuery(sb, !string.IsNullOrEmpty(RequestUri.Query));
            }

            sb.Append(HttpHeaders.SpaceChar);
            sb.Append(Scheme);
            sb.Append(HttpHeaders.SlashChar);
            sb.Append(Version.Major);
            sb.Append(HttpHeaders.DotChar);
            sb.Append(Version.Minor);
            sb.Append(HttpHeaders.NextLine);

            // 统一在这里补齐请求所需的头（Host / Content-Length / 可选默认 Content-Type）
            Headers.EnsureRequestHeaders(RequestUri, Body);

            // 由 HttpHeader.BuildHeaderBlock 负责把所有头写出
            Headers.BuildHeaderBlock(sb, combineValues: false);

            // 头部与主体之间的空行
            sb.Append(HttpHeaders.NextLine);

            encoding ??= Encoding.ASCII;

            sb.BuildByteBuffer(Body, out ByteBuffer buffer, encoding);

            return buffer;
        }

        protected override void Dispose(bool disposing)
        {
            Body.Dispose();
        }
    }
}