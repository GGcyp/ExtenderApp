using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal class UdpLinkClientFactory : LinkClientFactory<IUdpTransferLinkClient, IUdpLinker>
    {
        public UdpLinkClientFactory(ILinkerFactory<IUdpLinker> linkerFactory) : base(linkerFactory)
        {
        }

        protected override IUdpTransferLinkClient CreateLinkClient(IUdpLinker linker)
        {
            //return new UdpLinkClient(linker);
            return default!;
        }
    }
}