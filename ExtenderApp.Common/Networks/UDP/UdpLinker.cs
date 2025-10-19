using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// UDP连接类，继承自Linker类，并实现IUdpLinker接口。
    /// </summary>
    internal class UdpLinker : SocketLinker, IUdpLinker
    {
        public override EndPoint? RemoteEndPoint => Socket.RemoteEndPoint;

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

        protected override ValueTask<SocketOperationResult> ExecuteReceiveAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token)
        {
            if (!Socket.Connected)
            {
                return ValueTask.FromResult(new SocketOperationResult(CreateSocketException(SocketError.NotConnected)));
            }
            return args.ReceiveAsync(Socket, memory, token);
        }

        protected override ValueTask<SocketOperationResult> ExecuteSendAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token)
        {
            if (!Socket.Connected)
            {
                return ValueTask.FromResult(new SocketOperationResult(CreateSocketException(SocketError.NotConnected)));
            }
            return args.SendAsync(Socket, memory, token);
        }

        public void Bind(EndPoint localEndPoint)
        {
            Socket.Bind(localEndPoint);
        }

        public SocketOperationResult SendTo(Memory<byte> memory, EndPoint endPoint)
        {
            if (memory.IsEmpty || memory.Length <= 0)
            {
                return new SocketOperationResult(CreateSocketException(SocketError.NoBufferSpaceAvailable));
            }
            if (endPoint == null)
            {
                return new SocketOperationResult(CreateSocketException(SocketError.NotConnected));
            }

            var args = GetArgs();
            try
            {
                return args.SendToAsync(Socket, memory, endPoint, default).GetAwaiter().GetResult();
            }
            finally
            {
                ReleaseArgs(args);
            }
        }

        public ValueTask<SocketOperationResult> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default)
        {
            if (memory.IsEmpty || memory.Length <= 0)
            {
                return ValueTask.FromResult(new SocketOperationResult(CreateSocketException(SocketError.NoBufferSpaceAvailable)));
            }
            if (endPoint == null)
            {
                return ValueTask.FromResult(new SocketOperationResult(CreateSocketException(SocketError.NotConnected)));
            }

            var args = GetArgs();
            try
            {
                return args.SendToAsync(Socket, memory, endPoint, token);
            }
            finally
            {
                ReleaseArgs(args);
            }
        }
    }
}