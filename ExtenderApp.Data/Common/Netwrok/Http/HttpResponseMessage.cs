using System.Net;
using System.Text;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示 HTTP 响应消息（状态行 + 头部 + 可选主体）。
    /// 使用完毕请调用 Dispose 释放可能占用的 ByteBlock。
    /// </summary>
    public class HttpResponseMessage : DisposableObject
    {
        /// <summary>
        /// 与该响应关联的请求消息（若有）。仅供参考，可能为 <c>null</c>。
        /// </summary>
        public HttpRequestMessage RequestMessage { get; private set; }

        /// <summary>
        /// 响应的状态码（如 200、404 等）。
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// 状态短语（Reason-Phrase），例如 "OK" 或 "Not Found"。序列化时会写入状态行。
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// HTTP 协议版本（例如 HTTP/1.1）。默认值为 <see cref="HttpVersion.Version11"/>。
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// 响应头集合（不区分大小写的键名比较）。
        /// </summary>
        public HttpHeader Headers { get; set; }

        /// <summary>
        /// 响应主体的字节块。注意：该字段持有资源，使用完毕请调用 <see cref="Dispose"/> 回收底层缓冲。
        /// </summary>
        public ByteBlock Body { get; private set; }

        public HttpResponseMessage(HttpRequestMessage message)
        {
            Version = HttpVersion.Version11;
            ReasonPhrase = string.Empty;
            RequestMessage = message;
            Headers = new ();
        }

        /// <summary>
        /// 设置文本内容（会设置/覆盖 Content-Type 与 Content-WrittenCount 头）。
        /// </summary>
        /// <param name="text">文本内容。</param>
        /// <param name="encoding">可选编码（默认 UTF-8）。</param>
        /// <param name="contentType">可选 Content-Type（不含 charset 时会使用编码的 charset）。</param>
        public void SetContent(string text, Encoding? encoding = null, string? contentType = null)
        {
            encoding ??= Encoding.UTF8;
            WriteToBody(text, encoding);
            // 修正错误的默认 Content-Type（之前为 "bodyText/plain" 的拼写错误）
            Headers.SetValue(HttpHeaders.ContentType, string.IsNullOrEmpty(contentType) ? $"text/plain; charset={encoding.WebName}" : contentType);
            Headers.SetValue(HttpHeaders.ContentLength, Body.WrittenCount.ToString());
        }

        /// <summary>
        /// 设置二进制内容（会设置/覆盖 Content-Type 与 Content-WrittenCount 头）。
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
            Headers.SetValue(HttpHeaders.ContentType, contentType ?? "application/octet-stream");
            Headers.SetValue(HttpHeaders.ContentLength, span.Length.ToString());
        }

        /// <summary>
        /// 设置二进制内容（会设置/覆盖 Content-Type 与 Content-WrittenCount 头）。
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
            Body = block;

            Headers!.SetValue(HttpHeaders.ContentType, contentType ?? "application/octet-stream");
            Headers!.SetValue(HttpHeaders.ContentLength, block.WrittenCount.ToString());
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
        /// 序列化响应：构造状态行 + headers + CRLF + body，返回可直接写入网络流的 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="encoding">
        /// 将状态行和头部编码为字节时使用的编码。若为 <c>null</c>，默认使用 <see cref="Encoding.ASCII"/>（HTTP 头通常使用 ASCII 或 ISO-8859-1）。
        /// </param>
        /// <returns>
        /// 包含序列化后数据的 <see cref="ByteBuffer"/>（引用类型为 ref struct，调用方负责在合适时机调用其 <see cref="ByteBuffer.Dispose"/> 回收资源）。
        /// </returns>
        public ByteBuffer ToBuffer(Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            var sb = new StringBuilder();

            // 状态行: HTTP/{major}.{minor} {statusCode} {reason}\r\n
            sb.Append(RequestMessage.Scheme);
            sb.Append(HttpHeaders.SlashChar);
            sb.Append(HttpHeaders.SlashChar);
            sb.Append(Version.Major);
            sb.Append(HttpHeaders.DotChar);
            sb.Append(Version.Minor);
            sb.Append(HttpHeaders.SpaceChar);
            sb.Append(((int)StatusCode));
            sb.Append(HttpHeaders.SpaceChar);
            sb.Append(ReasonPhrase ?? string.Empty);
            sb.Append(HttpHeaders.NextLine);

            // Host 不适用于响应，这里直接写 headers
            Headers.BuildHeaderBlock(sb, combineValues: false);
            sb.Append(HttpHeaders.NextLine);

            sb.BuildByteBuffer(Body, out ByteBuffer buffer, encoding);

            return buffer;
        }

        protected override void Dispose(bool disposing)
        {
            Body.Dispose();
        }
    }
}