using System.Net.Sockets;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Networks
{
    public class TcpLinkerPolicy : LinkOperatePolicy<TcpLinkerData>
    {
        private static readonly ObjectPool<TcpLinkerData> _pool = ObjectPool.CreateDefaultPool<TcpLinkerData>();

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
