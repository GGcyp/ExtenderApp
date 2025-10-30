

namespace ExtenderApp.Data
{
    /// <summary>
    /// 序列化前统一准备/补齐请求与响应头的辅助方法。
    /// 把有关 Host / Content-Length / 可选默认 Content-Type 的规则集中到这里，避免分散在各处。
    /// </summary>
    public static class HttpHeaderHelpers
    {
        /// <summary>
        /// 在序列化请求之前确保头部已被补齐：
        /// - 如果没有 Host 则从 requestUri 补 Host；
        /// - 如果 body 非空且没有 Content-Length 则补上 Content-Length；
        /// - 如果 body 非空且没有 Content-Type 且提供了 defaultContentType，则设置默认 Content-Type。
        /// </summary>
        /// <param name="headers">消息头集合</param>
        /// <param name="requestUri">请求 Uri（可能为 null）</param>
        /// <param name="body">消息体</param>
        /// <param name="defaultContentType">可选默认 Content-Type（若为 null 则不设置）</param>
        public static void EnsureRequestHeaders(this HttpHeader headers, Uri? requestUri, in ByteBlock body, string? defaultContentType = null)
        {
            if (headers is null) throw new ArgumentNullException(nameof(headers));

            // Host：只在请求有 RequestUri 且未设置 Host 时补齐
            if (requestUri != null && !headers.ContainsHeader(HttpHeaders.Host))
            {
                var host = requestUri.Host;
                if (!requestUri.IsDefaultPort)
                    host += ":" + requestUri.Port;
                headers.SetValue(HttpHeaders.Host, host);
            }

            // Content-Length：当有 body 且未设置时补齐
            if (body.Length > 0 && !headers.ContainsHeader(HttpHeaders.ContentLength))
            {
                headers.SetValue(HttpHeaders.ContentLength, body.Length.ToString());
            }

            // 默认 Content-Type（仅在 body 存在且未显式设置时生效）
            if (!string.IsNullOrEmpty(defaultContentType) && body.Length > 0 && !headers.ContainsHeader(HttpHeaders.ContentType))
            {
                headers.SetValue(HttpHeaders.ContentType, defaultContentType);
            }
        }

        /// <summary>
        /// 在序列化响应之前确保头部已被补齐（示例：Content-Length、Date 等）。
        /// </summary>
        /// <param name="headers">响应头集合</param>
        /// <param name="body">响应体</param>
        /// <param name="defaultContentType">可选默认 Content-Type</param>
        public static void EnsureResponseHeaders(this HttpHeader headers, in ByteBlock body, string? defaultContentType = null)
        {
            if (headers is null) throw new ArgumentNullException(nameof(headers));

            if (body.Length > 0 && !headers.ContainsHeader(HttpHeaders.ContentLength))
                headers.SetValue(HttpHeaders.ContentLength, body.Length.ToString());

            if (!string.IsNullOrEmpty(defaultContentType) && body.Length > 0 && !headers.ContainsHeader(HttpHeaders.ContentType))
                headers.SetValue(HttpHeaders.ContentType, defaultContentType);

            if (!headers.ContainsHeader(HttpHeaders.Date))
                headers.SetValue(HttpHeaders.Date, DateTime.UtcNow.ToString("r"));
        }
    }
}