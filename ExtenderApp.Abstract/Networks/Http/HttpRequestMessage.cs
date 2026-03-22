using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 表示一个 HTTP 请求消息对象。该对象通过基类的选项集合存储方法、URI、头、参数和请求体等信息， 并提供将请求序列化为字节缓冲区的功能以便发送。
    /// </summary>
    public class HttpRequestMessage : OptionsObject
    {
        /// <summary>
        /// 请求方法（如 GET/POST）。通过选项动态读取/设置。
        /// </summary>
        public HttpMethod Method
        {
            get => TryGetOptionValue(HttpRequestOptions.MethodOption, out HttpMethod v) ? v : HttpMethod.Get;
            set => SetOptionValue(HttpRequestOptions.MethodOption, value);
        }

        /// <summary>
        /// 请求 URI（可以为 null，但序列化时需非空）。通过选项动态读取/设置。
        /// </summary>
        public Uri? RequestUri
        {
            get => TryGetOptionValue(HttpRequestOptions.RequestUriOption, out Uri? v) ? v : null;
            set => SetOptionValue(HttpRequestOptions.RequestUriOption, value!);
        }

        /// <summary>
        /// HTTP 版本（默认 HTTP/1.1）。
        /// </summary>
        public Version Version
        {
            get => TryGetOptionValue(HttpRequestOptions.VersionOption, out Version v) ? v : HttpVersion.Version11;
            set => SetOptionValue(HttpRequestOptions.VersionOption, value);
        }

        /// <summary>
        /// 头部使用的协议方案字符串（"HTTP" 或 "HTTPS"）。
        /// </summary>
        public string Scheme
        {
            get => TryGetOptionValue(HttpRequestOptions.SchemeOption, out string v) ? v : Uri.UriSchemeHttp;
            set => SetOptionValue(HttpRequestOptions.SchemeOption, value);
        }

        /// <summary>
        /// 请求头集合（不区分大小写），存储在 options 中。
        /// </summary>
        public HttpHeader Headers
        {
            get => GetOptionValue(HttpRequestOptions.HeadersOption);
            set => SetOptionValue(HttpRequestOptions.HeadersOption, value);
        }

        /// <summary>
        /// 查询/表单参数集合（延迟创建）。保持为本地 lazy，因为未在 HttpRequestOptions 中定义 ParametersOption。
        /// </summary>
        public HttpParameters? Params
        {
            get => GetOptionValue(HttpRequestOptions.ParametersOption);
            set => SetOptionValue(HttpRequestOptions.ParametersOption, value);
        }

        /// <summary>
        /// 请求体（ByteBlock），存储在 options 中。设置新值时会 Release 旧值以释放资源。
        /// </summary>
        public AbstractBuffer<byte>? Body
        {
            get => GetOptionValue(HttpRequestOptions.BodyOption);
            set
            {
                if (TryGetOptionValue(HttpRequestOptions.BodyOption, out AbstractBuffer<byte>? prev))
                {
                    prev?.TryRelease();
                }
                SetOptionValue(HttpRequestOptions.BodyOption, value);
            }
        }

        /// <summary>
        /// 检查当前请求是否使用 HTTPS 协议（Scheme 为 "HTTPS"，不区分大小写）。如果 Scheme 不是 "HTTPS"，则默认为 HTTP。
        /// </summary>
        public bool IsHttps => Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 创建一个默认的请求（Method = GET）。
        /// </summary>
        public HttpRequestMessage() : this(HttpMethod.Get, (Uri?)null)
        {
        }

        /// <summary>
        /// 使用指定方法和字符串形式的 URI 创建请求。
        /// </summary>
        /// <param name="method">HTTP 方法。</param>
        /// <param name="requestUri">请求 URI 字符串（可为 null）。</param>
        public HttpRequestMessage(HttpMethod method, [StringSyntax("Uri")] string requestUri) : this(method, string.IsNullOrEmpty(requestUri) ? null : new Uri(requestUri))
        {
        }

        /// <summary>
        /// 构造函数。注册常用选项到基类选项集合中。
        /// </summary>
        /// <param name="method">HTTP 方法，不能为空。</param>
        /// <param name="requestUri">可选的请求 URI（可为 null）。</param>
        public HttpRequestMessage(HttpMethod method, Uri? requestUri)
        {
            if (method.IsEmpty)
                throw new ArgumentException("HTTP 方法不能为空", nameof(method));

            RegisterOption(HttpRequestOptions.MethodOption, method);
            RegisterOption(HttpRequestOptions.BodyOption);
            RegisterOption(HttpRequestOptions.HeadersOption);
            RegisterOption(HttpRequestOptions.TimeoutOption);
            RegisterOption(HttpRequestOptions.ParametersOption);
            RegisterOption(HttpRequestOptions.MaxRedirectsOption);
            RegisterOption(HttpRequestOptions.RequestUriOption, requestUri, (s, v) =>
            {
                SetOptionValue(HttpRequestOptions.SchemeOption, v.Item2?.Scheme ?? Uri.UriSchemeHttp);
            });
            RegisterOption(HttpRequestOptions.SchemeOption, requestUri?.Scheme ?? Uri.UriSchemeHttp);
        }

        /// <summary>
        /// 设置文本内容（只写入 Body，并可设置 Content-Type；Content-Committed 不在此处设置，由序列化前统一补齐）。
        /// </summary>
        /// <param name="text">文本内容。</param>
        /// <param name="contentType">可选 Content-Type（不含 charset 时会使用编码的 charset）。</param>
        public void SetContent(string text, string? contentType = null)
        {
            SetContent(text, Encoding.GetEncoding(0), contentType);
        }

        /// <summary>
        /// 使用指定编码设置文本内容并更新 Content-Type（如未指定 contentType，则使用 text/plain 并带上编码）。
        /// </summary>
        /// <param name="text">文本内容。</param>
        /// <param name="encoding">文本编码。</param>
        /// <param name="contentType">可选 Content-Type。</param>
        public void SetContent(string text, Encoding encoding, string? contentType = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            WriteToBody(text, encoding);
            Headers.SetOptionValue(HttpHeaderOptions.ContentTypeIdentifier, string.IsNullOrEmpty(contentType) ? $"text/plain; charset={encoding.WebName}" : contentType);
        }

        /// <summary>
        /// 设置二进制内容（写入 Body，仅设置 Content-Type，不设置 Content-Committed）。
        /// </summary>
        /// <param name="buffer">要写入的字节块（拷贝到内部 ByteBlock）。</param>
        /// <param name="contentType">Content-Type，默认 application/octet-stream。</param>
        public void SetContent(AbstractBuffer<byte> buffer, string contentType = "application/octet-stream")
        {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));

            Body = buffer;
            Headers.SetOptionValue(HttpHeaderOptions.ContentTypeIdentifier, contentType);
        }

        /// <summary>
        /// 使用系统默认编码将当前 Params 转为 application/x-www-form-urlencoded 并作为请求体。 该操作会覆盖现有 Body。
        /// </summary>
        public void SetFormContentFromParams()
        {
            SetFormContentFromParams(Encoding.GetEncoding(0));
        }

        /// <summary>
        /// 使用指定编码将当前 Params 转为 application/x-www-form-urlencoded 并作为请求体。 该操作会覆盖现有 Body，并设置相应的 Content-Type。
        /// </summary>
        /// <param name="encoding">用于编码表单的字符编码。</param>
        public void SetFormContentFromParams(Encoding encoding)
        {
            ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));

            if (Params == null)
                return;

            var form = Params.ToFormUrlEncodedString();
            WriteToBody(form, encoding);

            string contentType = string.Format("application/x-www-form-urlencoded; charset={0}", encoding.WebName);
            if (!Headers.TrySetOptionValue(HttpHeaderOptions.ContentTypeIdentifier, contentType))
            {
                Headers.RegisterOption(HttpHeaderOptions.ContentTypeIdentifier, contentType);
            }
        }

        /// <summary>
        /// 将指定字符串按给定编码写入到 Body（覆盖原 Body）。
        /// </summary>
        /// <param name="bodyText">要写入的文本。</param>
        /// <param name="encoding">用于编码的字符集。</param>
        private void WriteToBody(string bodyText, Encoding encoding)
        {
            var byteCount = encoding.GetMaxByteCount(bodyText.Length);
            var block = MemoryBlock<byte>.GetBuffer(byteCount);
            byteCount = encoding.GetBytes(bodyText, block.GetSpan(byteCount));
            block.Advance(byteCount);
            Body = block;
        }

        /// <summary>
        /// 生成请求行与头部（包含必要时把 Params 追加到请求 URI）的字符串表示，返回用于发送的首部字符串（不包含二进制 Body）。
        /// </summary>
        /// <returns>包含请求行与头部的字符串（以 CRLF 结尾）。</returns>
        /// <exception cref="InvalidOperationException">当 RequestUri 为 null 时抛出。</exception>
        public string GetRequestString()
        {
            if (RequestUri is null)
                throw new InvalidOperationException("请求URI不能为空");

            StringBuilder sb = new();
            sb.Append(Method.ToString());
            sb.Append(HttpConstants.Space);
            //当没有查询字符串时，PathAndQuery 可能会返回空字符串，此时应该使用 AbsolutePath（如果也为空则用 "/"）来确保请求行格式正确
            var pathAndQuery = string.IsNullOrEmpty(RequestUri.PathAndQuery) ? (RequestUri.AbsolutePath ?? "/") : RequestUri.PathAndQuery;
            sb.Append(pathAndQuery);

            bool skipParams = false;
            if (Method.Equals(HttpMethod.Get))
            {
                if (Headers.TryGetOptionValue(HttpHeaderOptions.ContentTypeIdentifier, out string ct) &&
                    !string.IsNullOrEmpty(ct) &&
                    ct.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) &&
                    Body != null &&
                    Body != AbstractBuffer<byte>.Empty &&
                    Body.Committed > 0)
                {
                    skipParams = true;
                }

                if (!skipParams && Params != null)
                {
                    Params.BuildQuery(sb, !string.IsNullOrEmpty(RequestUri.Query));
                }
            }

            sb.Append(HttpConstants.Space);
            sb.Append(Scheme.ToUpperInvariant());
            sb.Append(HttpConstants.Slash);
            sb.Append(Version.Major);
            sb.Append(HttpConstants.Dot);
            sb.Append(Version.Minor);
            sb.Append(HttpConstants.NextLine);

            Headers.EnsureRequestHeaders(RequestUri, Body);

            Headers.BuildHeaderBlock(sb, combineValues: false);

            sb.Append(HttpConstants.NextLine);

            return sb.ToString();
        }

        /// <summary>
        /// 获取包含完整请求（首行 + 头部 + 可选 Body）的字节缓冲，文本部分使用 ASCII 编码。该方法适用于需要直接获取字节数据进行发送的场景。
        /// </summary>
        /// <param name="combineValues">是否将具有相同名称的多个头部值合并为一个头部行（用逗号分隔）。默认为 false，即不合并。</param>
        /// <returns>包含请求数据的字节缓冲（可能为 SequenceBuffer）。</returns>
        public SequenceBuffer<byte> GetRequestBuffer(bool combineValues = false) => GetRequestBuffer(Encoding.ASCII, combineValues);

        /// <summary>
        /// 将完整请求（首行 + 头部 + 可选 Body）写入到提供的序列缓冲中，文本部分使用指定的编码。
        /// </summary>
        /// <param name="encoding">用于转换文本部分的编码。</param>
        /// <param name="combineValues">是否将具有相同名称的多个头部值合并为一个头部行（用逗号分隔）。默认为 false，即不合并。</param>
        /// <returns>包含请求数据的字节缓冲（可能为 SequenceBuffer）。</returns>
        public SequenceBuffer<byte> GetRequestBuffer(Encoding encoding, bool combineValues = false)
        {
            SequenceBuffer<byte> sequence = SequenceBuffer<byte>.GetBuffer();
            WriteToBuffer(sequence, encoding, combineValues);
            return sequence;
        }

        /// <summary>
        /// 将完整请求（首行、头部与 Body）写入指定的 <paramref name="buffer"/>。文本部分使用 ASCII 编码。
        /// </summary>
        /// <param name="buffer">目标序列缓冲。</param>
        /// <param name="combineValues">是否将具有相同名称的多个头部值合并为一个头部行（用逗号分隔）。默认为 false，即不合并。</param>
        public void WriteToBuffer(SequenceBuffer<byte> buffer, bool combineValues = false) => WriteToBuffer(buffer, Encoding.ASCII, combineValues);

        /// <summary>
        /// 将完整请求（首行、头部与 Body）写入指定的 <paramref name="buffer"/>。文本部分使用提供的 <paramref name="encoding"/> 编码。
        /// </summary>
        /// <param name="buffer">目标序列缓冲。</param>
        /// <param name="encoding">用于文本编码的编码器。</param>
        /// <param name="combineValues">是否将具有相同名称的多个头部值合并为一个头部行（用逗号分隔）。默认为 false，即不合并。</param>
        /// <exception cref="ArgumentNullException">当 buffer 或 encoding 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">当 RequestUri 为 null 时抛出。</exception>
        public void WriteToBuffer(SequenceBuffer<byte> buffer, Encoding encoding, bool combineValues = false)
        {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
            ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));

            if (RequestUri is null)
                throw new InvalidOperationException("请求URI不能为空");

            //请求行：METHOD SP Request-URI SP HTTP-Version CRLF
            buffer.Write(Method.ToString(), encoding);
            buffer.Write(" ", encoding);
            //当没有查询字符串时，PathAndQuery 可能会返回空字符串，此时应该使用 AbsolutePath（如果也为空则用 "/"）来确保请求行格式正确
            var pathAndQuery = string.IsNullOrEmpty(RequestUri.PathAndQuery) ? (RequestUri.AbsolutePath ?? "/") : RequestUri.PathAndQuery;
            buffer.Write(pathAndQuery, encoding);

            // 请求头中如果是 GET 且 Content-Type 是 application/x-www-form-urlencoded 且 Body 有内容，则不将 Params 写入查询字符串（因为 Params 已经通过 Body 以表单形式提交了）
            bool skipParams = false;
            if (Method.Equals(HttpMethod.Get))
            {
                if (Headers.TryGetOptionValue(HttpHeaderOptions.ContentTypeIdentifier, out string ct) &&
                    !string.IsNullOrEmpty(ct) &&
                    ct.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) &&
                    Body != null &&
                    Body != AbstractBuffer<byte>.Empty &&
                    Body.Committed > 0)
                {
                    skipParams = true;
                }

                if (!skipParams && Params != null)
                {
                    Params.WriteQueryToBuffer(buffer, !string.IsNullOrEmpty(RequestUri.Query));
                }
            }

            buffer.Write(" ", encoding);
            buffer.Write(Scheme.ToUpperInvariant(), encoding);
            buffer.Write(HttpConstants.Slash, encoding);
            buffer.Write(Version.Major.ToString(), encoding);
            buffer.Write(HttpConstants.Dot, encoding);
            buffer.Write(Version.Minor.ToString(), encoding);
            buffer.Write(HttpConstants.NextLine, encoding);

            Headers.EnsureRequestHeaders(RequestUri, Body);

            Headers.BuildHeaderBlock(buffer, combineValues);
            buffer.Write(HttpConstants.NextLine, encoding);

            if (Body != null &&
                Body != AbstractBuffer<byte>.Empty &&
                Body.Committed > 0)
            {
                buffer.Append(Body);
            }
        }

        public override string ToString() => GetRequestString();
    }
}