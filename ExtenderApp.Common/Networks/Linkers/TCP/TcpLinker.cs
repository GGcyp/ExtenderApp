using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// TcpLinker 类表示一个基于 TCP 协议的链接器。
    /// </summary>
    internal class TcpLinker : SocketLinker, ITcpLinker
    {
        public bool NoDelay
        {
            get => Socket.NoDelay;
            set => Socket.NoDelay = value;
        }

        public TcpLinker(Socket socket) : base(socket)
        {
        }

        public void Connect(IPAddress[] addresses, int port)
        {
            ConnectAsync(addresses, port).GetAwaiter().GetResult();
        }

        public ValueTask ConnectAsync(IPAddress[] addresses, int port, CancellationToken token = default)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(addresses);
            if (IPEndPoint.MinPort > port || IPEndPoint.MaxPort < port)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "端口号超出有效范围。");
            }
            return Socket.ConnectAsync(addresses, port, token);
        }

        protected override ValueTask ExecuteConnectAsync(EndPoint remoteEndPoint, CancellationToken token)
        {
            return Socket.ConnectAsync(remoteEndPoint, token);
        }

        protected override ValueTask ExecuteDisconnectAsync(CancellationToken token)
        {
            return Socket.DisconnectAsync(true, token);
        }

        protected override ValueTask<Result<SocketOperationValue>> ExecuteReceiveAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token)
        {
            return args.ReceiveAsync(Socket, memory, token);
        }

        protected override ValueTask<Result<SocketOperationValue>> ExecuteSendAsync(AwaitableSocketEventArgs args, Memory<byte> memory, CancellationToken token)
        {
            return args.SendAsync(Socket, memory, token);
        }

        protected override ILinker Clone(Socket socket)
        {
            var result = new TcpLinker(socket)
            {
                NoDelay = NoDelay
            };
            return result;
        }
    }
}