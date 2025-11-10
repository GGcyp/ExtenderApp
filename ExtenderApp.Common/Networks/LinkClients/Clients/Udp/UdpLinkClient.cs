using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// Udp 链接客户端实现。
    /// </summary>
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

        public SocketOperationResult SendToAsync<T>(T value, EndPoint endPoint)
        {
            var sendBuffer = ValueToByteBuffer(value);
            return Linker.SendToAsync(sendBuffer, endPoint).GetAwaiter().GetResult();
        }

        public ValueTask<SocketOperationResult> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default)
        {
            return Linker.SendToAsync(memory, endPoint, token);
        }

        public ValueTask<SocketOperationResult> SendToAsync<T>(T value, EndPoint endPoint, CancellationToken token = default)
        {
            var sendBuffer = ValueToByteBuffer(value);
            return Linker.SendToAsync(sendBuffer, endPoint, token);
        }
    }
}