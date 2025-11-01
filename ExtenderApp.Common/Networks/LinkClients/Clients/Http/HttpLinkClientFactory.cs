using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    internal class HttpLinkClientFactory : LinkClientFactory<IHttpLinkClient, ITcpLinker>
    {
        public HttpLinkClientFactory(ILinkerFactory<ITcpLinker> linkerFactory) : base(linkerFactory)
        {
        }

        protected override IHttpLinkClient CreateLinkClient(ITcpLinker linker)
        {
            return new HttpLinkClient(linker);
        }
    }
}
