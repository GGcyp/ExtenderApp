using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    internal class TcpLinkClientFactory : LinkClientFactory<ITcpLinkClient, ITcpLinker>
    {
        public TcpLinkClientFactory(ILinkerFactory<ITcpLinker> linkerFactory) : base(linkerFactory)
        {
        }

        protected override ITcpLinkClient CreateLinkClient(ITcpLinker linker)
        {
            return new TcpLinkClient(linker);
        }
    }
}
