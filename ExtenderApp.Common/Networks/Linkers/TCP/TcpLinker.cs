using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;

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
            SendSlim.Wait();
            ReceiveSlim.Wait();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(addresses);
            if (IPEndPoint.MinPort > port || IPEndPoint.MaxPort < port)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "端口号超出有效范围。");
            }
            return Socket.ConnectAsync(addresses, port, token);
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