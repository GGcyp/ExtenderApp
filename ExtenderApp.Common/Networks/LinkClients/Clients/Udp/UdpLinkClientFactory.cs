using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal class UdpLinkClientFactory : LinkClientFactory<IUdpLinkClient, IUdpLinker>
    {
        public UdpLinkClientFactory(ILinkerFactory<IUdpLinker> linkerFactory) : base(linkerFactory)
        {
        }

        protected override IUdpLinkClient CreateLinkClient(IUdpLinker linker)
        {
            return new UdpLinkClient(linker);
        }
    }
}