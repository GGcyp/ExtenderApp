using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    internal class HttpLinkClient : LinkClient<ITcpLinker>, IHttpLinkClient
    {
        public HttpLinkClient(ITcpLinker linker) : base(linker)
        {

        }

        /// <summary>
        /// 简单的异步 GET 请求，返回响应文本（按 UTF-8 解码）。
        /// 使用 Connection: close，读取直到远端关闭连接停止。
        /// </summary>
        public async ValueTask<string> GetAsync(string host, string path = "/", int port = 80, CancellationToken token = default)
        {
            await EnsureConnectedAsync(host, port, token).ConfigureAwait(false);

            string request = $"GET {path} HTTP/1.1\r\nHost: {host}\r\nAccept: */*\r\nConnection: close\r\n\r\n";
            var requestBytes = Encoding.ASCII.GetBytes(request);

            // 发送请求
            await Linker.SendAsync(requestBytes.AsMemory(), token).ConfigureAwait(false);

            // 读取响应直到远端关闭
            return await ReadAllResponseAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// 简单的异步 POST 请求，body 作为二进制负载，返回响应文本（按 UTF-8 解码）。
        /// 使用 Connection: close，读取直到远端关闭连接停止。
        /// </summary>
        public async ValueTask<string> PostAsync(string host, string path, ReadOnlyMemory<byte> body, string contentType = "application/octet-stream", int port = 80, CancellationToken token = default)
        {
            await EnsureConnectedAsync(host, port, token).ConfigureAwait(false);

            string reqHead = $"POST {path} HTTP/1.1\r\nHost: {host}\r\nContent-Type: {contentType}\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n";
            var headBytes = Encoding.ASCII.GetBytes(reqHead);

            // 发送头部
            await Linker.SendAsync(headBytes.AsMemory(), token).ConfigureAwait(false);

            // 发送正文（若有）
            if (!body.IsEmpty)
            {
                await Linker.SendAsync(body.ToArray().AsMemory(), token).ConfigureAwait(false);
            }

            // 读取响应直到远端关闭
            return await ReadAllResponseAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// 同步包装方法（仅在需要同步调用时使用）。
        /// </summary>
        public string Get(string host, string path = "/", int port = 80, CancellationToken token = default)
            => GetAsync(host, path, port, token).GetAwaiter().GetResult();

        public string Post(string host, string path, ReadOnlyMemory<byte> body, string contentType = "application/octet-stream", int port = 80, CancellationToken token = default)
            => PostAsync(host, path, body, contentType, port, token).GetAwaiter().GetResult();

        private async ValueTask EnsureConnectedAsync(string host, int port, CancellationToken token)
        {
            if (Linker.Connected)
                return;

            // 解析主机地址，优先 IPv4
            IPAddress[] addrs = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
            IPAddress? addr = null;
            foreach (var a in addrs)
            {
                if (a.AddressFamily == AddressFamily.InterNetwork)
                {
                    addr = a;
                    break;
                }
            }
            addr ??= addrs.Length > 0 ? addrs[0] : null;

            if (addr is null)
                throw new InvalidOperationException($"无法解析主机: {host}");

            var ep = new IPEndPoint(addr, port);
            await Linker.ConnectAsync(ep, token).ConfigureAwait(false);
        }

        private async ValueTask<string> ReadAllResponseAsync(CancellationToken token)
        {
            var sb = new StringBuilder();
            byte[] buffer = new byte[8192];

            while (true)
            {
                var res = await Linker.ReceiveAsync(buffer.AsMemory(), token).ConfigureAwait(false);

                // 读取结果：当远端关闭连接时，BytesTransferred 很可能为 0
                int read = res.BytesTransferred;
                if (read <= 0)
                    break;

                sb.Append(Encoding.UTF8.GetString(buffer, 0, read));
            }

            return sb.ToString();
        }
    }
}
