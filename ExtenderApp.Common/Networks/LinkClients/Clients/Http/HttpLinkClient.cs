using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks.Sources;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using HttpRequestMessage = ExtenderApp.Data.HttpRequestMessage;
using HttpResponseMessage = ExtenderApp.Data.HttpResponseMessage;

namespace ExtenderApp.Common.Networks
{
    public class HttpLinkClient : LinkClient<ITcpLinker>, IHttpLinkClient, IValueTaskSource<HttpResponseMessage>, IDisposable
    {
        private readonly TcpLinkerStream _tcpLinkerStream;
        private readonly SslStream _sslStream;
        private readonly HttpParser Parser;
        private ManualResetValueTaskSourceCore<HttpResponseMessage> vts;

        public SslClientAuthenticationOptions AuthenticationOptions;
        // 接收循环任务与同步保护，避免重复启动接收任务
        private Task? _receiveTask;
        private readonly object _receiveLock = new();
        public short Version => vts.Version;

        private Stream? currentStream;
        private HttpRequestMessage? currentRequest;
        public int MaxRedirects { get; set; } = 5;

        public HttpLinkClient(ITcpLinker linker) : base(linker)
        {
            Parser = new();
            vts = new ManualResetValueTaskSourceCore<HttpResponseMessage>();
            vts.RunContinuationsAsynchronously = true;
            _tcpLinkerStream = linker.ToStream();
            _sslStream = new(_tcpLinkerStream);
            AuthenticationOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                CertificateRevocationCheckMode = X509RevocationMode.Online,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            };
        }

        public async ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default, SslClientAuthenticationOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.RequestUri);
            int redirectCount = 0;
            Uri currentUri = request.RequestUri;

            while (true)
            {
                await ConnectAsync(request.RequestUri, token).ConfigureAwait(false);

                currentRequest = request;
                currentStream = request.IsHttps ? _sslStream : _tcpLinkerStream;
                if (request.IsHttps)
                {
                    options = options ?? AuthenticationOptions;
                    options.TargetHost = request.RequestUri!.Host;
                    await _sslStream.AuthenticateAsClientAsync(options, token).ConfigureAwait(false);
                    await SendAuthenticateRequestMessage(request.ToBuffer(), token).ConfigureAwait(false);
                }
                else
                {
                    await SendRequestMessage(request.ToBuffer(), token).ConfigureAwait(false);
                }

                // 准备等待源并启动接收任务（若尚未启动）
                vts.Reset();
                EnsureReceiveTaskRunning(token);
                var response = await new ValueTask<HttpResponseMessage>(this, vts.Version).ConfigureAwait(false);

                // 如果不是重定向，直接返回
                if (response.StatusCode != HttpStatusCode.Redirect || redirectCount >= MaxRedirects)
                    return response;

                // 获取 Location 头（需检查是否存在并能解析）
                string? location = string.Empty;
                if (!response.Headers.TryGetValues(HttpHeaders.Location, out var locs))
                {
                    location = locs.FirstOrDefault();
                    if (string.IsNullOrEmpty(location))
                        return response;
                }

                Uri newUri = new Uri(currentUri, location); // 解析相对/绝对
                redirectCount++;

                // 必要时调整请求（例如 302/303 将方法改为 GET 并清除 Body）
                if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.RedirectMethod)
                {
                    request.Method = Data.HttpMethod.Get;
                    request.Body.Dispose();
                }

                if (!string.Equals(currentUri.Host, newUri.Host, StringComparison.OrdinalIgnoreCase) || currentUri.Port != newUri.Port)
                {
                    await Linker.DisconnectAsync().ConfigureAwait(false);
                }

                // 准备下次循环
                currentUri = newUri;
                request.RequestUri = newUri;
                request.Headers.SetValue(HttpHeaders.Host, newUri.Authority);
            }
        }

        private ValueTask ConnectAsync(Uri? requestUri, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(requestUri);
            int port = requestUri.Port > 0 ? requestUri.Port : 80;
            return Linker.ConnectAsync(new DnsEndPoint(requestUri.Host, port), token);
        }

        private ValueTask SendRequestMessage(ByteBuffer byteBuffer, CancellationToken token)
        {
            return SendRequestMessage(byteBuffer.UnreadSequence, byteBuffer.Rental, token);
        }

        private async ValueTask SendRequestMessage(ReadOnlySequence<byte> memories, SequencePool<byte>.SequenceRental rental, CancellationToken token)
        {
            await Linker.SendAsync(memories, token).ConfigureAwait(false);
            rental.Dispose();
        }

        private ValueTask SendAuthenticateRequestMessage(ByteBuffer byteBuffer, CancellationToken token)
        {
            return SendAuthenticateRequestMessage(byteBuffer.UnreadSequence, byteBuffer.Rental, token);
        }

        private async ValueTask SendAuthenticateRequestMessage(ReadOnlySequence<byte> memories, SequencePool<byte>.SequenceRental rental, CancellationToken token)
        {
            foreach (var segment in memories)
            {
                await _sslStream.WriteAsync(segment, token).ConfigureAwait(false);
            }
            await _sslStream.FlushAsync(token).ConfigureAwait(false);
            rental.Dispose();
        }

        private void EnsureReceiveTaskRunning(CancellationToken token)
        {
            lock (_receiveLock)
            {
                if (_receiveTask != null && !_receiveTask.IsCompleted)
                    return;

                // fire-and-forget 接收循环（在托管线程池中运行）
                _receiveTask = Task.Run(() => ReceiveLoopAsync(token), token);
            }
        }

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

        #region IValueTaskSource<HttpResponseMessage>（委托 vts）
        public HttpResponseMessage GetResult(short token) => vts.GetResult(token);

        public ValueTaskSourceStatus GetStatus(short token) => vts.GetStatus(token);

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => vts.OnCompleted(continuation, state, token, flags);
        #endregion
    }
}