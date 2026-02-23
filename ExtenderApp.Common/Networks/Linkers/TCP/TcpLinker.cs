using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Networks;
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
            get => GetOptionValue(LinkOptions.NoDelayIdentifier);
            set => SetOptionValue(LinkOptions.NoDelayIdentifier, value);
        }

        public TcpLinker(Socket socket) : base(socket)
        {
            RegisterOption(LinkOptions.NoDelayIdentifier, LinkOptions.NoDelayIdentifier.DefaultValue, static (o, item) => ((TcpLinker)o!).NoDelay = item.Item2);
        }

        public void Connect(IPAddress[] addresses, int port)
        {
            ConnectAsync(addresses, port).GetAwaiter().GetResult();
        }

        public async ValueTask ConnectAsync(IPAddress[] addresses, int port, CancellationToken token = default)
        {
            SendSlim.Wait();
            ReceiveSlim.Wait();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(addresses);
            if (IPEndPoint.MinPort > port || IPEndPoint.MaxPort < port)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "端口号超出有效范围。");
            }
            await Socket.ConnectAsync(addresses, port, token);
            Connected = true;
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