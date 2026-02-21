using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 将基于事件的 <see cref="SocketAsyncEventArgs"/> I/O 封装为可 <c>await</c> 的 <see cref="ValueTask{TResult}"/> 模式。
    /// </summary>
    public class AwaitableSocketEventArgs : SocketAsyncEventArgs, IValueTaskSource<Result<LinkOperationValue>>, IValueTaskSource<Result<Socket>>
    {
        // 对象池
        private static readonly ObjectPool<AwaitableSocketEventArgs> _pool
            = ObjectPool.Create<AwaitableSocketEventArgs>();

        /// <summary>
        /// 从对象池获取一个实例。
        /// </summary>
        public static AwaitableSocketEventArgs Get() => _pool.Get();

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
        private ManualResetValueTaskSourceCore<Result<LinkOperationValue>> vts;

        /// <summary>
        /// 一次 Accept 操作的 awaitable 核心。
        /// </summary>
        private ManualResetValueTaskSourceCore<Result<Socket>> vtsAccept;

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
        /// 当前轮次的令牌版本号。构造 <see cref="ValueTask{TResult}"/> 时需携带该版本以确保一次性消费。
        /// </summary>
        public short Version => vts.Version;

        /// <summary>
        /// 当前轮次的 Accept 令牌版本号。
        /// </summary>
        public short AcceptVersion => vtsAccept.Version;

        /// <summary>
        /// Socket 操作完成回调：根据操作类型（收发或Accept）和结果（成功或失败）设置相应的 <see cref="ValueTask"/> 状态，并清理取消注册。
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

            if (SocketError == SocketError.Success)
            {
                if (pendingOperation == PendingOperation.Accept)
                {
                    vtsAccept.SetResult(Result.Success(AcceptSocket!));
                }
                else
                {
                    vts.SetResult(GetSocketOperationResult());
                }
            }
            else
            {
                socketError ??= CreateSocketException(SocketError);
                if (pendingOperation == PendingOperation.Accept)
                {
                    vtsAccept.SetResult(Result.FromException<Socket>(socketError));
                }
                else
                {
                    vts.SetException(socketError);
                }
            }
        }

        #region Send

        /// <summary>
        /// 发送指定缓冲区的数据（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">目标套接字（TCP 或已 Connect 的 UDP）。</param>
        /// <param name="memory">要发送的有效数据窗口。调用方需保证在本次异步发送完成前，该缓冲区保持有效且内容不被修改。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O（SAEA 不支持细粒度取消单个操作）。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> SendAsync(Socket socket, Memory<byte> memory, CancellationToken token = default)
        {
            return SendAsync(socket, memory, LinkFlags.None, token);
        }

        /// <summary>
        /// 发送指定缓冲区的数据（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">目标套接字（TCP 或已 Connect 的 UDP）。</param>
        /// <param name="memory">要发送的有效数据窗口。调用方需保证在本次异步发送完成前，该缓冲区保持有效且内容不被修改。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O（SAEA 不支持细粒度取消单个操作）。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> SendAsync(Socket socket, Memory<byte> memory, LinkFlags flags, CancellationToken token = default)
        {
            return SendAsync(socket, memory, (SocketFlags)flags, token);
        }

        /// <summary>
        /// 发送指定缓冲区的数据（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">目标套接字（TCP 或已 Connect 的 UDP）。</param>
        /// <param name="memory">要发送的有效数据窗口。调用方需保证在本次异步发送完成前，该缓冲区保持有效且内容不被修改。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O（SAEA 不支持细粒度取消单个操作）。</param>
        /// <param name="flags">发送标志，通常为 <see cref="SocketFlags.None"/>。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// - TCP 为字节流协议，可能发生“部分发送”；若需保证全部数据发出，请在调用方根据返回长度循环直至发送完毕。 <br/>
        /// - 对于 UDP，若套接字已 Connect 则可使用本方法；未 Connect 的 UDP 请使用 <see cref="SendToAsync(Socket, Memory{byte}, EndPoint, CancellationToken, SocketFlags)"/>。
        /// </remarks>
        public ValueTask<Result<LinkOperationValue>> SendAsync(Socket socket, Memory<byte> memory, SocketFlags flags, CancellationToken token = default)
        {
            ResetState();
            SocketFlags = flags;
            this.token = token;
            pendingOperation = PendingOperation.Send;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            BufferList = default;
            SetBuffer(memory);

            if (!socket.SendAsync(this))
            {
                OnCompleted(this);
            }

            return this;
        }

        /// <summary>
        /// 发送非连续的多个缓冲区数据（Scatter 发送）。
        /// </summary>
        /// <param name="socket">目标套接字。</param>
        /// <param name="bufferList">包含多个 <see cref="ArraySegment{T}"/> 的列表。SAEA 要求非连续内存必须使用该类型。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> SendAsync(Socket socket, IList<ArraySegment<byte>> bufferList, CancellationToken token = default)
        {
            return SendAsync(socket, bufferList, LinkFlags.None, token);
        }

        /// <summary>
        /// 发送非连续的多个缓冲区数据（Scatter 发送）。
        /// </summary>
        /// <param name="socket">目标套接字。</param>
        /// <param name="bufferList">包含多个 <see cref="ArraySegment{T}"/> 的列表。SAEA 要求非连续内存必须使用该类型。</param>
        /// <param name="token">取消令牌。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> SendAsync(Socket socket, IList<ArraySegment<byte>> bufferList, LinkFlags flags, CancellationToken token = default)
        {
            return SendAsync(socket, bufferList, (SocketFlags)flags, token);
        }

        /// <summary>
        /// 发送非连续的多个缓冲区数据（Scatter 发送）。
        /// </summary>
        /// <param name="socket">目标套接字。</param>
        /// <param name="bufferList">包含多个 <see cref="ArraySegment{T}"/> 的列表。SAEA 要求非连续内存必须使用该类型。</param>
        /// <param name="token">取消令牌。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> SendAsync(Socket socket, IList<ArraySegment<byte>> bufferList, SocketFlags flags, CancellationToken token = default)
        {
            ResetState();
            SocketFlags = flags;
            this.token = token;
            pendingOperation = PendingOperation.Send;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            // 设置 BufferList 会自动清除之前可能存在的单缓冲区
            BufferList = bufferList;
            SetBuffer(default);

            if (!socket.SendAsync(this))
            {
                OnCompleted(this);
            }

            return this;
        }

        /// <summary>
        /// UDP 发送到指定远端（未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect）。</param>
        /// <param name="buffer">要发送的数据缓冲。需在操作完成前保持有效且内容不被修改。</param>
        /// <param name="remoteEndPoint">目标远端地址；其 <see cref="EndPoint.AddressFamily"/> 必须与 <paramref name="socket"/> 一致。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// - 若要发送广播，请先设置 <c>socket.SetOptionValue(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true)</c>。 <br/>
        /// - 若套接字已 Connect，建议使用 <see cref="SendAsync(Socket, Memory{byte}, CancellationToken, SocketFlags)"/>。
        /// </remarks>
        public ValueTask<Result<LinkOperationValue>> SendToAsync(Socket socket, Memory<byte> buffer, EndPoint remoteEndPoint, CancellationToken token = default)
        {
            return SendToAsync(socket, buffer, remoteEndPoint, LinkFlags.None, token);
        }

        /// <summary>
        /// UDP 发送到指定远端（未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect）。</param>
        /// <param name="buffer">要发送的数据缓冲。需在操作完成前保持有效且内容不被修改。</param>
        /// <param name="remoteEndPoint">目标远端地址；其 <see cref="EndPoint.AddressFamily"/> 必须与 <paramref name="socket"/> 一致。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// - 若要发送广播，请先设置 <c>socket.SetOptionValue(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true)</c>。 <br/>
        /// - 若套接字已 Connect，建议使用 <see cref="SendAsync(Socket, Memory{byte}, CancellationToken, SocketFlags)"/>。
        /// </remarks>
        public ValueTask<Result<LinkOperationValue>> SendToAsync(Socket socket, Memory<byte> buffer, EndPoint remoteEndPoint, LinkFlags flags, CancellationToken token = default)
        {
            return SendToAsync(socket, buffer, remoteEndPoint, (SocketFlags)flags, token);
        }

        /// <summary>
        /// UDP 发送到指定远端（未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect）。</param>
        /// <param name="buffer">要发送的数据缓冲。需在操作完成前保持有效且内容不被修改。</param>
        /// <param name="remoteEndPoint">目标远端地址；其 <see cref="EndPoint.AddressFamily"/> 必须与 <paramref name="socket"/> 一致。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">发送标志。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// - 若要发送广播，请先设置 <c>socket.SetOptionValue(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true)</c>。 <br/>
        /// - 若套接字已 Connect，建议使用 <see cref="SendAsync(Socket, Memory{byte}, CancellationToken, SocketFlags)"/>。
        /// </remarks>
        public ValueTask<Result<LinkOperationValue>> SendToAsync(Socket socket, Memory<byte> buffer, EndPoint remoteEndPoint, SocketFlags flags, CancellationToken token = default)
        {
            ResetState();
            SocketFlags = flags;
            this.token = token;
            pendingOperation = PendingOperation.SendTo;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            RemoteEndPoint = remoteEndPoint;
            SetBuffer(buffer);

            if (!socket.SendToAsync(this))
            {
                OnCompleted(this);
            }

            return this;
        }

        /// <summary>
        /// UDP 发送到指定远端的多个缓冲区数据（Scatter 发送，未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字。</param>
        /// <param name="bufferList">包含多个 <see cref="ArraySegment{T}"/> 的列表。SAEA 要求非连续内存必须使用该类型。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        public ValueTask<Result<LinkOperationValue>> SendToAsync(Socket socket, IList<ArraySegment<byte>> bufferList, CancellationToken token = default)
        {
            return SendToAsync(socket, bufferList, LinkFlags.None, token);
        }

        /// <summary>
        /// UDP 发送到指定远端的多个缓冲区数据（Scatter 发送，未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字。</param>
        /// <param name="bufferList">包含多个 <see cref="ArraySegment{T}"/> 的列表。SAEA 要求非连续内存必须使用该类型。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">发送标志，通常为 <see cref="SocketFlags.None"/>。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        public ValueTask<Result<LinkOperationValue>> SendToAsync(Socket socket, IList<ArraySegment<byte>> bufferList, LinkFlags flags, CancellationToken token = default)
        {
            return SendToAsync(socket, bufferList, (SocketFlags)flags, token);
        }

        /// <summary>
        /// UDP 发送到指定远端的多个缓冲区数据（Scatter 发送，未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字。</param>
        /// <param name="bufferList">包含多个 <see cref="ArraySegment{T}"/> 的列表。SAEA 要求非连续内存必须使用该类型。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">发送标志，通常为 <see cref="SocketFlags.None"/>。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        public ValueTask<Result<LinkOperationValue>> SendToAsync(Socket socket, IList<ArraySegment<byte>> bufferList, SocketFlags flags, CancellationToken token = default)
        {
            ResetState();
            SocketFlags = flags;
            this.token = token;
            pendingOperation = PendingOperation.Send;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            // 设置 BufferList 会自动清除之前可能存在的单缓冲区
            BufferList = bufferList;
            SetBuffer(default);

            if (!socket.SendToAsync(this))
            {
                OnCompleted(this);
            }

            return this;
        }

        #endregion Send

        #region Receive

        /// <summary>
        /// 接收（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">源套接字（TCP 或已 Connect 的 UDP）。</param>
        /// <param name="buffer">可写缓冲区；内核将在其中写入接收的数据。调用方需保证缓冲在本次操作完成前保持有效。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// - TCP：返回字节数为 0 通常表示对端优雅关闭。 <br/>
        /// - UDP：若缓冲不足可能发生截断，可通过 <see cref="SocketAsyncEventArgs.SocketFlags"/> 检查 <see cref="SocketFlags.Truncated"/>。
        /// </remarks>
        public ValueTask<Result<LinkOperationValue>> ReceiveAsync(Socket socket, Memory<byte> buffer, CancellationToken token = default)
        {
            return ReceiveAsync(socket, buffer, SocketFlags.None, token);
        }

        /// <summary>
        /// 接收（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">源套接字（TCP 或已 Connect 的 UDP）。</param>
        /// <param name="buffer">可写缓冲区；内核将在其中写入接收的数据。调用方需保证缓冲在本次操作完成前保持有效。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> ReceiveAsync(Socket socket, Memory<byte> buffer, LinkFlags flags, CancellationToken token = default)
        {
            return ReceiveAsync(socket, buffer, (SocketFlags)flags, token);
        }

        /// <summary>
        /// 接收（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">源套接字（TCP 或已 Connect 的 UDP）。</param>
        /// <param name="buffer">可写缓冲区；内核将在其中写入接收的数据。调用方需保证缓冲在本次操作完成前保持有效。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> ReceiveAsync(Socket socket, Memory<byte> buffer, SocketFlags flags, CancellationToken token = default)
        {
            ResetState();
            SocketFlags = flags;
            this.token = token;
            pendingOperation = PendingOperation.Receive;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            SetBuffer(buffer);
            BufferList = null;

            if (!socket.ReceiveAsync(this))
            {
                OnCompleted(this);
            }

            return this;
        }

        /// <summary>
        /// 接收数据到非连续的多个缓冲区（Gather 接收）。
        /// </summary>
        /// <param name="socket">源套接字。</param>
        /// <param name="bufferList">用于存放接收数据的 <see cref="ArraySegment{T}"/> 列表。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> ReceiveAsync(Socket socket, IList<ArraySegment<byte>> bufferList, CancellationToken token = default)
        {
            return ReceiveAsync(socket, bufferList, SocketFlags.None, token);
        }

        /// <summary>
        /// 接收数据到非连续的多个缓冲区（Gather 接收）。
        /// </summary>
        /// <param name="socket">源套接字。</param>
        /// <param name="bufferList">用于存放接收数据的 <see cref="ArraySegment{T}"/> 列表。</param>
        /// <param name="token">取消令牌。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> ReceiveAsync(Socket socket, IList<ArraySegment<byte>> bufferList, LinkFlags flags, CancellationToken token = default)
        {
            return ReceiveAsync(socket, bufferList, (SocketFlags)flags, token);
        }

        /// <summary>
        /// 接收数据到非连续的多个缓冲区（Gather 接收）。
        /// </summary>
        /// <param name="socket">源套接字。</param>
        /// <param name="bufferList">用于存放接收数据的 <see cref="ArraySegment{T}"/> 列表。</param>
        /// <param name="token">取消令牌。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>操作结果的 <see cref="ValueTask"/>。</returns>
        public ValueTask<Result<LinkOperationValue>> ReceiveAsync(Socket socket, IList<ArraySegment<byte>> bufferList, SocketFlags flags, CancellationToken token = default)
        {
            ResetState();
            SocketFlags = flags;
            this.token = token;
            pendingOperation = PendingOperation.Receive;

            if (token.CanBeCanceled)
            {
                currentSocket = socket;
                ctr = token.UnsafeRegister(static s => ((AwaitableSocketEventArgs)s!).Cancel(), this);
            }

            BufferList = bufferList;
            SetBuffer(default);

            if (!socket.ReceiveAsync(this))
            {
                OnCompleted(this);
            }

            return this;
        }

        /// <summary>
        /// UDP 接收并获取来源地址（未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect）。</param>
        /// <param name="buffer">可写缓冲区；用于接收的数据存放。</param>
        /// <param name="anyEndPoint">
        /// 用于占位的 EndPoint，其 AddressFamily 必须与 <paramref name="socket"/> 一致（例如 new IPEndPoint(IPAddress.Any, 0)）。 完成后 <see
        /// cref="SocketAsyncEventArgs.RemoteEndPoint"/> 将被设置为实际来源地址。
        /// </param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>广播/组播可通过 <see cref="SocketAsyncEventArgs.SocketFlags"/> 检查 <see cref="SocketFlags.Broadcast"/> 与 <see cref="SocketFlags.Multicast"/>。</remarks>
        public ValueTask<Result<LinkOperationValue>> ReceiveFromAsync(Socket socket, Memory<byte> buffer, EndPoint anyEndPoint, CancellationToken token = default)
        {
            return ReceiveFromAsync(socket, buffer, anyEndPoint, SocketFlags.None, token);
        }

        /// <summary>
        /// UDP 接收并获取来源地址（未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect）。</param>
        /// <param name="buffer">可写缓冲区；用于接收的数据存放。</param>
        /// <param name="anyEndPoint">
        /// 用于占位的 EndPoint，其 AddressFamily 必须与 <paramref name="socket"/> 一致（例如 new IPEndPoint(IPAddress.Any, 0)）。 完成后 <see
        /// cref="SocketAsyncEventArgs.RemoteEndPoint"/> 将被设置为实际来源地址。
        /// </param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>广播/组播可通过 <see cref="SocketAsyncEventArgs.SocketFlags"/> 检查 <see cref="SocketFlags.Broadcast"/> 与 <see cref="SocketFlags.Multicast"/>。</remarks>
        public ValueTask<Result<LinkOperationValue>> ReceiveFromAsync(Socket socket, Memory<byte> buffer, EndPoint anyEndPoint, LinkFlags flags, CancellationToken token = default)
        {
            return ReceiveFromAsync(socket, buffer, anyEndPoint, (SocketFlags)flags, token);
        }

        /// <summary>
        /// UDP 接收并获取来源地址（未 Connect 的 UDP 使用）。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect）。</param>
        /// <param name="buffer">可写缓冲区；用于接收的数据存放。</param>
        /// <param name="anyEndPoint">
        /// 用于占位的 EndPoint，其 AddressFamily 必须与 <paramref name="socket"/> 一致。 完成后 <see cref="SocketAsyncEventArgs.RemoteEndPoint"/> 将被设置为实际来源地址。
        /// </param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        public ValueTask<Result<LinkOperationValue>> ReceiveFromAsync(Socket socket, Memory<byte> buffer, EndPoint anyEndPoint, SocketFlags flags, CancellationToken token = default)
        {
            ResetState();
            SocketFlags = flags;
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
                OnCompleted(this);
            }

            return this;
        }

        /// <summary>
        /// UDP 接收（带 IP 层报文信息与标志），用于识别 Truncated/Broadcast/Multicast 等。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect 或 Connect 均可）。</param>
        /// <param name="buffer">可写缓冲区；用于接收的数据存放。</param>
        /// <param name="anyEndPoint">
        /// 用于占位的 EndPoint，其 AddressFamily 必须与 <paramref name="socket"/> 一致。 完成后 <see cref="SocketAsyncEventArgs.RemoteEndPoint"/> 将被设置为实际来源地址。
        /// </param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// 要获取 <see cref="SocketAsyncEventArgs.ReceiveMessageFromPacketInfo"/>，需在套接字上启用数据报包信息： IPv4： <c>socket.SetOptionValue(SocketOptionLevel.IP,
        /// SocketOptionName.PacketInformation, true)</c>； IPv6： <c>socket.SetOptionValue(SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true)</c>。
        /// </remarks>
        public ValueTask<Result<LinkOperationValue>> ReceiveMessageFromAsync(Socket socket, Memory<byte> buffer, EndPoint anyEndPoint, CancellationToken token = default)
        {
            return ReceiveMessageFromAsync(socket, buffer, anyEndPoint, SocketFlags.None, token);
        }

        /// <summary>
        /// UDP 接收（带 IP 层报文信息与标志），用于识别 Truncated/Broadcast/Multicast 等。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect 或 Connect 均可）。</param>
        /// <param name="buffer">可写缓冲区；用于接收的数据存放。</param>
        /// <param name="anyEndPoint">
        /// 用于占位的 EndPoint，其 AddressFamily 必须与 <paramref name="socket"/> 一致。 完成后 <see cref="SocketAsyncEventArgs.RemoteEndPoint"/> 将被设置为实际来源地址。
        /// </param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        /// <remarks>
        /// 要获取 <see cref="SocketAsyncEventArgs.ReceiveMessageFromPacketInfo"/>，需在套接字上启用数据报包信息： IPv4： <c>socket.SetOptionValue(SocketOptionLevel.IP,
        /// SocketOptionName.PacketInformation, true)</c>； IPv6： <c>socket.SetOptionValue(SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true)</c>。
        /// </remarks>
        public ValueTask<Result<LinkOperationValue>> ReceiveMessageFromAsync(Socket socket, Memory<byte> buffer, EndPoint anyEndPoint, LinkFlags flags, CancellationToken token = default)
        {
            return ReceiveMessageFromAsync(socket, buffer, anyEndPoint, (SocketFlags)flags, token);
        }

        /// <summary>
        /// UDP 接收（带 IP 层报文信息与标志），用于识别 Truncated/Broadcast/Multicast 等。
        /// </summary>
        /// <param name="socket">UDP 套接字（未 Connect 或 Connect 均可）。</param>
        /// <param name="buffer">可写缓冲区；用于接收的数据存放。</param>
        /// <param name="anyEndPoint">
        /// 用于占位的 EndPoint，其 AddressFamily 必须与 <paramref name="socket"/> 一致。 完成后 <see cref="SocketAsyncEventArgs.RemoteEndPoint"/> 将被设置为实际来源地址。
        /// </param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 I/O。</param>
        /// <param name="flags">接收标志。</param>
        /// <returns>
        /// 一个表示操作结果的 <see cref="ValueTask"/>。成功时，其结果为包含 <see cref="LinkOperationValue"/> 的 <see cref="Result{T}"/>； 失败时 await 会抛出 <see
        /// cref="SocketException"/>；取消时则抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        public ValueTask<Result<LinkOperationValue>> ReceiveMessageFromAsync(Socket socket, Memory<byte> buffer, EndPoint anyEndPoint, SocketFlags flags, CancellationToken token = default)
        {
            ResetState();
            SocketFlags = flags;
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
                OnCompleted(this);
            }

            return this;
        }

        #endregion Receive

        #region Accept

        /// <summary>
        /// 等待 TCP 连接接入（awaitable，支持取消）。
        /// </summary>
        /// <param name="socket">处于监听状态的套接字。</param>
        /// <param name="token">取消令牌；取消时将关闭 <paramref name="socket"/> 以中断 Accept。</param>
        /// <returns>一个表示操作结果的 <see cref="Result{Socket}"/>。成功时包含已连接的 Socket，失败时包含异常信息。</returns>
        public ValueTask<Result<Socket>> AcceptAsync(Socket socket, CancellationToken token = default)
        {
            ResetState();
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
                OnCompleted(this);
            }

            return this;
        }

        #endregion Accept

        /// <summary>
        /// 根据当前 <see cref="SocketAsyncEventArgs"/> 的状态，构造一个 <see cref="Result{SocketOperationValue}"/>。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Result<LinkOperationValue> GetSocketOperationResult()
        {
            if (this.SocketError != SocketError.Success)
            {
                return Result.FromException<LinkOperationValue>(socketError!);
            }

            LinkOperationValue value = new LinkOperationValue(BytesTransferred, RemoteEndPoint, ReceiveMessageFromPacketInfo);
            return Result.Success(value);
        }

        #region Awaitable

        #region SocketOperationValue

        Result<LinkOperationValue> IValueTaskSource<Result<LinkOperationValue>>.GetResult(short token)
        {
            try
            {
                return vts.GetResult(token);
            }
            finally
            {
                ResetState();
                _pool.Release(this);
            }
        }

        ValueTaskSourceStatus IValueTaskSource<Result<LinkOperationValue>>.GetStatus(short token)
            => vts.GetStatus(token);

        void IValueTaskSource<Result<LinkOperationValue>>.OnCompleted(
            Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => vts.OnCompleted(continuation, state, token, flags);

        #endregion SocketOperationValue

        #region SocketAccept

        Result<Socket> IValueTaskSource<Result<Socket>>.GetResult(short token)
        {
            try
            {
                return vtsAccept.GetResult(token);
            }
            finally
            {
                ResetState();
                _pool.Release(this);
            }
        }

        ValueTaskSourceStatus IValueTaskSource<Result<Socket>>.GetStatus(short token)
            => vtsAccept.GetStatus(token);

        void IValueTaskSource<Result<Socket>>.OnCompleted(
            Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => vtsAccept.OnCompleted(continuation, state, token, flags);

        #endregion SocketAccept

        private void ResetState()
        {
            socketError = null;
            vts.Reset();
            vtsAccept.Reset();
            completion = 0;
            pendingOperation = PendingOperation.None;
            currentSocket = null;
            token = default;
            AcceptSocket = null;
            RemoteEndPoint = null;

            // 核心变动：必须显式清除 BufferList，因为 SAEA 中它是持久持有的 且设置 BufferList 为 null 是为了让后续 SetBuffer 调用有效（反之亦然）
            BufferList = null;
            SetBuffer(default);
        }

        #endregion Awaitable



        /// <summary>
        /// 取消当前挂起的 I/O：确保仅完成一次，并关闭套接字以打断内核 I/O，然后以 <see cref="OperationCanceledException"/> 完成。
        /// </summary>
        private void Cancel()
        {
            // 确保只由取消路径完成一次
            if (Interlocked.CompareExchange(ref completion, 2, 0) != 0)
                return;

            try { currentSocket?.Dispose(); } catch { /* 忽略 */ }

            ctr.Dispose();

            var oce = new OperationCanceledException(token);
            if (pendingOperation == PendingOperation.Accept)
                vtsAccept.SetResult(Result.FromException<Socket>(oce));
            else
                vts.SetException(oce);
        }

        /// <summary>
        /// 根据 <see cref="SocketError"/> 构建对应的 <see cref="SocketException"/>。
        /// </summary>
        protected static SocketException CreateSocketException(SocketError e)
        {
            return new SocketException((int)e);
        }

        public static implicit operator ValueTask<Result<LinkOperationValue>>(AwaitableSocketEventArgs args)
            => new ValueTask<Result<LinkOperationValue>>(args, args.Version);

        public static implicit operator ValueTask<Result<Socket>>(AwaitableSocketEventArgs args)
            => new ValueTask<Result<Socket>>(args, args.AcceptVersion);
    }
}