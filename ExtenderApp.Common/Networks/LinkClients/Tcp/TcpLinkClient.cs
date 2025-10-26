using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// Tcp 链路客户端实现。
    /// </summary>
    internal class TcpLinkClient : LinkClientAwareSender<ITcpLinkClient, ITcpLinker>, ITcpLinkClient
    {
        public TcpLinkClient(ITcpLinker linker) : base(linker)
        {
        }
    }
}