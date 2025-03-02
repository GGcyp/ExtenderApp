using System.Net.Sockets;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.NetWorks
{
    public class TcpLinkPolicy : LinkOperatePolicy<TcpLinkData>
    {
        private static readonly ObjectPool<TcpLinkData> _pool
            = ObjectPool.Create(new FactoryPooledObjectPolicy<TcpLinkData>(() => new TcpLinkData(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)));

        public override TcpLinkData GetData()
        {
            return _pool.Get();
        }

        public override void ReleaseData(TcpLinkData data)
        {
            _pool.Release(data);
        }
    }
}
