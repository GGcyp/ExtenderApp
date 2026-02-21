using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// Udp 链接客户端实现。
    /// </summary>
    internal class UdpLinkClient 
    {
        public UdpLinkClient(IUdpLinker linker)
        {
        }


        public Result<LinkOperationValue> SendToAsync<T>(T value, EndPoint endPoint)
        {
            //var sendBuffer = ValueToByteBuffer(value);
            //return Linker.SendToAsync(sendBuffer, endPoint).GetAwaiter().GetResult();
            return default;
        }

        public ValueTask<Result<LinkOperationValue>> SendToAsync<T>(T value, EndPoint endPoint, CancellationToken token = default)
        {
            //var sendBuffer = ValueToByteBuffer(value);
            //return Linker.SendToAsync(sendBuffer, endPoint, token);
            return default;
        }
    }
}