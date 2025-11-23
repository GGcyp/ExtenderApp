using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 将基于事件的 <see cref="SocketAsyncEventArgs"/>
    /// I/O 封装为可 <c>await</c> 的 <see
    /// cref="ValueTask{TResult}"/> 模式。
    /// </summary>
    /// <remarks>
    /// 设计要点：
    /// - 单实例不可并发使用：一次仅能有一个未完成的操作；开始下一次操作前会调用 <see cref="ManualResetValueTaskSourceCore{TResult}.Reset"/>。
    /// - 通过 <see cref="ManualResetValueTaskSourceCore{TResult}"/>
    /// 实现零分配等待，并将延续异步投递（ <see cref="ManualResetValueTaskSourceCore{TResult}.RunContinuationsAsynchronously"/>）。
    /// - 成功完成时返回 <see
    ///   cref="SocketOperationResult"/>；失败时以 <see
    ///   cref="SocketException"/> 或 <see cref="OperationCanceledException"/>
    /// 结束任务（await 时抛出）。
    /// - 接收需由调用方提供可写缓冲（ <see
    ///   cref="ReceiveAsync(Socket, Memory{byte},
    ///   CancellationToken)"/>）；发送从 <see
    ///   cref="Memory{T}"/> 发起（ <see
    ///   cref="SendAsync(Socket, Memory{byte},
    ///   SocketFlags, CancellationToken)"/>）。
    /// - 取消策略：取消时默认关闭 Socket 以中断 I/O；SAEA 不支持细粒度取消单个操作。
    /// </remarks>
    public class AwaitableSocketEventArgs : SocketAsyncEventArgs, IValueTaskSource<SocketOperationResult>, IValueTaskSource<Socket>
    {
        private enum PendingOperation : byte
        {
            None = 0,
            Send,
            SendTo,
            Receive,
            ReceiveFrom,
            ReceiveMessageFrom,
            Accept
        }

        /// <summary>
        /// 一次基于缓冲的收发操作的 awaitable 核心。
        /// </summary>
        private ManualResetValueTaskSourceCore<SocketOperationResult> vts;

        /// <summary>
        /// 一次 Accept 操作的 awaitable 核心。
        /// </summary>
        private ManualResetValueTaskSourceCore<Socket> vtsAccept;

        /// <summary>
        /// 缓存的 <see cref="SocketException"/>，用于异常完成路径减少临时分配。
        /// </summary>
        private SocketException? socketError;

        // 取消相关状态

        /// <summary>
        /// 取消令牌的注册句柄，用于在 I/O 期间响应取消。
        /// </summary>
        private CancellationTokenRegistration ctr;

        /// <summary>
        /// 本轮 I/O 关联的取消令牌。
        /// </summary>
        private CancellationToken token;

        /// <summary>
        /// 当前正在执行 I/O 的套接字（用于取消时关闭以打断 I/O）。
        /// </summary>
        private Socket? currentSocket;

        /// <summary>
        /// 完成状态：0=进行中，1=I/O 完成，2=已取消。用于避免重复完成。
        /// </summary>
        private int completion;

        /// <summary>
        /// 当前操作类型，用于在完成/取消时路由到正确的 awaitable 管道。
        /// </summary>
        private PendingOperation pendingOperation;

        /// <summary>
        /// 当前轮次的令牌版本号。构造 <see
        /// cref="ValueTask{TResult}"/> 时需携带该版本以确保一次性消费。
        /// </summary>
        public short Version => vts.Version;

        public short AcceptVersion => vtsAccept.Version;

        /// <summary>
        /// 初始化 AwaitableSocketEventArgs 实例，并启用异步延续投递。
        /// </summary>
        public AwaitableSocketEventArgs() : base()
        {
            vts.RunContinuationsAsynchronously = true;
        }

        /// <summary>
        /// Socket 操作完成回调：根据 <see
        /// cref="SocketAsyncEventArgs.SocketError"/> 触发成功或异常完成，并清理取消注册。
        /// </summary>
        /// <param name="e">完成的事件参数（即自身）。</param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            // 仅允许一次完成；若已被取消则忽略 I/O 完成
            if (Interlocked.CompareExchange(ref completion, 1, 0) != 0)
            {
                // 若先被取消则只需释放注册
                if (Volatile.Read(ref completion) == 2)
                    ctr.Dispose();
                return;
            }

            ctr.Dispose();
            currentSocket = null;
            token = default;

            if (SocketError == SocketError.Success)
            {
                if (pendingOperation == PendingOperation.Accept)
                {
                    vtsAccept.SetResult(AcceptSocket!);
                }
                else
                {
                    vts.SetResult(GetSocketOperationResult());
                }
            }
            else
            {
                socketError ??= CreateSocketException(SocketError);
                vtsAccept.SetException(socketError);
                vts.SetException(socketError);
            }

            pendingOperation = PendingOperation.None;
        }

        #region Send

        /// <summary>
        /// 发送指定缓冲区的数据（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">
        /// 目标套接字（TCP 或已 Connect 的 UDP）。
        /// </param>
        /// <param name="memory">要发送的有效数据窗口。调用方需保证在本次异步发送完成前，该缓冲区保持有效且内容不被修改。</param>
        /// <param name="token">
        /// 取消令牌；取消时将关闭 <paramref name="socket"/>
        /// 以中断 I/O（SAEA 不支持细粒度取消单个操作）。
        /// </param>
        /// <param name="flags">
        /// 发送标志，通常为 <see cref="SocketFlags.None"/>。
        /// </param>
        /// <returns>
        /// 成功时返回 <see
        /// cref="SocketOperationResult"/>（其中
        /// BytesTransferred 为本次实际发送的字节数）； 失败时
        /// await 抛出 <see
        /// cref="SocketException"/>；取消时抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// - TCP
        ///   为字节流协议，可能发生“部分发送”；若需保证全部数据发出，请在调用方根据返回长度循环直至发送完毕。 <br/>
        /// - 对于 UDP，若套接字已 Connect 则可使用本方法；未
        ///   Connect 的 UDP 请使用 <see
        ///   cref="SendToAsync(Socket,
        ///   ReadOnlyMemory{byte}, EndPoint,
        ///   SocketFlags, CancellationToken)"/>。
        /// </remarks>
        public ValueTask<SocketOperationResult> SendAsync(Socket socket, Memory<byte> memory, CancellationToken token, SocketFlags flags = SocketFlags.None)
        {
            socketError = null;
            vts.Reset();
            SocketFlags = flags;
            completion = 0;
            this.token = token;
            pendingOperation = PendingOperation.Send;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            SetBuffer(memory);

            if (!socket.SendAsync(this))
            {
                // 同步完成路径：释放注册并返回
                ctr.Dispose();
                currentSocket = null;
                this.token = default;

                if (SocketError == SocketError.Success)
                    return ValueTask.FromResult(GetSocketOperationResult());

                return ValueTask.FromException<SocketOperationResult>(CreateSocketException(SocketError));
            }

            return new ValueTask<SocketOperationResult>(this, Version);
        }

        /// <summary>
        /// UDP 发送到指定远端（未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect）。</param>
        /// <param name="buffer">要发送的数据缓冲。需在操作完成前保持有效且内容不被修改。</param>
        /// <param name="remoteEndPoint">
        /// 目标远端地址；其 <see
        /// cref="EndPoint.AddressFamily"/> 必须与
        /// <paramref name="socket"/> 一致。
        /// </param>
        /// <param name="token">
        /// 取消令牌；取消时将关闭 <paramref name="socket"/>
        /// 以中断 I/O。
        /// </param>
        /// <param name="flags">
        /// 发送标志，通常为 <see cref="SocketFlags.None"/>。
        /// </param>
        /// <returns>
        /// 成功返回 <see
        /// cref="SocketOperationResult"/>；失败抛出
        /// <see cref="SocketException"/>；取消抛出
        /// <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// - 若要发送广播，请先设置
        ///   <c>socket.SetSocketOption(SocketOptionLevel.Socket,
        ///   SocketOptionName.Broadcast,
        ///   true)</c>。 <br/>
        /// - 若套接字已 Connect，建议使用 <see
        ///   cref="SendAsync(Socket,
        ///   Memory{byte}, CancellationToken, SocketFlags)"/>。
        /// </remarks>
        public ValueTask<SocketOperationResult> SendToAsync(Socket socket, ReadOnlyMemory<byte> buffer, EndPoint remoteEndPoint, CancellationToken token = default, SocketFlags flags = SocketFlags.None)
        {
            socketError = null;
            vts.Reset();
            SocketFlags = flags;
            completion = 0;
            this.token = token;
            pendingOperation = PendingOperation.SendTo;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            RemoteEndPoint = remoteEndPoint;                    // 必须指定远端
            SetBuffer(MemoryMarshal.AsMemory(buffer));          // ReadOnlyMemory -> ResultMessage

            if (!socket.SendToAsync(this))
            {
                ctr.Dispose();
                currentSocket = null;
                this.token = default;

                if (SocketError == SocketError.Success)
                    return ValueTask.FromResult(GetSocketOperationResult());

                return ValueTask.FromException<SocketOperationResult>(CreateSocketException(SocketError));
            }

            return new ValueTask<SocketOperationResult>(this, Version);
        }

        #endregion Send

        #region Receive

        /// <summary>
        /// 接收（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">
        /// 源套接字（TCP 或已 Connect 的 UDP）。
        /// </param>
        /// <param name="buffer">可写缓冲区；内核将在其中写入接收的数据。调用方需保证缓冲在本次操作完成前保持有效。</param>
        /// <param name="token">
        /// 取消令牌；取消时将关闭 <paramref name="socket"/>
        /// 以中断 I/O。
        /// </param>
        /// <returns>
        /// 成功返回 <see
        /// cref="SocketOperationResult"/>；失败抛出
        /// <see cref="SocketException"/>；取消抛出
        /// <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// - TCP：返回字节数为 0 通常表示对端优雅关闭。 <br/>
        /// - UDP：若缓冲不足可能发生截断，可通过 <see
        ///   cref="SocketAsyncEventArgs.SocketFlags"/>
        ///   检查 <see cref="SocketFlags.Truncated"/>。
        /// </remarks>
        public ValueTask<SocketOperationResult> ReceiveAsync(Socket socket, Memory<byte> buffer, CancellationToken token)
        {
            socketError = null;
            vts.Reset();
            completion = 0;
            this.token = token;
            pendingOperation = PendingOperation.Receive;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            SetBuffer(buffer);

            if (!socket.ReceiveAsync(this))
            {
                ctr.Dispose();
                currentSocket = null;
                this.token = default;

                if (SocketError == SocketError.Success)
                    return ValueTask.FromResult(GetSocketOperationResult());

                socketError = CreateSocketException(SocketError);
                return ValueTask.FromException<SocketOperationResult>(CreateSocketException(SocketError));
            }

            return new ValueTask<SocketOperationResult>(this, Version);
        }

        /// <summary>
        /// UDP 接收并获取来源地址（未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect）。</param>
        /// <param name="buffer">可写缓冲区；用于接收的数据存放。</param>
        /// <param name="anyEndPoint">
        /// 用于占位的 EndPoint，其 AddressFamily 必须与
        /// <paramref name="socket"/> 一致（例如 new
        /// IPEndPoint(IPAddress.Any, 0)）。 完成后
        /// <see
        /// cref="SocketAsyncEventArgs.RemoteEndPoint"/> 将被设置为实际来源地址。
        /// </param>
        /// <param name="token">
        /// 取消令牌；取消时将关闭 <paramref name="socket"/>
        /// 以中断 I/O。
        /// </param>
        /// <returns>
        /// 成功返回 <see
        /// cref="SocketOperationResult"/>（包含来源地址）；失败抛出
        /// <see cref="SocketException"/>；取消抛出
        /// <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// 广播/组播可通过 <see
        /// cref="SocketAsyncEventArgs.SocketFlags"/>
        /// 检查 <see cref="SocketFlags.Broadcast"/>
        /// 与 <see cref="SocketFlags.Multicast"/>。
        /// </remarks>
        public ValueTask<SocketOperationResult> ReceiveFromAsync(Socket socket, Memory<byte> buffer, EndPoint anyEndPoint, CancellationToken token = default)
        {
            socketError = null;
            vts.Reset();
            completion = 0;
            this.token = token;
            pendingOperation = PendingOperation.ReceiveFrom;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            RemoteEndPoint = anyEndPoint; // 提供占位 EP，完成后将被设置为真实来源
            SetBuffer(buffer);

            if (!socket.ReceiveFromAsync(this))
            {
                ctr.Dispose();
                currentSocket = null;
                this.token = default;

                if (SocketError == SocketError.Success)
                    return ValueTask.FromResult(GetSocketOperationResult());

                socketError = CreateSocketException(SocketError);
                return ValueTask.FromException<SocketOperationResult>(CreateSocketException(SocketError));
            }

            return new ValueTask<SocketOperationResult>(this, Version);
        }

        /// <summary>
        /// UDP 接收（带 IP 层报文信息与标志），用于识别
        /// Truncated/Broadcast/Multicast 等。
        /// </summary>
        /// <param name="socket">
        /// UDP 套接字（未 Connect 或 Connect 均可）。
        /// </param>
        /// <param name="buffer">可写缓冲区；用于接收的数据存放。</param>
        /// <param name="anyEndPoint">
        /// 用于占位的 EndPoint，其 AddressFamily 必须与
        /// <paramref name="socket"/> 一致。 完成后 <see
        /// cref="SocketAsyncEventArgs.RemoteEndPoint"/> 将被设置为实际来源地址。
        /// </param>
        /// <param name="token">
        /// 取消令牌；取消时将关闭 <paramref name="socket"/>
        /// 以中断 I/O。
        /// </param>
        /// <returns>
        /// 成功返回 <see
        /// cref="SocketOperationResult"/>（包含来源地址与
        /// <see
        /// cref="SocketAsyncEventArgs.ReceiveMessageFromPacketInfo"/>
        /// 信息）； 失败抛出 <see
        /// cref="SocketException"/>；取消抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// 要获取 <see
        /// cref="SocketAsyncEventArgs.ReceiveMessageFromPacketInfo"/>，需在套接字上启用数据报包信息：
        /// IPv4：
        /// <c>socket.SetSocketOption(SocketOptionLevel.IP,
        /// SocketOptionName.PacketInformation,
        /// true)</c>； IPv6：
        /// <c>socket.SetSocketOption(SocketOptionLevel.IPv6,
        /// SocketOptionName.PacketInformation, true)</c>。
        /// </remarks>
        public ValueTask<SocketOperationResult> ReceiveMessageFromAsync(Socket socket, Memory<byte> buffer, EndPoint anyEndPoint, CancellationToken token = default)
        {
            socketError = null;
            vts.Reset();
            completion = 0;
            this.token = token;
            pendingOperation = PendingOperation.ReceiveMessageFrom;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            RemoteEndPoint = anyEndPoint;
            SetBuffer(buffer);

            if (!socket.ReceiveMessageFromAsync(this))
            {
                ctr.Dispose();
                currentSocket = null;
                this.token = default;

                if (SocketError == SocketError.Success)
                    return ValueTask.FromResult(GetSocketOperationResult());

                socketError = CreateSocketException(SocketError);
                return ValueTask.FromException<SocketOperationResult>(CreateSocketException(SocketError));
            }

            return new ValueTask<SocketOperationResult>(this, Version);
        }

        #endregion Receive

        #region Accept

        /// <summary>
        /// 等待 TCP 连接接入（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">处于监听状态的套接字。</param>
        /// <param name="token">
        /// 取消令牌；取消时将关闭 <paramref name="socket"/>
        /// 以中断 Accept。
        /// </param>
        /// <returns>
        /// 成功时返回已建立连接的 Socket；失败抛出
        /// SocketException；取消抛出 OperationCanceledException。
        /// </returns>
        public ValueTask<Socket> AcceptAsync(Socket socket, CancellationToken token = default)
        {
            socketError = null;
            vtsAccept.Reset();
            completion = 0;
            this.token = token;
            pendingOperation = PendingOperation.Accept;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            AcceptSocket = null; // 由内核填充

            if (!socket.AcceptAsync(this))
            {
                ctr.Dispose();
                currentSocket = null;
                this.token = default;

                if (SocketError == SocketError.Success)
                {
                    var accepted = AcceptSocket!;
                    AcceptSocket = null;
                    return ValueTask.FromResult(accepted);
                }

                return ValueTask.FromException<Socket>(CreateSocketException(SocketError));
            }

            socketError = CreateSocketException(SocketError);
            return new ValueTask<Socket>(this, AcceptVersion);
        }

        #endregion Accept

        /// <summary>
        /// 构造统一的操作结果对象。仅在 <see
        /// cref="SocketError.Success"/> 时填充成功结果。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SocketOperationResult GetSocketOperationResult()
        {
            if (this.SocketError != SocketError.Success)
            {
                return new SocketOperationResult(false, default, default, socketError, default);
            }

            return new SocketOperationResult(true, BytesTransferred, RemoteEndPoint, null, ReceiveMessageFromPacketInfo);
        }

        #region Awaitable

        #region SocketOperationResult

        SocketOperationResult IValueTaskSource<SocketOperationResult>.GetResult(short token)
            => vts.GetResult(token);

        ValueTaskSourceStatus IValueTaskSource<SocketOperationResult>.GetStatus(short token)
            => vts.GetStatus(token);

        void IValueTaskSource<SocketOperationResult>.OnCompleted(
            Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => vts.OnCompleted(continuation, state, token, flags);

        #endregion SocketOperationResult

        #region SocketAccept

        Socket IValueTaskSource<Socket>.GetResult(short token)
            => vtsAccept.GetResult(token);

        ValueTaskSourceStatus IValueTaskSource<Socket>.GetStatus(short token)
            => vtsAccept.GetStatus(token);

        void IValueTaskSource<Socket>.OnCompleted(
            Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => vtsAccept.OnCompleted(continuation, state, token, flags);

        #endregion SocketAccept

        #endregion Awaitable

        /// <summary>
        /// 取消当前挂起的 I/O：确保仅完成一次，并关闭套接字以打断内核
        /// I/O，然后以 <see
        /// cref="OperationCanceledException"/> 完成。
        /// </summary>
        private void Cancel()
        {
            // 确保只由取消路径完成一次
            if (Interlocked.CompareExchange(ref completion, 2, 0) != 0)
                return;

            try { currentSocket?.Dispose(); } catch { /* 忽略 */ }
            currentSocket = null;

            ctr.Dispose();

            var oce = new OperationCanceledException(token);
            if (pendingOperation == PendingOperation.Accept)
                vtsAccept.SetException(oce);
            else
                vts.SetException(oce);

            pendingOperation = PendingOperation.None;
        }

        /// <summary>
        /// 根据 <see cref="SocketError"/> 构建对应的
        /// <see cref="SocketException"/>。
        /// </summary>
        protected static SocketException CreateSocketException(SocketError e)
        {
            return new SocketException((int)e);
        }
    }
}