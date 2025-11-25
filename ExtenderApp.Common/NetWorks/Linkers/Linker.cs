using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 抽象基类 Linker，实现 ILinker，提供统一的发送/接收/连接/断开模板与并发门控。
    /// </summary>
    public abstract class Linker : DisposableObject, ILinker
    {
        private const int DefaultCapacity = 1024 * 512;

        /// <summary>
        /// 发送操作的并发门控信号量。
        /// </summary>
        protected readonly SemaphoreSlim SendSlim;

        /// <summary>
        /// 接收操作的并发门控信号量。
        /// </summary>
        protected readonly SemaphoreSlim ReceiveSlim;

        public CapacityLimiter CapacityLimiter { get; }

        public ValueCounter SendCounter { get; }

        public ValueCounter ReceiveCounter { get; }

        #region 子类实现

        public abstract bool Connected { get; }

        public abstract EndPoint? LocalEndPoint { get; }

        public abstract EndPoint? RemoteEndPoint { get; }

        public abstract ProtocolType ProtocolType { get; }

        public abstract SocketType SocketType { get; }

        public abstract AddressFamily AddressFamily { get; }

        #endregion 子类实现

        public Linker() : this(DefaultCapacity)
        {
        }

        public Linker(long capacity)
        {
            SendSlim = new(1, 1);
            ReceiveSlim = new(1, 1);

            CapacityLimiter = new(capacity);

            SendCounter = new();
            ReceiveCounter = new();
            SendCounter.Start();
            ReceiveCounter.Start();
        }

        #region Connect/Close

        /// <summary>
        /// 同步连接到远端。
        /// </summary>
        public void Connect(EndPoint remoteEndPoint)
        {
            ThrowIfDisposed();
            // 保证连接/断开与收发互斥，避免在 I/O 中途切换连接状态
            SendSlim.Wait();
            ReceiveSlim.Wait();

            if (Connected)
                Disconnect();

            try
            {
                ExecuteConnectAsync(remoteEndPoint, default).GetAwaiter().GetResult();
            }
            finally
            {
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        /// <summary>
        /// 异步连接到远端。
        /// </summary>
        public async ValueTask ConnectAsync(EndPoint remoteEndPoint, CancellationToken token = default)
        {
            ThrowIfDisposed();
            await SendSlim.WaitAsync(token).ConfigureAwait(false);
            await ReceiveSlim.WaitAsync(token).ConfigureAwait(false);

            if (Connected)
                await DisconnectAsync(token).ConfigureAwait(false);

            try
            {
                await ExecuteConnectAsync(remoteEndPoint, token).ConfigureAwait(false);
            }
            finally
            {
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        /// <summary>
        /// 同步断开连接。
        /// </summary>
        public void Disconnect()
        {
            ThrowIfDisposed();
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                ExecuteDisconnectAsync(default).GetAwaiter().GetResult();
            }
            finally
            {
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        /// <summary>
        /// 异步断开连接。
        /// </summary>
        public async ValueTask DisconnectAsync(CancellationToken token = default)
        {
            ThrowIfDisposed();
            await SendSlim.WaitAsync(token).ConfigureAwait(false);
            await ReceiveSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                await ExecuteDisconnectAsync(token).ConfigureAwait(false);
            }
            finally
            {
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        #endregion Connect/Close

        #region Send

        public SocketOperationResult Send(Memory<byte> memory)
        {
            if (memory.IsEmpty)
                return new SocketOperationResult(false, 0, RemoteEndPoint, null, default);
            ThrowIfDisposed();
            var lease = CapacityLimiter.Acquire(memory.Length);
            SendSlim.Wait();
            try
            {
                SocketOperationResult result = ExecuteSendAsync(memory, default).GetAwaiter().GetResult();
                SendCounter.Increment(result.BytesTransferred);
                return result;
            }
            finally
            {
                SendSlim.Release();
                lease.Dispose();
            }
        }

        public async ValueTask<SocketOperationResult> SendAsync(Memory<byte> memory, CancellationToken token = default)
        {
            if (memory.IsEmpty)
                return SocketOperationResult.Failure();
            ThrowIfDisposed();
            var lease = await CapacityLimiter.AcquireAsync(memory.Length, token).ConfigureAwait(false);
            await SendSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var result = await ExecuteSendAsync(memory, token).ConfigureAwait(false);
                SendCounter.Increment(result.BytesTransferred);
                return result;
            }
            finally
            {
                SendSlim.Release();
                lease.Dispose();
            }
        }

        #endregion Send

        #region Receive

        public SocketOperationResult Receive(Memory<byte> memory)
        {
            ThrowIfDisposed();
            var lease = CapacityLimiter.Acquire(memory.Length);
            ReceiveSlim.Wait();
            try
            {
                SocketOperationResult result = ExecuteReceiveAsync(memory, default).GetAwaiter().GetResult();
                ReceiveCounter.Increment(result.BytesTransferred);
                return result;
            }
            finally
            {
                ReceiveSlim.Release();
                lease.Dispose();
            }
        }

        public async ValueTask<SocketOperationResult> ReceiveAsync(Memory<byte> memory, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (memory.IsEmpty || memory.Length == 0)
                return SocketOperationResult.Failure();

            var lease = await CapacityLimiter.AcquireAsync(memory.Length, token).ConfigureAwait(false);
            await ReceiveSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var result = await ExecuteReceiveAsync(memory, token).ConfigureAwait(false);
                ReceiveCounter.Increment(result.BytesTransferred);
                return result;
            }
            finally
            {
                ReceiveSlim.Release();
                lease.Dispose();
            }
        }

        #endregion Receive

        #region Execute

        /// <summary>
        /// 执行实际的连接操作（由子类实现具体协议/套接字逻辑）。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点。</param>
        /// <param name="token">取消令牌；取消时应终止正在进行的连接流程并抛出取消异常。</param>
        protected abstract ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token);

        /// <summary>
        /// 执行实际的断开操作（由子类实现具体协议/套接字逻辑）。
        /// </summary>
        /// <param name="token">取消令牌；取消时应尽快终止断开流程。</param>
        protected abstract ValueTask ExecuteDisconnectAsync(CancellationToken token);

        /// <summary>
        /// 执行实际的发送操作（由子类实现具体协议/套接字逻辑）。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口；调用方需保证在操作完成前其内容与引用保持有效。</param>
        /// <param name="token">取消令牌；取消时应中断正在进行的发送并以异常结束。</param>
        /// <returns>本次发送的结果，至少包含已发送字节数与错误信息。</returns>
        protected abstract ValueTask<SocketOperationResult> ExecuteSendAsync(Memory<byte> memory, CancellationToken token);

        /// <summary>
        /// 执行实际的接收操作（由子类实现具体协议/套接字逻辑）。
        /// </summary>
        /// <param name="memory">可写缓冲区；底层应将收到的数据写入其中。</param>
        /// <param name="token">取消令牌；取消时应中断正在进行的接收并以异常结束。</param>
        /// <returns>
        /// 本次接收的结果：对于 TCP，返回 0 通常表示对端优雅关闭；对于 UDP，若缓冲不足可能发生截断（由子类在结果中体现）。
        /// </returns>
        protected abstract ValueTask<SocketOperationResult> ExecuteReceiveAsync(Memory<byte> memory, CancellationToken token);

        #endregion Execute

        public abstract ILinker Clone();

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            Disconnect();

            SendSlim.Wait();
            ReceiveSlim.Wait();

            SendCounter.Dispose();
            ReceiveCounter.Dispose();
            CapacityLimiter.Dispose();

            SendSlim.Dispose();
            ReceiveSlim.Dispose();
        }
    }
}