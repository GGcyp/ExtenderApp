using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 为 <see cref="ILinker"/> 提供一个抽象基类，实现了统一的发送/接收/连接/断开模板以及并发门控。
    /// </summary>
    public abstract class Linker : DisposableObject, ILinker
    {
        private const int DefaultCapacity = 1024 * 512;

        /// <summary>
        /// 为发送操作提供并发控制的信号量，确保同一时间只有一个发送操作在执行。
        /// </summary>
        protected readonly SemaphoreSlim SendSlim;

        /// <summary>
        /// 为接收操作提供并发控制的信号量，确保同一时间只有一个接收操作在执行。
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Result<SocketOperationValue> Send(Memory<byte> memory)
        {
            if (memory.IsEmpty)
                throw new ArgumentException("内存缓存区不可为空", nameof(memory));
            ThrowIfDisposed();
            var lease = CapacityLimiter.Acquire(memory.Length);
            SendSlim.Wait();
            try
            {
                var result = ExecuteSendAsync(memory, default).GetAwaiter().GetResult();
                if (result)
                {
                    SendCounter.Increment(result.Value.BytesTransferred);
                }
                return result;
            }
            finally
            {
                SendSlim.Release();
                lease.Dispose();
            }
        }

        /// <inheritdoc/>
        public async ValueTask<Result<SocketOperationValue>> SendAsync(Memory<byte> memory, CancellationToken token = default)
        {
            if (memory.IsEmpty)
                throw new ArgumentException("内存缓存区不可为空", nameof(memory));
            ThrowIfDisposed();
            var lease = await CapacityLimiter.AcquireAsync(memory.Length, token).ConfigureAwait(false);
            await SendSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var result = await ExecuteSendAsync(memory, token).ConfigureAwait(false);
                if (result)
                {
                    SendCounter.Increment(result.Value.BytesTransferred);
                }
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

        /// <inheritdoc/>
        public Result<SocketOperationValue> Receive(Memory<byte> memory)
        {
            if (memory.IsEmpty)
                throw new ArgumentException("内存缓存区不可为空", nameof(memory));
            ThrowIfDisposed();

            var lease = CapacityLimiter.Acquire(memory.Length);
            ReceiveSlim.Wait();
            try
            {
                var result = ExecuteReceiveAsync(memory, default).GetAwaiter().GetResult();
                if (result)
                {
                    ReceiveCounter.Increment(result.Value.BytesTransferred);
                }
                return result;
            }
            finally
            {
                ReceiveSlim.Release();
                lease.Dispose();
            }
        }

        /// <inheritdoc/>
        public async ValueTask<Result<SocketOperationValue>> ReceiveAsync(Memory<byte> memory, CancellationToken token = default)
        {
            if (memory.IsEmpty)
                throw new ArgumentException("内存缓存区不可为空", nameof(memory));
            ThrowIfDisposed();

            var lease = await CapacityLimiter.AcquireAsync(memory.Length, token).ConfigureAwait(false);
            await ReceiveSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var result = await ExecuteReceiveAsync(memory, token).ConfigureAwait(false);
                if (result)
                {
                    ReceiveCounter.Increment(result.Value.BytesTransferred);
                }
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
        /// 执行实际的连接操作，由子类根据具体协议实现。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点。</param>
        /// <param name="token">取消令牌。当操作被取消时，实现应抛出 <see cref="OperationCanceledException"/>。</param>
        protected abstract ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token);

        /// <summary>
        /// 执行实际的断开操作，由子类根据具体协议实现。
        /// </summary>
        /// <param name="token">取消令牌。实现应尽快终止断开流程。</param>
        protected abstract ValueTask ExecuteDisconnectAsync(CancellationToken token);

        /// <summary>
        /// 执行实际的发送操作，由子类根据具体协议实现。
        /// </summary>
        /// <param name="memory">要发送的有效数据窗口。</param>
        /// <param name="token">取消令牌。当操作被取消时，实现应返回一个包含 <see cref="OperationCanceledException"/> 的失败 <see cref="Result"/>。</param>
        /// <returns>一个表示异步发送操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result{SocketOperationValue}"/>。</returns>
        protected abstract ValueTask<Result<SocketOperationValue>> ExecuteSendAsync(Memory<byte> memory, CancellationToken token);

        /// <summary>
        /// 执行实际的接收操作，由子类根据具体协议实现。
        /// </summary>
        /// <param name="memory">用于写入接收数据的缓冲区。</param>
        /// <param name="token">取消令牌。当操作被取消时，实现应返回一个包含 <see cref="OperationCanceledException"/> 的失败 <see cref="Result"/>。</param>
        /// <returns>
        /// 一个表示异步接收操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result{SocketOperationValue}"/>。
        /// </returns>
        protected abstract ValueTask<Result<SocketOperationValue>> ExecuteReceiveAsync(Memory<byte> memory, CancellationToken token);

        #endregion Execute

        /// <inheritdoc/>
        public abstract void SetOption(LinkOptionLevel optionLevel, LinkOptionName optionName, DataBuffer optionValue);

        /// <inheritdoc/>
        public abstract ILinker Clone();

        protected override async ValueTask DisposeAsyncManagedResources()
        {
            await SendSlim.WaitAsync();
            await ReceiveSlim.WaitAsync();
            await DisposeAsync();

            SendCounter.Dispose();
            ReceiveCounter.Dispose();
            CapacityLimiter.Dispose();

            SendSlim.Dispose();
            ReceiveSlim.Dispose();
        }
    }
}