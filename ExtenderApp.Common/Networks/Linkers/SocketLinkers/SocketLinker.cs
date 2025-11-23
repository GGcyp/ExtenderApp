using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 套字节链接器的抽象基类。
    /// </summary>
    public abstract class SocketLinker : Linker, ILinker, ILinkBind
    {
        private static readonly ObjectPool<AwaitableSocketEventArgs> _pool
            = ObjectPool.CreateDefaultPool<AwaitableSocketEventArgs>();

        /// <summary>
        /// 当前链接器所使用的 Socket 实例。
        /// </summary>
        internal Socket Socket { get; }

        public SocketLinker(Socket socket)
        {
            Socket = socket;
        }

        public override bool Connected => Socket.Connected;

        public override EndPoint? LocalEndPoint => Socket.LocalEndPoint;

        public override EndPoint? RemoteEndPoint => Socket.RemoteEndPoint;

        public override SocketType SocketType => Socket.SocketType;

        public override ProtocolType ProtocolType => Socket.ProtocolType;

        public override AddressFamily AddressFamily => Socket.AddressFamily;

        /// <summary>
        /// 从对象池中获取 AwaitableSocketEventArgs 实例。
        /// </summary>
        /// <returns>
        /// AwaitableSocketEventArgs 实例
        /// </returns>
        protected AwaitableSocketEventArgs GetArgs() => _pool.Get();

        /// <summary>
        /// 向对象池中释放 AwaitableSocketEventArgs 实例。
        /// </summary>
        /// <param name="args">要被释放的实例</param>
        protected void ReleaseArgs(AwaitableSocketEventArgs args) => _pool.Release(args);

        public void Bind(EndPoint endPoint)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            try
            {
                Socket.Bind(endPoint);
            }
            finally
            {
                SendSlim.Release();
                ReceiveSlim.Release();
            }
        }

        protected override sealed ValueTask<SocketOperationResult> ExecuteSendAsync(Memory<byte> memory, CancellationToken token)
        {
            var args = _pool.Get();
            var vt = ExecuteSendAsync(args, memory, token);

            if (vt.IsCompletedSuccessfully)
            {
                try
                {
                    var result = vt.Result; // 同步完成，安全读取
                    _pool.Release(args);
                    return new ValueTask<SocketOperationResult>(result);
                }
                catch
                {
                    _pool.Release(args);
                    throw;
                }
            }

            return AwaitAndReleaseAsync(vt, args);
        }

        protected override sealed ValueTask<SocketOperationResult> ExecuteReceiveAsync(Memory<byte> memory, CancellationToken token)
        {
            var args = _pool.Get();
            var vt = ExecuteReceiveAsync(args, memory, token);

            if (vt.IsCompletedSuccessfully)
            {
                try
                {
                    var result = vt.Result;
                    _pool.Release(args);
                    return new ValueTask<SocketOperationResult>(result);
                }
                catch
                {
                    _pool.Release(args);
                    throw;
                }
            }

            return AwaitAndReleaseAsync(vt, args);
        }

        private async ValueTask<SocketOperationResult> AwaitAndReleaseAsync(ValueTask<SocketOperationResult> pending, AwaitableSocketEventArgs a)
        {
            try
            {
                return await pending.ConfigureAwait(false);
            }
            finally
            {
                _pool.Release(a);
            }
        }

        protected SocketException CreateSocketException(SocketError error)
        {
            return new SocketException((int)error);
        }

        /// <summary>
        /// 使用提供的 <paramref name="args"/> 执行一次底层发送逻辑（单次 I/O）。
        /// </summary>
        /// <param name="args">
        /// 已从对象池获取的可复用 <see cref="AwaitableSocketEventArgs"/>；实现须：
        /// 1. 在开始操作前调用其 SendAsync(…) 或 SendToAsync(…) 等方法；
        /// 2. 不自行释放/归还到对象池（由封装层负责释放）；
        /// 3. 不并发复用同一个实例（单次操作完成前不得再次使用）。
        /// </param>
        /// <param name="memory">
        /// 要发送的内存窗口。实现不得在操作未完成时修改或捕获其引用用于越界访问。对于 TCP 可能发生“部分发送”——返回
        /// BytesTransferred &lt; memory.Length 需由调用方循环补发。
        /// </param>
        /// <param name="token">
        /// 取消令牌。取消触发时实现应尽快终止 I/O（通常通过关闭套接字或调用 args 内部取消逻辑），使等待方得到
        /// <see cref="OperationCanceledException"/>。若已完成则忽略取消。
        /// </param>
        /// <returns>
        /// 表示发送结果的 <see cref="SocketOperationResult"/>：
        /// - 成功：BytesTransferred 为实际发送字节数，SocketError 为 null；
        /// - 失败：await 抛出 <see cref="SocketException"/>（由 args 的完成回调产生）；
        /// - 取消：await 抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        protected abstract ValueTask<SocketOperationResult> ExecuteSendAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token);

        /// <summary>
        /// 使用提供的 <paramref name="args"/> 执行一次底层接收逻辑（单次 I/O）。
        /// </summary>
        /// <param name="args">
        /// 已从对象池获取的 <see cref="AwaitableSocketEventArgs"/>：实现应调用其 ReceiveAsync/ReceiveFromAsync/ReceiveMessageFromAsync 等。
        /// 不得自行归还或在未完成时复用；完成后由基类封装释放。
        /// </param>
        /// <param name="memory">
        /// 可写缓冲区，用于接收数据；实现应确保内核写入后通过结果返回实际长度。对于 TCP 返回 0 通常表示对端优雅关闭。
        /// 对于 UDP 若发生截断，可在 SocketFlags 中带有 Truncated（由 args 内部状态体现，派生类应在结果中映射）。
        /// </param>
        /// <param name="token">
        /// 取消令牌；取消时应关闭套接字或触发 args 的取消流程，使 await 抛出 <see cref="OperationCanceledException"/>。
        /// </param>
        /// <returns>
        /// 表示接收结果的 <see cref="SocketOperationResult"/>：
        /// - 成功：BytesTransferred 为收到字节数，可能包含 RemoteEndPoint（UDP/ReceiveFrom）及 PacketInfo；
        /// - 对端关闭（TCP）：BytesTransferred == 0；
        /// - 失败：await 抛出 <see cref="SocketException"/>；
        /// - 取消：await 抛出 <see cref="OperationCanceledException"/>。
        /// </returns>
        protected abstract ValueTask<SocketOperationResult> ExecuteReceiveAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token);

        protected override ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token)
        {
            return Socket.ConnectAsync(remoteEndPoint, token);
        }

        public override ILinker Clone()
        {
            var socket = new Socket(AddressFamily, SocketType, ProtocolType);
            return Clone(socket);
        }

        protected abstract ILinker Clone(Socket socket);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            Socket.Dispose();
        }
    }
}