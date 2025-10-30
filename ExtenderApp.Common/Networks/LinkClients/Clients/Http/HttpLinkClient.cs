using System.Buffers;
using System.Net;
using System.Threading.Tasks.Sources;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using HttpRequestMessage = ExtenderApp.Data.HttpRequestMessage;
using HttpResponseMessage = ExtenderApp.Data.HttpResponseMessage;

namespace ExtenderApp.Common.Networks
{
    public class HttpLinkClient : LinkClient<ITcpLinker>, IHttpLinkClient, IValueTaskSource<HttpResponseMessage>, IDisposable
    {
        private readonly HttpParser Parser;
        private ManualResetValueTaskSourceCore<HttpResponseMessage> vts;

        // 接收循环任务与同步保护，避免重复启动接收任务
        private Task? _receiveTask;
        private readonly object _receiveLock = new();
        public short Version => vts.Version;

        public HttpLinkClient(ITcpLinker linker) : base(linker)
        {
            Parser = new();
            vts = new ManualResetValueTaskSourceCore<HttpResponseMessage>();
            vts.RunContinuationsAsynchronously = true;
        }

        public async ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            await ConnectAsync(request.RequestUri, token).ConfigureAwait(false);

            // 发送请求（释放 ByteBuffer 的租约）
            await SendRequestMessage(request.ToBuffer(), token).ConfigureAwait(false);

            // 准备等待源并启动接收任务（若尚未启动）
            vts.Reset();
            EnsureReceiveTaskRunning(token);

            // 返回可等待的 ValueTask（等待接收任务解析并 SetResult）
            return await new ValueTask<HttpResponseMessage>(this, vts.Version).ConfigureAwait(false);
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
            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)Utility.KilobytesToBytes(4));
            try
            {
                while (!token.IsCancellationRequested)
                {
                    SocketOperationResult result;
                    try
                    {
                        result = await Linker.ReceiveAsync(buffer, token).ConfigureAwait(false);
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

                    if (result.BytesTransferred <= 0)
                    {
                        vts.SetException(new InvalidOperationException("底层连接已关闭或远端异常断开"));
                        return;
                    }

                    if (Parser.TryParseResponse(buffer.AsSpan(0, result.BytesTransferred), out HttpResponseMessage responseMessage, out int consumed))
                    {
                        vts.SetResult(responseMessage);
                        ArrayPool<byte>.Shared.Return(buffer);
                        break;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
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