using System.Net;
using System.Text;
using ExtenderApp.Data;
using ExtenderApp.Abstract;
using HttpMethod = ExtenderApp.Data.HttpMethod;
using HttpRequestMessage = ExtenderApp.Data.HttpRequestMessage;
using HttpResponseMessage = ExtenderApp.Data.HttpResponseMessage;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 简单的 HTTP 请求/响应解析器。
    /// - 以 ASCII/ISO-8859-1 解码首行与头部（HTTP 头通常为 ASCII）。
    /// - 仅支持基于 Content-Length 的正文提取，不支持 chunked/分块编码。
    /// - 方法为 TryParse 风格：数据不足时返回 false，调用方可等待更多字节再重试。
    /// </summary>
    internal class HttpParser : DisposableObject, IHttpParser
    {
        /// <summary>
        /// 默认分配给内部头部缓冲区的初始大小（字节）。用于优化常见场景下的头部读取，避免频繁扩容。
        /// </summary>
        private const int DefaultHeaderSize = 4 * 1024;

        /// <summary>
        /// HTTP 头部终止标记 "\r\n\r\n" 的字节表示，用于在接收缓冲中查找头部结束位置。
        /// </summary>
        private static readonly byte[] HeaderTerminator = { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };

        /// <summary>
        /// 内部接收缓冲（ByteBlock），用于累积来自网络的字节并在缓冲中进行解析。
        /// 注意：HttpParser 非线程安全；该缓冲仅在单一解析上下文中使用并在 Dispose 时释放。
        /// </summary>
        private ByteBlock block;

        /// <summary>
        /// 当前已解析出的头部长度（包含 HeaderTerminator 的长度），用于计算已消费字节与 body 偏移。
        /// </summary>
        private int headerBlockLen;

        /// <summary>
        /// 根据响应头中的 Content-Length 解析得到的主体长度（字节）。
        /// 若未提供 Content-Length，则为 0（当前实现不支持 chunked 编码）。
        /// </summary>
        private int contentLength;

        /// <summary>
        /// 用于识别响应状态行中以 "HTTP/" 开头的前缀（比较时使用 StartsWith）。
        /// </summary>
        private static string HttpScheme = $"{Uri.UriSchemeHttp}/";

        /// <summary>
        /// 用于识别响应状态行中以 "HTTPS/" 开头的前缀（如果需要兼容不同前缀形式）。
        /// </summary>
        private static string HttpsScheme = $"{Uri.UriSchemeHttps}/";

        /// <summary>
        /// 在解析请求时临时保存对应的 HttpRequestMessage（若解析响应时需要参考原始请求）。
        /// 解析完成后会重置为 null。
        /// </summary>
        private HttpRequestMessage? request;

        /// <summary>
        /// 正在构造的 HttpResponseMessage 实例（当解析响应头后创建并在解析 body 后完成）。
        /// 在解析完成并返回给调用方后会被置为 null 以准备下一次解析。
        /// </summary>
        private HttpResponseMessage? response;

        public HttpParser()
        {
            block = new ByteBlock(DefaultHeaderSize);
        }

        public bool TryParseRequest(ReadOnlySpan<byte> buffer, out HttpRequestMessage? message, out int bytesConsumed, Encoding? encoding = null)
        {
            message = default;
            bytesConsumed = 0;

            // 写入接收数据到内部缓冲（复用单个 ByteBlock）
            block.Write(buffer);

            if (request is null)
            {
                // 解析头部
                if (!TryParseRequestHeader(encoding, out message))
                {
                    return false; // 头部未完整到达
                }
                request = message;
                block.ReadAdvance(headerBlockLen);
            }

            if (contentLength == 0)
                return true;

            bytesConsumed = headerBlockLen + contentLength;
            ReadOnlySpan<byte> unread = block.UnreadSpan;
            int stillNeedLen = contentLength - block.Remaining;
            // 检查 body 是否完整
            if (stillNeedLen > 0 && stillNeedLen > block.WritableBytes)
            {
                block.Ensure(stillNeedLen);
                return false; // body 未到齐
            }

            // 复制 body 到新的 ByteBlock（现有类型 API 要求 copy）
            if (block.Remaining < contentLength)
            {
                return false;
            }

            ReadOnlySpan<byte> bodySpan = unread.Slice(0, contentLength);
            message!.SetContent(block: new ByteBlock(bodySpan));

            // 消费内部缓冲并压缩（移除已解析数据）
            block.ReadAdvance(bytesConsumed);
            block.Dispose();
            block = new(DefaultHeaderSize);
            request = null;
            headerBlockLen = 0;
            contentLength = 0;
            return true;
        }

        public bool TryParseResponse(ReadOnlySpan<byte> buffer, HttpRequestMessage requestMessage, out HttpResponseMessage? message, out int bytesConsumed, Encoding? encoding = null)
        {
            message = response;
            bytesConsumed = 0;

            // 把新的字节写入内部缓存（复用 block）
            block.Write(buffer);

            if (response is null)
            {
                // 解析头部
                if (!TryParseResponseHeader(encoding, requestMessage, out message))
                {
                    return false; // 头部未完整到达
                }
                response = message;
                block.ReadAdvance(headerBlockLen);
            }

            if (contentLength == 0)
                return true;

            bytesConsumed = headerBlockLen + contentLength;
            ReadOnlySpan<byte> unread = block.UnreadSpan;
            int stillNeedLen = contentLength - block.Remaining;
            // 检查 body 是否完整
            if (stillNeedLen > 0 && stillNeedLen > block.WritableBytes)
            {
                block.Ensure(stillNeedLen);
                return false; // body 未到齐
            }

            if (block.Remaining < contentLength)
            {
                return false;
            }

            ReadOnlySpan<byte> bodySpan = unread.Slice(0, contentLength);
            response.SetContent(new ByteBlock(bodySpan));

            // 消费内部缓冲并重置（与请求解析一致的行为）
            block.ReadAdvance(bytesConsumed);
            block.Dispose();
            block = new(DefaultHeaderSize);
            response = null;
            headerBlockLen = 0;
            contentLength = 0;
            return true;
        }

        private bool TryParseResponseHeader(Encoding? encoding, HttpRequestMessage requestMessage, out HttpResponseMessage message)
        {
            message = null!;
            ReadOnlySpan<byte> unread = block.UnreadSpan;
            // 查找头部结束标记 "\r\n\r\n"
            int headerEnd = unread.IndexOf(HeaderTerminator);
            if (headerEnd < 0)
                return false; // 头部还未完整到达

            headerBlockLen = headerEnd + HeaderTerminator.Length;
            encoding ??= Encoding.ASCII;

            // 逐行解析头部（与请求解析保持一致的风格）
            int pos = 0;
            var remaining = unread.Slice(pos, headerBlockLen - pos);
            string statusLine = GetLineString(remaining, encoding, out var nLength);

            HttpHeader headers = new();

            while (pos < headerBlockLen)
            {
                remaining = unread.Slice(pos, headerBlockLen - pos);
                string line = GetLineString(remaining, encoding, out nLength);

                if (!string.IsNullOrEmpty(line))
                {
                    // header 行：Name: Value
                    int idx = line.IndexOf(':');
                    if (idx > 0)
                    {
                        string name = line.Substring(0, idx).Trim();
                        string value = line.Substring(idx + 1).Trim();
                        headers.SetValue(name, value);
                    }
                }

                // 前进到下一行（nLength 为不含 LF 的长度）
                pos += nLength + 1;
            }

            if (string.IsNullOrEmpty(statusLine))
                throw new InvalidOperationException("无法解析 HTTP 响应状态行");

            // 解析 status-line: HTTP/VERSION SP STATUSCODE SP REASON-PHRASE
            var parts = statusLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                throw new InvalidOperationException("无法解析 HTTP 响应状态行");

            message = new HttpResponseMessage(requestMessage)
            {
                Headers = headers
            };

            // 版本
            if (parts[0].StartsWith(HttpScheme, StringComparison.OrdinalIgnoreCase))
            {
                var ver = parts[0].Substring(5);
                var verParts = ver.Split('.', 2);
                if (verParts.Length == 2 && int.TryParse(verParts[0], out int major) && int.TryParse(verParts[1], out int minor))
                {
                    message.Version = new Version(major, minor);
                }
            }
            else if (parts[0].StartsWith(HttpsScheme, StringComparison.OrdinalIgnoreCase))
            {
                var ver = parts[0].Substring(6);
                var verParts = ver.Split('.', 2);
                if (verParts.Length == 2 && int.TryParse(verParts[0], out int major) && int.TryParse(verParts[1], out int minor))
                {
                    message.Version = new Version(major, minor);
                }
            }

            // 状态码
            if (int.TryParse(parts[1], out int statusCode))
                message.StatusCode = (HttpStatusCode)statusCode;

            // Reason-Phrase（可能包含空格）
            message.ReasonPhrase = parts.Length >= 3 ? parts[2] : string.Empty;

            // Body（基于 Content-Length）
            if (message.Headers.TryGetValues(HttpHeaders.ContentLength, out var maybeLen))
            {
                if (int.TryParse(maybeLen[0] ?? string.Empty, out var parsedLen))
                    contentLength = parsedLen;
            }

            return true;
        }

        private bool TryParseRequestHeader(Encoding? encoding, out HttpRequestMessage message)
        {
            message = null!;
            ReadOnlySpan<byte> unread = block.UnreadSpan;
            int headerEnd = unread.IndexOf(HeaderTerminator);
            if (headerEnd < 0)
                return false; // 头部还未完整到达

            headerBlockLen = headerEnd + HeaderTerminator.Length;
            encoding ??= Encoding.ASCII;

            // 逐行解析头部（避免把整个头部转为单个 string 再 Split）
            int pos = 0;
            var remaining = unread.Slice(pos, headerBlockLen - pos);
            string requestLine = GetLineString(remaining, encoding, out var nLength);
            message = new HttpRequestMessage();
            var headers = message.Headers;

            while (pos < headerBlockLen)
            {
                remaining = unread.Slice(pos, headerBlockLen - pos);
                string line = GetLineString(remaining, encoding, out nLength);

                if (!string.IsNullOrEmpty(line))
                {
                    // header 行：Name: Value
                    int idx = line.IndexOf(':');
                    if (idx > 0)
                    {
                        string name = line.Substring(0, idx).Trim();
                        string value = line.Substring(idx + 1).Trim();
                        headers.SetValue(name, value);
                    }
                }

                //前进到下一行
                pos += nLength + 1;
            }

            if (string.IsNullOrEmpty(requestLine))
                return false;

            // 解析 request-line: METHOD SP URI SP HTTP/VERSION
            var parts = requestLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return false;

            // 准备消息对象
            message.Method = new HttpMethod(parts[0]);
            if (Uri.TryCreate(parts[1], UriKind.RelativeOrAbsolute, out var uri))
                message.RequestUri = uri;

            if (parts[2].StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase))
            {
                var ver = parts[2].Substring(5);
                var verParts = ver.Split('.', 2);
                if (verParts.Length == 2 && int.TryParse(verParts[0], out int major) && int.TryParse(verParts[1], out int minor))
                    message.Version = new Version(major, minor);
            }

            // Content-Length
            if (message.Headers.TryGetValues(HttpHeaders.ContentLength, out var maybeLen))
            {
                if (int.TryParse(maybeLen[0] ?? string.Empty, out var parsedLen))
                    contentLength = parsedLen;
            }
            return true;
        }

        private string GetLineString(ReadOnlySpan<byte> span, Encoding encoding, out int nLength)
        {
            nLength = span.IndexOf((byte)'\n');
            if (nLength < 0) return string.Empty;//没有找到换行符

            // 计算不含 CRLF 的行长度
            int lineLen = nLength;
            if (lineLen > 0 && span[lineLen - 1] == (byte)'\r')
                lineLen--;

            // 取得行的 byte slice
            ReadOnlySpan<byte> lineSpan = span.Slice(0, lineLen);

            // 将行按 ASCII 解成 string（这里只对单行分配）
            return encoding.GetString(lineSpan);
        }

        protected override void DisposeManagedResources()
        {
            block.Dispose();
        }
    }
}