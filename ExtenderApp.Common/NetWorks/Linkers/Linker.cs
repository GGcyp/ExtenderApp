using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 为 <see cref="ILinker"/> 提供一个抽象基类，实现了统一的发送/接收/连接/断开模板以及并发门控。 该类负责处理流量统计、容量限制、并发同步以及生命周期管理。
    /// </summary>
    public abstract class Linker : OptionsObject, ILinker
    {
        private const int DefaultCapacity = 16 * 1024;
        private const string BufferExpandString = "缓冲区长度不足";

        /// <summary>
        /// 为发送操作提供并发控制的信号量，确保同一时间只有一个发送操作在执行。
        /// </summary>
        protected readonly SemaphoreSlim SendSlim;

        /// <summary>
        /// 为接收操作提供并发控制的信号量，确保同一时间只有一个接收操作在执行。
        /// </summary>
        protected readonly SemaphoreSlim ReceiveSlim;

        /// <summary>
        /// 获取当前连接器的并发字节容量限制器。
        /// </summary>
        public CapacityLimiter CapacityLimiter => GetOptionValue(LinkOptions.CapacityLimiterIdentifier);

        /// <summary>
        /// 获取发送字节计数的统计器。
        /// </summary>
        public ValueCounter SendCounter => GetOptionValue(LinkOptions.SendCounterIdentifier);

        /// <summary>
        /// 获取接收字节计数的统计器。
        /// </summary>
        public ValueCounter ReceiveCounter => GetOptionValue(LinkOptions.ReceiveCounterIdentifier);

        #region Properties

        /// <inheritdoc/>
        public bool Connected
        {
            get => GetOptionValue(LinkOptions.ConnectedIdentifier);
            protected set => SetOptionProtected(LinkOptions.ConnectedIdentifier, value);
        }

        /// <inheritdoc/>
        public EndPoint? LocalEndPoint
        {
            get => GetOptionValue(LinkOptions.LocalEndPointIdentifier);
            protected set => SetOptionProtected(LinkOptions.LocalEndPointIdentifier, value!);
        }

        /// <inheritdoc/>
        public EndPoint? RemoteEndPoint
        {
            get => GetOptionValue(LinkOptions.RemoteEndPointIdentifier);
            protected set => SetOptionProtected(LinkOptions.RemoteEndPointIdentifier, value!);
        }

        /// <inheritdoc/>
        public ProtocolType ProtocolType
        {
            get => GetOptionValue(LinkOptions.ProtocolTypeIdentifier);
            protected set => SetOptionProtected(LinkOptions.ProtocolTypeIdentifier, value);
        }

        /// <inheritdoc/>
        public SocketType SocketType
        {
            get => GetOptionValue(LinkOptions.SocketTypeIdentifier);
            protected set => SetOptionProtected(LinkOptions.SocketTypeIdentifier, value);
        }

        /// <inheritdoc/>
        public AddressFamily AddressFamily
        {
            get => GetOptionValue(LinkOptions.AddressFamilyIdentifier);
            protected set => SetOptionProtected(LinkOptions.AddressFamilyIdentifier, value);
        }

        /// <inheritdoc/>
        public int ReceiveBufferSize
        {
            get => GetOptionValue(LinkOptions.ReceiveBufferSizeIdentifier);
            set => SetOptionValue(LinkOptions.ReceiveBufferSizeIdentifier, value);
        }

        /// <inheritdoc/>
        public int SendBufferSize
        {
            get => GetOptionValue(LinkOptions.SendBufferSizeIdentifier);
            set => SetOptionValue(LinkOptions.SendBufferSizeIdentifier, value);
        }

        /// <inheritdoc/>
        public int ReceiveTimeout
        {
            get => GetOptionValue(LinkOptions.ReceiveTimeoutIdentifier);
            set => SetOptionValue(LinkOptions.ReceiveTimeoutIdentifier, value);
        }

        /// <inheritdoc/>
        public int SendTimeout
        {
            get => GetOptionValue(LinkOptions.SendTimeoutIdentifier);
            set => SetOptionValue(LinkOptions.SendTimeoutIdentifier, value);
        }

        #endregion Properties

        /// <summary>
        /// 初始化 <see cref="Linker"/> 类的新实例，使用默认并发容量（16KB）。
        /// </summary>
        public Linker() : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// 使用指定的并发容量初始化 <see cref="Linker"/> 类的新实例。
        /// </summary>
        /// <param name="capacity">允许的最大并发字节等待容量。</param>
        public Linker(long capacity)
        {
            SendSlim = new(1, 1);
            ReceiveSlim = new(1, 1);

            RegisterOption(LinkOptions.CapacityLimiterIdentifier, new(capacity));
            RegisterOption(LinkOptions.SendCounterIdentifier, new());
            RegisterOption(LinkOptions.ReceiveCounterIdentifier, new());
            RegisterOption(LinkOptions.ConnectedIdentifier, false);
            RegisterOption(LinkOptions.LocalEndPointIdentifier);
            RegisterOption(LinkOptions.RemoteEndPointIdentifier);
            RegisterOption(LinkOptions.ProtocolTypeIdentifier);
            RegisterOption(LinkOptions.SocketTypeIdentifier);
            RegisterOption(LinkOptions.AddressFamilyIdentifier);
            RegisterOption(LinkOptions.ReceiveBufferSizeIdentifier);
            RegisterOption(LinkOptions.SendBufferSizeIdentifier);
            RegisterOption(LinkOptions.ReceiveTimeoutIdentifier);
            RegisterOption(LinkOptions.SendTimeoutIdentifier);

            SendCounter.Start();
            ReceiveCounter.Start();
        }

        #region Connect/Close

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
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
                RemoteEndPoint = remoteEndPoint;
                Connected = true;
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
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
                RemoteEndPoint = remoteEndPoint;
                Connected = true;
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
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
                Connected = false;
                RemoteEndPoint = null;
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
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
                Connected = false;
                RemoteEndPoint = null;
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        #endregion Connect/Close

        #region Bind

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public void Bind(EndPoint endPoint)
        {
            ThrowIfDisposed();
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                if (Connected)
                    throw new InvalidOperationException("已连接时不允许绑定。");
                ExecuteBind(endPoint);
            }
            finally
            {
                SetOptionValue(LinkOptions.LocalEndPointIdentifier, endPoint);
                ReceiveSlim.Release();
                SendSlim.Release();
            }
        }

        #endregion Bind

        #region Send

        /// <inheritdoc/>
        /// <returns>返回包含 <see cref="LinkOperationValue"/> 的结果。若缓冲区为空或长度无效，返回失败状态。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Send(ReadOnlySpan<byte> span)
        {
            return Send(span, LinkFlags.None);
        }

        /// <inheritdoc/>
        /// <returns>返回包含 <see cref="LinkOperationValue"/> 的结果。若缓冲区为空或长度无效，返回失败状态。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Send(ReadOnlySpan<byte> span, LinkFlags flags)
        {
            ThrowIfDisposed();
            if (span.IsEmpty || span.Length <= 0)
                return Result.Failure<LinkOperationValue>(BufferExpandString);

            var lease = CapacityLimiter.Acquire(span.Length);
            SendSlim.Wait();
            try
            {
                var result = ExecuteSend(span, flags);
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
        /// <returns>返回包含 <see cref="LinkOperationValue"/> 的结果。若缓冲区为空或长度无效，返回失败状态。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Send(ReadOnlyMemory<byte> memory)
        {
            if (memory.IsEmpty || memory.Length <= 0)
                return Result.Failure<LinkOperationValue>(BufferExpandString);
            return Send(memory.Span, LinkFlags.None);
        }

        /// <inheritdoc/>
        /// <returns>返回包含 <see cref="LinkOperationValue"/> 的结果。若缓冲区为空或长度无效，返回失败状态。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Send(ReadOnlyMemory<byte> memory, LinkFlags flags)
        {
            if (memory.IsEmpty || memory.Length <= 0)
                return Result.Failure<LinkOperationValue>(BufferExpandString);
            return Send(memory.Span, flags);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Send(IList<ArraySegment<byte>> buffer)
        {
            return Send(buffer, LinkFlags.None);
        }

        /// <inheritdoc/>
        /// <returns>返回包含 <see cref="LinkOperationValue"/> 的结果。若缓冲区为空或长度无效，返回失败状态。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Send(IList<ArraySegment<byte>> buffer, LinkFlags flags)
        {
            ThrowIfDisposed();
            var res = GetBufferLength(buffer, out long length);
            if (!res) return res;

            var lease = CapacityLimiter.Acquire(length);
            SendSlim.Wait();
            try
            {
                var result = ExecuteSendAsync(buffer, flags, default).GetAwaiter().GetResult();
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

        #region SendAsync

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public async ValueTask<Result<LinkOperationValue>> SendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
        {
            return await SendAsync(memory, LinkFlags.None, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        /// <returns>返回包含 <see cref="LinkOperationValue"/> 的结果。若缓冲区为空或长度无效，返回失败状态。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public async ValueTask<Result<LinkOperationValue>> SendAsync(ReadOnlyMemory<byte> memory, LinkFlags flags, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (memory.IsEmpty || memory.Length <= 0)
                return Result.Failure<LinkOperationValue>(BufferExpandString);

            var lease = await CapacityLimiter.AcquireAsync(memory.Length, token).ConfigureAwait(false);
            await SendSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var result = await ExecuteSendAsync(MemoryMarshal.AsMemory(memory), flags, token).ConfigureAwait(false);
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
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public async ValueTask<Result<LinkOperationValue>> SendAsync(IList<ArraySegment<byte>> buffer, CancellationToken token = default)
        {
            return await SendAsync(buffer, LinkFlags.None, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        /// <returns>返回包含 <see cref="LinkOperationValue"/> 的结果。若缓冲区为空或长度无效，返回失败状态。</returns>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public async ValueTask<Result<LinkOperationValue>> SendAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token = default)
        {
            ThrowIfDisposed();
            var res = GetBufferLength(buffer, out long length);
            if (!res) return res;

            var lease = await CapacityLimiter.AcquireAsync(length, token).ConfigureAwait(false);
            await SendSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var result = await ExecuteSendAsync(buffer, flags, token).ConfigureAwait(false);
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

        #endregion SendAsync

        #region Receive

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Receive(Span<byte> span)
        {
            return Receive(span, LinkFlags.None);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Receive(Span<byte> span, LinkFlags flags)
        {
            ThrowIfDisposed();
            if (span.IsEmpty || span.Length <= 0)
                return Result.Failure<LinkOperationValue>(BufferExpandString);

            var lease = CapacityLimiter.Acquire(span.Length);
            ReceiveSlim.Wait();
            try
            {
                var result = ExecuteReceive(span, flags);
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
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Receive(Memory<byte> memory)
        {
            return Receive(memory, LinkFlags.None);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Receive(Memory<byte> memory, LinkFlags flags)
        {
            ThrowIfDisposed();
            if (memory.IsEmpty || memory.Length <= 0)
                return Result.Failure<LinkOperationValue>(BufferExpandString);

            var lease = CapacityLimiter.Acquire(memory.Length);
            ReceiveSlim.Wait();
            try
            {
                var result = ExecuteReceive(memory.Span, flags);
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
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Receive(IList<ArraySegment<byte>> buffer)
        {
            return Receive(buffer, LinkFlags.None);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public Result<LinkOperationValue> Receive(IList<ArraySegment<byte>> segment, LinkFlags flags)
        {
            ThrowIfDisposed();
            var res = GetBufferLength(segment, out long length);
            if (!res) return res;

            var lease = CapacityLimiter.Acquire(length);
            ReceiveSlim.Wait();
            try
            {
                var result = ExecuteReceive(segment, flags);
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

        #region ReceiveAsync

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public ValueTask<Result<LinkOperationValue>> ReceiveAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return ReceiveAsync(memory, LinkFlags.None, token);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public async ValueTask<Result<LinkOperationValue>> ReceiveAsync(Memory<byte> memory, LinkFlags flags, CancellationToken token = default)
        {
            ThrowIfDisposed();
            if (memory.IsEmpty || memory.Length <= 0)
                return Result.Failure<LinkOperationValue>(BufferExpandString);

            var lease = await CapacityLimiter.AcquireAsync(memory.Length, token).ConfigureAwait(false);
            await ReceiveSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var result = await ExecuteReceiveAsync(memory, flags, token).ConfigureAwait(false);
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
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public ValueTask<Result<LinkOperationValue>> ReceiveAsync(IList<ArraySegment<byte>> buffer, CancellationToken token = default)
        {
            return ReceiveAsync(buffer, LinkFlags.None, token);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">当对象已释放时抛出。</exception>
        public async ValueTask<Result<LinkOperationValue>> ReceiveAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token = default)
        {
            ThrowIfDisposed();
            var res = GetBufferLength(buffer, out long length);
            if (!res) return res;

            var lease = await CapacityLimiter.AcquireAsync(length, token).ConfigureAwait(false);
            await ReceiveSlim.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var result = await ExecuteReceiveAsync(buffer, flags, token).ConfigureAwait(false);
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

        #endregion ReceiveAsync

        #region Execute

        /// <summary>
        /// 执行实际的异步连接操作，由子类根据具体网络协议实现。
        /// </summary>
        /// <param name="remoteEndPoint">目标远端终结点。</param>
        /// <param name="token">用于取消操作的令牌。</param>
        /// <returns>一个表示异步连接操作的 <see cref="ValueTask"/>。</returns>
        protected abstract ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token);

        /// <summary>
        /// 执行实际的异步断开操作，由子类根据具体网络协议实现。
        /// </summary>
        /// <param name="token">用于取消操作的令牌。</param>
        /// <returns>一个表示异步断开操作的 <see cref="ValueTask"/>。</returns>
        protected abstract ValueTask ExecuteDisconnectAsync(CancellationToken token);

        /// <summary>
        /// 执行实际的绑定操作，由子类根据具体网络协议实现。
        /// </summary>
        /// <param name="endPoint">要绑定的本地终结点。</param>
        protected abstract void ExecuteBind(EndPoint endPoint);

        #region Send

        /// <summary>
        /// 执行同步发送跨度数据的具体逻辑。
        /// </summary>
        /// <param name="span">包含要发送字节的跨度。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>发送操作的执行结果。</returns>
        protected abstract Result<LinkOperationValue> ExecuteSend(ReadOnlySpan<byte> span, LinkFlags flags);

        /// <summary>
        /// 执行实际的单缓冲区异 async 发送操作。
        /// </summary>
        /// <param name="memory">要发送的数据内存区域。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">用于取消操作的令牌。</param>
        /// <returns>一个表示异步发送结果的 <see cref="ValueTask{TResult}"/>。</returns>
        protected abstract ValueTask<Result<LinkOperationValue>> ExecuteSendAsync(Memory<byte> memory, LinkFlags flags, CancellationToken token);

        /// <summary>
        /// 执行实际的非连续缓冲区异步发送操作。
        /// </summary>
        /// <param name="buffer">非连续的缓冲区列表。</param>
        /// <param name="flags">发送标志。</param>
        /// <param name="token">用于取消操作的令牌。</param>
        /// <returns>一个表示异步发送结果的 <see cref="ValueTask{TResult}"/>。</returns>
        protected abstract ValueTask<Result<LinkOperationValue>> ExecuteSendAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token);

        #endregion Send

        #region Receive

        /// <summary>
        /// 执行同步接收跨度数据的具体逻辑。
        /// </summary>
        /// <param name="span">用于接收数据的跨度。</param>
        /// <returns>接收操作的执行结果。</returns>
        protected abstract Result<LinkOperationValue> ExecuteReceive(Span<byte> span, LinkFlags flags);

        /// <summary>
        /// 执行同步接收非连续缓冲区数据的具体逻辑。
        /// </summary>
        /// <param name="buffer">用于填充接收数据的非连续缓冲区列表。</param>
        /// <returns>接收操作的执行结果。</returns>
        protected abstract Result<LinkOperationValue> ExecuteReceive(IList<ArraySegment<byte>> buffer, LinkFlags flags);

        /// <summary>
        /// 执行实际的单缓冲区异步接收操作。
        /// </summary>
        /// <param name="memory">用于写入接收数据的目标内存区域。</param>
        /// <param name="token">用于取消操作的令牌。</param>
        /// <returns>一个表示异步接收结果的 <see cref="ValueTask{TResult}"/>。</returns>
        protected abstract ValueTask<Result<LinkOperationValue>> ExecuteReceiveAsync(Memory<byte> memory, LinkFlags flags, CancellationToken token);

        /// <summary>
        /// 执行实际的非连续缓冲区异步接收操作。
        /// </summary>
        /// <param name="buffer">用于填充接收数据的非连续缓冲区列表。</param>
        /// <param name="token">用于取消操作的令牌。</param>
        /// <returns>一个表示异步接收结果的 <see cref="ValueTask{TResult}"/>。</returns>
        protected abstract ValueTask<Result<LinkOperationValue>> ExecuteReceiveAsync(IList<ArraySegment<byte>> buffer, LinkFlags flags, CancellationToken token);

        #endregion Receive

        #endregion Execute

        /// <inheritdoc/>
        public abstract ILinker Clone();

        /// <summary>
        /// 计算非连续缓冲区列表的总字节长度，并执行基础状态验证。
        /// </summary>
        /// <param name="buffer">缓冲区列表。</param>
        /// <param name="length">输出参数，返回计算出的总长度。</param>
        /// <returns>若缓冲区有效且包含分片则返回成功结果；否则返回包含错误信息的失败结果。</returns>
        protected Result<LinkOperationValue> GetBufferLength(IList<ArraySegment<byte>> buffer, out long length)
        {
            length = 0;
            if (buffer == null || buffer.Count == 0)
                return Result.Failure<LinkOperationValue>(BufferExpandString);
            ThrowIfDisposed();

            for (int i = 0; i < buffer.Count; i++)
            {
                length += buffer[i].Count;
            }
            return Result.Success(LinkOperationValue.Empty);
        }

        /// <inheritdoc/>
        protected override sealed void DisposeManagedResources()
        {
            if (IsDisposed)
                return;
            DisposeAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncManagedResources()
        {
            await SendSlim.WaitAsync().ConfigureAwait(false);
            await ReceiveSlim.WaitAsync().ConfigureAwait(false);

            SendCounter.Dispose();
            ReceiveCounter.Dispose();
            CapacityLimiter.Dispose();

            SendSlim.Dispose();
            ReceiveSlim.Dispose();
        }
    }
}