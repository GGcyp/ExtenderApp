using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks.Sources;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;
using HttpRequestMessage = ExtenderApp.Contracts.HttpRequestMessage;
using HttpResponseMessage = ExtenderApp.Contracts.HttpResponseMessage;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 简易的 HTTP 客户端，基于底层 ITcpLinker 封装为可发送请求并接收响应的客户端。
    /// - 支持明文和 TLS（通过 SslStream）请求发送/接收。
    /// - 使用 HttpParser 对响应进行增量解析（基于 Content-Capacity）。
    /// - 对外以 ValueTask&lt;HttpResponseMessage&gt; 形式返回解析结果。
    /// </summary>
    internal class HttpLinkClient : LinkClient<ITcpLinker>, IHttpLinkClient, IValueTaskSource<HttpResponseMessage>, IDisposable
    {
        /// <summary>
        /// 发送或请求明文 HTTP 请求时使用的 TcpLinkerStream。
        /// </summary>
        private readonly TcpLinkerStream _tcpLinkerStream;

        /// <summary>
        /// 发送或请求 HTTPS 请求时使用的 SslStream。
        /// </summary>
        private readonly SslStream _sslStream;

        /// <summary>
        /// Http 响应解析器。
        /// </summary>
        private IHttpParser Parser;

        /// <summary>
        /// 异步结果的手动重置 ValueTask 源。
        /// </summary>
        private ManualResetValueTaskSourceCore<HttpResponseMessage> vts;

        /// <summary>
        /// 默认的 SSL 客户端身份验证选项。
        /// </summary>
        public SslClientAuthenticationOptions AuthenticationOptions;

        /// <summary>
        /// 当前的接收循环任务（若有）。
        /// </summary>
        private Task? receiveTask;

        /// <summary>
        /// 当前接收循环的取消令牌源。
        /// </summary>
        private CancellationTokenSource? receiveCts;

        private readonly object _receiveLock = new();

        /// <summary>
        /// 当前 vts 的版本号，用于 ValueTask 源标识。
        /// </summary>
        public short Version => vts.Version;

        /// <summary>
        /// 当前正在处理的流（可能是明文流或 SSL 流）。
        /// </summary>
        private Stream? currentStream;

        /// <summary>
        /// 当前正在处理的请求消息（用于解析响应时参考请求头等信息）。
        /// </summary>
        private HttpRequestMessage? currentRequest;

        /// <summary>
        /// 使用指定的 ITcpLinker 创建 HttpLinkClient 实例。
        /// </summary>
        /// <param name="linker">底层 TCP 链接器，用于建立连接并收发原始字节。</param>
        public HttpLinkClient(ITcpLinker linker, IHttpParser? httpParser = null) : base(linker)
        {
            Parser = httpParser ?? new HttpParser();
            vts = new ManualResetValueTaskSourceCore<HttpResponseMessage>();
            vts.RunContinuationsAsynchronously = true;
            _tcpLinkerStream = linker.GetStream();
            _sslStream = new(_tcpLinkerStream);
            AuthenticationOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                CertificateRevocationCheckMode = X509RevocationMode.Online,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            };
        }

        //public async ValueTask SetHttpParser(IHttpParser httpParser, CancellationToken token = default)
        //{
        //    ArgumentNullException.ThrowIfNull(httpParser, nameof(httpParser));

        //    if (receiveTask is not null)
        //        await receiveTask.WaitAsync(token);

        //    Parser = httpParser;
        //}

        //public async ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, SslClientAuthenticationOptions? options = null, CancellationToken token = default)
        //{
        //    ArgumentNullException.ThrowIfNull(request);
        //    ArgumentNullException.ThrowIfNull(request.RequestUri);
        //    receiveCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        //    token = receiveCts.Token;

        //    int redirectCount = 0;
        //    Uri currentUri = request.RequestUri;
        //    int maxRedirects = request.MaxRedirects;

        //    while (true)
        //    {
        //        await ConnectAsync(request.RequestUri, token).ConfigureAwait(false);

        //        currentRequest = request;
        //        currentStream = request.IsHttps ? _sslStream : _tcpLinkerStream;
        //        if (request.IsHttps)
        //        {
        //            options = options ?? AuthenticationOptions;
        //            options.TargetHost = request.RequestUri!.Host;
        //            await _sslStream.AuthenticateAsClientAsync(options, token).ConfigureAwait(false);
        //            await SendAuthenticateRequestMessage(request.ToBuffer(), token).ConfigureAwait(false);
        //        }
        //        else
        //        {
        //            await SendRequestMessage(request.ToBuffer(), token).ConfigureAwait(false);
        //        }

        //        // 准备等待源并启动接收任务（若尚未启动）
        //        vts.Reset();
        //        EnsureReceiveTaskRunning(token);
        //        var response = await new ValueTask<HttpResponseMessage>(this, vts.Version).ConfigureAwait(false);

        //        // 如果不是重定向，直接返回
        //        if (response.StatusCode != HttpStatusCode.Redirect || redirectCount >= maxRedirects)
        //            return response;

        //        // 获取 Location 头（需检查是否存在并能解析）
        //        string? location = string.Empty;
        //        if (!response.Headers.TryGetValues(HttpHeaders.Location, out var locs))
        //        {
        //            location = locs.FirstOrDefault();
        //            if (string.IsNullOrEmpty(location))
        //                return response;
        //        }

        //        Uri newUri = new Uri(currentUri, location); // 解析相对/绝对
        //        redirectCount++;

        //        // 必要时调整请求（例如 302/303 将方法改为 GET 并清除 Body）
        //        if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.RedirectMethod)
        //        {
        //            request.Method = Contracts.HttpMethod.Get;
        //            request.Body.Dispose();
        //        }

        //        if (!string.Equals(currentUri.Host, newUri.Host, StringComparison.OrdinalIgnoreCase) || currentUri.Port != newUri.Port)
        //        {
        //            await Linker.DisconnectAsync().ConfigureAwait(false);
        //        }

        //        // 准备下次循环
        //        currentUri = newUri;
        //        request.RequestUri = newUri;
        //        request.Headers.SetValue(HttpHeaders.Host, newUri.Authority);
        //    }
        //}

        ///// <summary>
        ///// 连接到指定请求 URI 的主机与端口。
        ///// </summary>
        ///// <param name="requestUri">目标 URI，不能为空。</param>
        ///// <param name="token">取消令牌。</param>
        ///// <returns>表示连接操作的 ValueTask。</returns>
        //private ValueTask ConnectAsync(Uri? requestUri, CancellationToken token)
        //{
        //    ArgumentNullException.ThrowIfNull(requestUri);
        //    int port = requestUri.Port > 0 ? requestUri.Port : 80;
        //    return Linker.ConnectAsync(new DnsEndPoint(requestUri.Host, port), token);
        //}

        ///// <summary>
        ///// 将 ByteBuffer 转换并通过底层 Linker 发送（用于明文请求）。
        ///// </summary>
        //private ValueTask SendRequestMessage(ByteBuffer byteBuffer, CancellationToken token)
        //{
        //    //return SendRequestMessage(byteBuffer.CommittedSequence, byteBuffer.Rental, token);
        //    return ValueTask.CompletedTask;
        //}

        ///// <summary>
        ///// 将 ByteBuffer 转换并通过 SslStream 写出（用于 HTTPS 请求发送前，SslStream 已完成握手）。
        ///// </summary>
        //private ValueTask SendAuthenticateRequestMessage(ByteBuffer byteBuffer, CancellationToken token)
        //{
        //    return default;
        //}


        /// <summary>
        /// 确保接收循环任务正在运行；若尚未运行则在线程池中启动一个任务。
        /// </summary>
        /// <param name="token">用于控制接收循环的取消令牌。</param>
        private void EnsureReceiveTaskRunning(CancellationToken token)
        {
            lock (_receiveLock)
            {
                if (receiveTask != null && !receiveTask.IsCompleted)
                    return;

                // fire-and-forget 接收循环（在托管线程池中运行）
                receiveTask = Task.Run(() => ReceiveLoopAsync(token), token);
            }
        }

        /// <summary>
        /// 接收循环：从 currentStream 读取字节并交给 HttpParser 进行增量解析。 解析成功时通过 vts.SetResult 完成等待的任务；出现错误则通过 vts.SetException 传递异常。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(currentStream);
            ArgumentNullException.ThrowIfNull(currentRequest);
            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)Utility.KilobytesToBytes(4));
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesTransferred = 0;
                    try
                    {
                        bytesTransferred = await currentStream.ReadAsync(buffer, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // 取消：如果等待者存在则通知取消
                        vts.SetException(new OperationCanceledException(token));
                        return;
                    }
                    catch (Exception ex)
                    {
                        vts.SetException(ex);
                        return;
                    }

                    if (bytesTransferred <= 0)
                    {
                        vts.SetException(new InvalidOperationException("底层连接已关闭或远端异常断开"));
                        return;
                    }

                    if (Parser.TryParseResponse(buffer.AsSpan(0, bytesTransferred), currentRequest, out var responseMessage, out int consumed))
                    {
                        vts.SetResult(responseMessage!);
                        break;
                    }
                }
            }
            finally
            {
                await Linker.DisconnectAsync();
                ArrayPool<byte>.Shared.Return(buffer);
                currentStream = null;
            }
        }

        protected override void DisposeManagedResources()
        {
            receiveCts?.Cancel();
            receiveTask?.Wait();
            receiveCts?.Dispose();
            receiveTask?.Dispose();
            _sslStream.Dispose();
            _tcpLinkerStream.Dispose();
        }

        #region IValueTaskSource<HttpResponseMessage>（委托 vts）

        /// <summary>
        /// 从内部 ManualResetValueTaskSourceCore 获取结果（用于 ValueTask 源）。
        /// </summary>
        public HttpResponseMessage GetResult(short token) => vts.GetResult(token);

        /// <summary>
        /// 获取当前内部源的状态。
        /// </summary>
        public ValueTaskSourceStatus GetStatus(short token) => vts.GetStatus(token);

        /// <summary>
        /// 注册当内部源完成时要调用的 continuation。
        /// </summary>
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => vts.OnCompleted(continuation, state, token, flags);

        public ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, SslClientAuthenticationOptions? options = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask SetHttpParser(IHttpParser httpParser, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion IValueTaskSource<HttpResponseMessage>（委托 vts）
    }
}