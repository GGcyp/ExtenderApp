using ExtenderApp.Abstract;
using ExtenderApp.Common.ConcurrentOperates;
using System.Net.Sockets;

namespace ExtenderApp.Common.NetWorks.LinkOperates
{
    public class LinkOperate<TPolicy, TData> : DisposableObject, IResettable
        where TPolicy : class, IConcurrentOperatePolicy<Socket, TData>
        where TData : ConcurrentOperateData
    {
        private ConcurrentOperate<TPolicy, Socket, TData> concurrentOperate;

        public bool TryReset()
        {
            return true;
        }
    }
}
