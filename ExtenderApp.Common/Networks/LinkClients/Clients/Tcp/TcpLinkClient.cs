using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Networks;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// Tcp 链路客户端实现。
    /// </summary>
    public abstract class TcpLinkClient : LinkClient<ITcpLinker>, ITcpLinkClient
    {
        public bool NoDelay
        {
            get => Linker.NoDelay;
            set => SetOptionValue(LinkOptions.NoDelayIdentifier, value);
        }

        public TcpLinkClient(ITcpLinker linker) : base(linker)
        {
        }

        public abstract void Connect(IPAddress[] addresses, int port);

        public abstract ValueTask ConnectAsync(IPAddress[] addresses, int port, CancellationToken token = default);
    }
}