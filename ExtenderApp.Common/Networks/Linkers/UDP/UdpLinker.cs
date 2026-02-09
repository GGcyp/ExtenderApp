using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// UDP连接类，继承自Linker类，并实现IUdpLinker接口。
    /// </summary>
    internal class UdpLinker : SocketLinker, IUdpLinker
    {
        public UdpLinker(Socket socket) : base(socket)
        {
        }

        // 已连接UDP：设置默认远端（若未绑定会自动绑定本地端口）
        protected override ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token)
        {
            Socket.Connect(remoteEndPoint); // UDP下为快速同步调用
            return ValueTask.CompletedTask;
        }

        protected override ValueTask ExecuteDisconnectAsync(CancellationToken token)
        {
            try
            {
                // UDP下Disconnect清除默认远端
                if (Socket.Connected)
                    Socket.Disconnect(reuseSocket: false);
            }
            catch
            {
                // 某些平台/状态下可能抛异常，忽略即可
            }
            return ValueTask.CompletedTask;
        }

        public Result<SocketOperationValue> SendTo(Memory<byte> memory, EndPoint endPoint)
        {
            ThrowIfDisposed();
            var args = AwaitableSocketEventArgs.Get();
            return args.SendToAsync(Socket, memory, endPoint, default).GetAwaiter().GetResult();
        }

        public ValueTask<Result<SocketOperationValue>> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default)
        {
            ThrowIfDisposed();
            var args = AwaitableSocketEventArgs.Get();
            return args.SendToAsync(Socket, memory, endPoint, token);
        }

        protected override ILinker Clone(Socket socket)
        {
            return new UdpLinker(socket);
        }
    }
}