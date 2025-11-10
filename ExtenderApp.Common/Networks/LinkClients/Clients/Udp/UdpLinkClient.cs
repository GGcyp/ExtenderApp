using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class UdpLinkClient : LinkClientAwareSender<IUdpLinkClient, IUdpLinker>, IUdpLinkClient
    {
        public UdpLinkClient(IUdpLinker linker) : base(linker)
        {
        }

        public void Bind(EndPoint endPoint)
        {
            Linker.Bind(endPoint);
        }

        public SocketOperationResult SendTo(Memory<byte> memory, EndPoint endPoint)
        {
            return Linker.SendTo(memory, endPoint);
        }

        public ValueTask<SocketOperationResult> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default)
        {
            return Linker.SendToAsync(memory, endPoint, token);
        }
    }
}