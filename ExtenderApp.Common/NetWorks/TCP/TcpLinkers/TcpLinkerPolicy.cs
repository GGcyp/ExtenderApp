using System.Net.Sockets;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.NetWorks
{
    public class TcpLinkerPolicy : LinkOperatePolicy<TcpLinkerData>
    {
        private static readonly ObjectPool<TcpLinkerData> _pool
            = ObjectPool.Create(new FactoryPooledObjectPolicy<TcpLinkerData>(() => new TcpLinkerData(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)));

        public override TcpLinkerData GetData()
        {
            return _pool.Get();
        }

        public override void ReleaseData(TcpLinkerData data)
        {
            _pool.Release(data);
        }
    }
}
