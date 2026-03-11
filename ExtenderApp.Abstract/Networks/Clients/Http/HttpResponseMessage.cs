using System.Net;
using System.Text;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 表示 HTTP 响应消息（状态行 + 头部 + 可选主体）。
    /// 此对象可能持有底层字节缓冲区资源（<see cref="Body"/>），使用完毕请调用 Dispose 或释放相关资源。
    /// </summary>
    public class HttpResponseMessage : OptionsObject
    {
        /// <summary>
        /// 与该响应关联的请求消息（若有）。仅供参考，可能为 <c>null</c>。
        /// </summary>
        public HttpRequestMessage? RequestMessage
            => GetOptionValue(HttpResponseOptions.RequestMessageOption);

        /// <summary>
        /// 响应头集合（不区分大小写的键名比较）。
        /// </summary>
        public HttpHeader Headers
        {
            get => GetOptionValue(HttpResponseOptions.HeadersOption);
            set => SetOptionValue(HttpResponseOptions.HeadersOption, value);
        }

        /// <summary>
        /// 响应主体的字节块。注意：该字段持有资源，使用完毕请调用 <see cref="Dispose"/> 回收底层缓冲。
        /// </summary>
        public AbstractBuffer<byte> Body
        {
            get => GetOptionValue(HttpResponseOptions.BodyOption);
            set
            {
                if (TryGetOptionValue(HttpResponseOptions.BodyOption, out var buffer))
                {
                    buffer.TryRelease();
                }
                SetOptionValue(HttpResponseOptions.BodyOption, value);
            }
        }

        /// <summary>
        /// 响应的状态码（如 200、404 等）。默认值为 <see cref="HttpStatusCode.OK"/>。
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get => GetOptionValue(HttpResponseOptions.StatusCodeOption);
            set => SetOptionValue(HttpResponseOptions.StatusCodeOption, value);
        }

        /// <summary>
        /// 状态短语（Reason-Phrase），例如 "OK" 或 "Not Found"。序列化时会写入状态行。
        /// </summary>
        public string ReasonPhrase
        {
            get => GetOptionValue(HttpResponseOptions.ReasonPhraseOption);
            set => SetOptionValue(HttpResponseOptions.ReasonPhraseOption, value);
        }

        /// <summary>
        /// HTTP 协议版本（例如 HTTP/1.1）。默认值为 <see cref="HttpVersion.Version11"/>。
        /// </summary>
        public Version Version
        {
            get => GetOptionValue(HttpResponseOptions.VersionOption);
            set => SetOptionValue(HttpResponseOptions.VersionOption, value);
        }

        /// <summary>
        /// 头部使用的协议方案字符串（"HTTP" 或 "HTTPS"）。
        /// </summary>
        public string Scheme
        {
            get => GetOptionValue(HttpResponseOptions.SchemeOption);
            set => SetOptionValue(HttpResponseOptions.SchemeOption, value);
        }

        /// <summary>
        /// 使用指定关联请求创建一个新的 <see cref="HttpResponseMessage"/> 实例。
        /// 该构造函数会在内部注册响应所需的选项项（头部、版本、状态码、说明短语等）。
        /// </summary>
        /// <param name="requestMessage">可选的关联请求消息（用于参考），可以为 <c>null</c>。</param>
        public HttpResponseMessage(HttpRequestMessage? requestMessage)
        {
            RegisterOption(HttpResponseOptions.BodyOption);
            RegisterOption(HttpResponseOptions.VersionOption);
            RegisterOption(HttpResponseOptions.HeadersOption);
            RegisterOption(HttpResponseOptions.StatusCodeOption);
            RegisterOption(HttpResponseOptions.ReasonPhraseOption);
            RegisterOption(HttpResponseOptions.RequestMessageOption, requestMessage);
            RegisterOption(HttpResponseOptions.SchemeOption, requestMessage?.Scheme ?? Uri.UriSchemeHttp);
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
        /// <param name="encoding">文本编码，不能为空。</param>
        /// <param name="contentType">可选 Content-Type。</param>
        public void SetContent(string text, Encoding encoding, string? contentType = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            WriteToBody(text, encoding);
            Headers.SetOptionValue(HttpHeaderOptions.ContentTypeOption, string.IsNullOrEmpty(contentType) ? $"text/plain; charset={encoding.WebName}" : contentType);
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
            Headers.SetOptionValue(HttpHeaderOptions.ContentTypeOption, contentType);
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
        /// 获取响应首部字符串（状态行 + 头部，不包含主体）。
        /// 使用系统默认编码（ANSI code page）对文本部分进行编码。
        /// </summary>
        /// <returns>仅包含状态行与头部的字符串，已以 CRLF 结束。</returns>
        public string GetResponseString()
        {
            return GetResponseString(Encoding.GetEncoding(0));
        }

        /// <summary>
        /// 构造响应首部字符串（状态行 + 头部，不包含主体）的文本表示，使用指定编码时可用于调试或日志记录。
        /// </summary>
        /// <param name="encoding">用于编码文本的字符编码，不能为空。</param>
        /// <returns>仅包含状态行与头部的字符串，已以 CRLF 结束。</returns>
        public string GetResponseString(Encoding encoding)
        {
            ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
            var sb = new StringBuilder();

            // 状态行: (HTTP or HTTPS)/{major}.{minor} {statusCode} {reason}\r\n
            var schemeUpper = (Scheme ?? Uri.UriSchemeHttp).ToUpperInvariant();
            sb.Append(schemeUpper);
            sb.Append(HttpConstants.Slash);
            sb.Append(Version.Major);
            sb.Append(HttpConstants.Dot);
            sb.Append(Version.Minor);
            sb.Append(HttpConstants.Space);
            sb.Append((int)StatusCode);
            sb.Append(HttpConstants.Space);
            sb.Append(ReasonPhrase ?? string.Empty);
            sb.Append(HttpConstants.NextLine);

            // 补齐响应头（例如 Content-Length, Date 等）
            Headers.EnsureResponseHeaders(Body);

            // 写入头部与分隔行
            Headers.BuildHeaderBlock(sb, combineValues: false);
            sb.Append(HttpConstants.NextLine);

            return sb.ToString();
        }

        /// <summary>
        /// 获取包含完整响应（首行 + 头部 + 可选 Body）的序列缓冲，文本部分使用 ASCII 编码。
        /// </summary>
        /// <returns>包含完整响应数据的 <see cref="SequenceBuffer{byte}"/>（调用方负责释放）。</returns>
        public SequenceBuffer<byte> GetResponseBuffer() => GetResponseBuffer(Encoding.ASCII);

        /// <summary>
        /// 获取包含完整响应（首行 + 头部 + 可选 Body）的序列缓冲，文本部分使用指定编码。
        /// </summary>
        /// <param name="encoding">用于编码状态行与头部的字符编码，不能为空。</param>
        /// <returns>包含完整响应数据的 <see cref="SequenceBuffer{byte}"/>（调用方负责释放）。</returns>
        public SequenceBuffer<byte> GetResponseBuffer(Encoding encoding)
        {
            var buffer = SequenceBuffer<byte>.GetBuffer();
            WriteToBuffer(buffer, encoding);
            return buffer;
        }

        /// <summary>
        /// 将完整响应（首行 + 头部 + 可选 Body）写入提供的序列缓冲，文本部分使用 ASCII 编码。
        /// </summary>
        /// <param name="buffer">目标 <see cref="SequenceBuffer{byte}"/>，不能为空。</param>
        public void WriteToBuffer(SequenceBuffer<byte> buffer) => WriteToBuffer(buffer, Encoding.ASCII);

        /// <summary>
        /// 将完整响应（首行 + 头部 + 可选 Body）写入提供的序列缓冲，文本部分使用指定编码。
        /// </summary>
        /// <param name="buffer">目标 <see cref="SequenceBuffer{byte}"/>，不能为空。</param>
        /// <param name="encoding">用于编码状态行与头部的字符编码，不能为空。</param>
        /// <exception cref="ArgumentNullException">当 buffer 或 encoding 为 null 时抛出。</exception>
        public void WriteToBuffer(SequenceBuffer<byte> buffer, Encoding encoding)
        {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
            ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));

            var schemeUpper = (Scheme ?? Uri.UriSchemeHttp).ToUpperInvariant();
            buffer.Write(schemeUpper, encoding);
            buffer.Write(HttpConstants.Slash, encoding);
            buffer.Write(Version.Major.ToString(), encoding);
            buffer.Write(HttpConstants.Dot, encoding);
            buffer.Write(Version.Minor.ToString(), encoding);
            buffer.Write(HttpConstants.Space, encoding);
            buffer.Write(((int)StatusCode).ToString(), encoding);
            buffer.Write(HttpConstants.Space, encoding);
            buffer.Write(ReasonPhrase ?? string.Empty, encoding);
            buffer.Write(HttpConstants.NextLine, encoding);

            Headers.EnsureResponseHeaders(Body);
            // 不合并头部值（与请求序列化保持一致）
            Headers.BuildHeaderBlock(buffer, false);
            buffer.Write(HttpConstants.NextLine, encoding);

            if (Body != null &&
                Body != AbstractBuffer<byte>.Empty &&
                Body.Committed > 0)
            {
                buffer.Append(Body);
            }
        }
    }
}