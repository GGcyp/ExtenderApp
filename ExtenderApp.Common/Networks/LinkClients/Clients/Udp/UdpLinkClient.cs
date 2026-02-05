using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// Udp 链接客户端实现。
    /// </summary>
    internal class UdpLinkClient : TransferLinkClient<IUdpLinker>, IUdpTransferLinkClient
    {
        public UdpLinkClient(IUdpLinker linker) : base(linker)
        {
        }

        public void Bind(EndPoint endPoint)
        {
            Linker.Bind(endPoint);
        }

        public Result<SocketOperationValue> SendToAsync<T>(T value, EndPoint endPoint)
        {
            var sendBuffer = ValueToByteBuffer(value);
            return Linker.SendToAsync(sendBuffer, endPoint).GetAwaiter().GetResult();
        }

        public ValueTask<Result<SocketOperationValue>> SendToAsync<T>(T value, EndPoint endPoint, CancellationToken token = default)
        {
            var sendBuffer = ValueToByteBuffer(value);
            return Linker.SendToAsync(sendBuffer, endPoint, token);
        }
    }
}