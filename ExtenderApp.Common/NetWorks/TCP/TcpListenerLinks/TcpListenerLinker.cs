using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data.File;

namespace ExtenderApp.Common.Networks
{
    public class TcpListenerLinker : ListenerLinker<TcpLinker>
    {
        private readonly ObjectPool<TcpLinker> _linkPool;

        public TcpListenerLinker(IBinaryParser binaryParser, SequencePool<byte> sequencePool) : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            _linkPool = ObjectPool.Create(new LinkPoolPolicy<TcpLinker>(binaryParser, sequencePool, (b, s) =>
            {
                return new TcpLinker(binaryParser, sequencePool);
            }), canReuse: false);
        }

        protected override TcpLinker CreateOperate(Socket clientSocket)
        {
            var result = _linkPool.Get();
            result.Set(clientSocket);
            return result;
        }
    }
}
