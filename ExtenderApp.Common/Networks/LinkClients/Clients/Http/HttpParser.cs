using System.Net;
using System.Text;
using System.Threading;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using HttpMethod = ExtenderApp.Data.HttpMethod;
using HttpRequestMessage = ExtenderApp.Data.HttpRequestMessage;
using HttpResponseMessage = ExtenderApp.Data.HttpResponseMessage;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 简单的 HTTP 请求/响应解析器。
    /// - 以 ASCII/ISO-8859-1 解码首行与头部（HTTP 头通常为 ASCII）。
    /// - 仅支持基于 Content-Length 的正文提取，不支持 chunked/分块编码。
    /// - 方法为 TryParse 风格：数据不足时返回 false，调用方可等待更多字节再重试。
    /// </summary>
    internal class HttpParser : DisposableObject, IHttpParser
    {
        private const int DefaultHeaderSize = 4 * 1024;
        private static readonly byte[] HeaderTerminator = { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
        private ByteBlock block;
        private HttpResponseMessage? response;
        private int headerBlockLen;
        private int contentLength;

        private HttpRequestMessage? request;

        public HttpParser()
        {
            block = new ByteBlock(DefaultHeaderSize);
        }

        /// <summary>
        /// 尝试从字节切片解析 HTTP 请求。
        /// 将接收到的 buffer 写入内部接收缓冲 block 并尽可能基于 Span 解析。
        /// 成功解析时：返回 true，输出完整消息并自动从内部缓冲消费已解析字节。
        /// 若数据不足则返回 false（不会消费内部缓冲），上层应等待更多字节并重试。
        /// </summary>
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

            return true;
        }

        /// <summary>
        /// 尝试从字节切片解析 HTTP 响应。
        /// 与请求解析类似：查找头部终止符并根据 Content-Length 提取 body。
        /// </summary>
        public bool TryParseResponse(ReadOnlySpan<byte> buffer, out HttpResponseMessage? message, out int bytesConsumed, Encoding? encoding = null)
        {
            message = response;
            bytesConsumed = 0;

            // 把新的字节写入内部缓存（复用 block）
            block.Write(buffer);

            if (response is null)
            {
                // 解析头部
                if (!TryParseResponseHeader(encoding, out message))
                {
                    return false; // 头部未完整到达
                }
                response = message;
                block.ReadAdvance(headerBlockLen);
            }

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
            return true;
        }

        private bool TryParseResponseHeader(Encoding? encoding, out HttpResponseMessage message)
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
                    // header 行：Name: value
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

            message = new HttpResponseMessage
            {
                Headers = headers
            };

            // 版本
            if (parts[0].StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase))
            {
                var ver = parts[0].Substring(5);
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
                    // header 行：Name: value
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

        protected override void Dispose(bool disposing)
        {
            block.Dispose();
        }
    }
}