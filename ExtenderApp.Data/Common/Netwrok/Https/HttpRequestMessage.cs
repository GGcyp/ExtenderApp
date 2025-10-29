using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ExtenderApp.Data.Common.Netwrok
{
    internal class HttpRequestMessage : DisposableObject
    {
        private readonly Lazy<HttpParams> _paramsLazy;
        private byte[]? _body;

        public HttpMethod Method { get; set; }
        public Uri? RequestUri { get; set; }
        public Version Version { get; set; }
        public HttpHeader Headers { get; }

        public HttpParams Params => _paramsLazy.Value;

        public byte[]? Body => _body;

        public HttpRequestMessage() : this(HttpMethod.Get, string.Empty)
        {
            TcpClient tcpClient = new();
            tcpClient.Close();
        }

        public HttpRequestMessage(HttpMethod method, [StringSyntax("Uri")] string? requestUri) : this(method, string.IsNullOrEmpty(requestUri) ? null : new Uri(requestUri))
        {
        }
        public HttpRequestMessage(HttpMethod method, Uri? requestUri)
        {
            Method = method;
            RequestUri = requestUri;
            Headers = new ();
            _paramsLazy = new();
            Version = HttpVersion.Version11;
        }

        /// <summary>
        /// 设置文本内容（会设置 Content-Type 与 Content-Length 头）。
        /// </summary>
        public void SetContent(string text, Encoding? encoding = null, string? contentType = "text/plain; charset=utf-8")
        {
            encoding ??= Encoding.UTF8;
            _body = encoding.GetBytes(text ?? string.Empty);
            Headers.SetValue("Content-Type", contentType ?? "text/plain; charset=utf-8");
            Headers.SetValue("Content-Length", _body.Length.ToString());
        }

        /// <summary>
        /// 设置二进制内容（会设置 Content-Type 与 Content-Length 头）。
        /// </summary>
        public void SetContent(byte[] bytes, string? contentType = "application/octet-stream")
        {
            _body = bytes ?? Array.Empty<byte>();
            Headers.SetValue("Content-Type", contentType ?? "application/octet-stream");
            Headers.SetValue("Content-Length", _body.Length.ToString());
        }

        /// <summary>
        /// 将当前 Params 以 application/x-www-form-urlencoded 作为请求体设置（覆盖现有 Body）。
        /// </summary>
        public void SetFormContentFromParams(Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var form = Params.ToFormUrlEncodedString();
            _body = encoding.GetBytes(form);
            Headers.SetValue("Content-Type", "application/x-www-form-urlencoded; charset=" + (encoding.WebName));
            Headers.SetValue("Content-Length", _body.Length.ToString());
        }

        /// <summary>
        /// 序列化请求（start-line + headers + CRLF + body）为字节数组，可直接发送到网络流。
        /// - 若 Method 为 GET 且 Params 不为空，会把 Params 作为查询字符串追加到 RequestUri。
        /// - 若存在 Body，会在头部加上 Content-Length（若未显式设置）。
        /// </summary>
        public byte[] ToBytes()
        {
            if (RequestUri is null)
                throw new InvalidOperationException("RequestUri not set.");

            // path + query
            string pathAndQuery = string.IsNullOrEmpty(RequestUri.PathAndQuery) ? "/" : RequestUri.PathAndQuery;

            // 如果是 GET 请求且有 Params，则把 Params 追加为查询字符串
            if (Method.Equals(HttpMethod.Get) && Params.Count > 0)
            {
                var qs = Params.ToQueryString();
                if (!string.IsNullOrEmpty(qs))
                {
                    if (string.IsNullOrEmpty(RequestUri.Query))
                        pathAndQuery = (RequestUri.AbsolutePath ?? "/") + "?" + qs;
                    else
                        pathAndQuery = (RequestUri.AbsolutePath ?? "/") + RequestUri.Query + "&" + qs;
                }
            }

            var sb = new StringBuilder();
            sb.Append($"{Method} {pathAndQuery} HTTP/{Version.Major}.{Version.Minor}\r\n");

            // Host header 如果不存在则手动写入一行 Host
            if (!Headers.ContainsHeader("Host"))
            {
                string host = RequestUri.IsDefaultPort ? RequestUri.Host : $"{RequestUri.Host}:{RequestUri.Port}";
                sb.Append($"Host: {host}\r\n");
            }

            // 如果有 body，则确保 Content-Length 存在（但不覆盖用户显式设置）
            if (_body is not null && _body.Length > 0 && !Headers.ContainsHeader("Content-Length"))
            {
                sb.Append($"Content-Length: {_body.Length}\r\n");
            }

            // 将其它头写入（Headers.BuildHeaderBlock 不会在末尾追加空行）
            Headers.BuildHeaderBlock(sb, combineValues: false);

            // 头部与主体之间的空行
            sb.Append("\r\n");

            var headerBytes = Encoding.ASCII.GetBytes(sb.ToString());

            if (_body is null || _body.Length == 0)
                return headerBytes;

            var result = new byte[headerBytes.Length + _body.Length];
            Buffer.BlockCopy(headerBytes, 0, result, 0, headerBytes.Length);
            Buffer.BlockCopy(_body, 0, result, headerBytes.Length, _body.Length);
            return result;
        }
    }
}
