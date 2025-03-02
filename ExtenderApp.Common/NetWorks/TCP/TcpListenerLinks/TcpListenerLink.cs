using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Common.ObjectPools.Policy;

namespace ExtenderApp.Common.NetWorks.TCP
{
    internal class TcpListenerLink : ListenerLinkOperate<TcpListenerLink>
    {
        private readonly ObjectPool<TcpLink> _pool;

        public TcpListenerLink(IBinaryParser binaryParser, ISplitterParser splitterParser, LinkTypeStore store) : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            //_pool = ObjectPool.Create(new FactoryPooledObjectPolicy<TcpLink>(() =>
            //{
            //    return new TcpLink(splitterParser, binaryParser, store);
            //}));
        }

        protected override TcpListenerLink CreateOperate(Socket clientSocket)
        {
            throw new NotImplementedException();
        }
    }
}
