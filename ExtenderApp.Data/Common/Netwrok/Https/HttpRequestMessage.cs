using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;

namespace ExtenderApp.Data.Common.Netwrok.Https
{
    internal class HttpRequestMessage : DisposableObject
    {
        public HttpMethod Method { get; set; }
        public Uri? RequestUri { get; set; }
        public Version Version { get; set; } = new(1, 1);
        public Dictionary<string, List<string>> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>请求体原始字节；可能为 null 表示无体。</summary>
        public byte[]? Body { get; set; }

        public long ContentLength => Body?.LongLength ?? 0;

        public HttpRequestMessage() : this(HttpMethod.Get, string.Empty)
        {

        }

        public HttpRequestMessage(HttpMethod method, [StringSyntax("Uri")] string? requestUri) : this(method, string.IsNullOrEmpty(requestUri) ? null : new Uri(requestUri))
        {
        }
        public HttpRequestMessage(HttpMethod method, Uri? requestUri)
        {
            Method = HttpMethod.Get;
        }

        public void AddHeader(string name, string value)
        {
            if (!Headers.TryGetValue(name, out var list))
            {
                list = new List<string>();
                Headers[name] = list;
            }
            list.Add(value);
        }

        public bool TryGetHeader(string name, out string[] values)
        {
            if (Headers.TryGetValue(name, out var list))
            {
                values = list.ToArray();
                return true;
            }
            values = Array.Empty<string>();
            return false;
        }

        /// <summary>
        /// 将请求序列化为用于发送的字节（start-line + headers + CRLF + body）。
        /// 只做最小格式化（不处理分块编码等高级特性）。
        /// </summary>
        public byte[] ToBytes()
        {
            if (RequestUri is null)
                throw new InvalidOperationException("RequestUri not set.");

            string pathAndQuery = string.IsNullOrEmpty(RequestUri.PathAndQuery) ? "/" : RequestUri.PathAndQuery;
            var sb = new StringBuilder();
            sb.Append($"{Method} {pathAndQuery} HTTP/{Version.Major}.{Version.Minor}\r\n");

            // Host header
            if (!Headers.ContainsKey("Host"))
            {
                string host = RequestUri.IsDefaultPort ? RequestUri.Host : $"{RequestUri.Host}:{RequestUri.Port}";
                sb.Append($"Host: {host}\r\n");
            }

            // Ensure Content-Length if body present and not specified
            if (Body is not null && Body.Length > 0 && !Headers.ContainsKey("Content-Length"))
            {
                sb.Append($"Content-Length: {Body.Length}\r\n");
            }

            foreach (var kv in Headers)
            {
                foreach (var v in kv.Value)
                    sb.Append($"{kv.Key}: {v}\r\n");
            }

            sb.Append("\r\n");
            var headerBytes = Encoding.ASCII.GetBytes(sb.ToString());

            if (Body is null || Body.Length == 0)
                return headerBytes;

            var result = new byte[headerBytes.Length + Body.Length];
            Buffer.BlockCopy(headerBytes, 0, result, 0, headerBytes.Length);
            Buffer.BlockCopy(Body, 0, result, headerBytes.Length, Body.Length);
            return result;
        }

        public void SetContent(string text, Encoding? encoding = null, string? contentType = "text/plain; charset=utf-8")
        {
            encoding ??= Encoding.UTF8;
            Body = encoding.GetBytes(text);
            Headers["Content-Type"] = new List<string> { contentType };
            Headers["Content-Length"] = new List<string> { Body.Length.ToString() };
        }

        public void SetContent(byte[] bytes, string? contentType = "application/octet-stream")
        {
            Body = bytes;
            Headers["Content-Type"] = new List<string> { contentType };
            Headers["Content-Length"] = new List<string> { Body.Length.ToString() };
        }
    }
}
