using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// UDP连接类，继承自Linker类，并实现IUdpLinker接口。
    /// </summary>
    internal class UdpLinker : SocketLinker, IUdpLinker
    {
        public UdpLinker(Socket socket) : base(socket)
        {
        }

        public Result<LinkOperationValue> SendTo(Memory<byte> memory, EndPoint endPoint)
        {
            ThrowIfDisposed();
            var args = AwaitableSocketEventArgs.Get();
            return args.SendToAsync(Socket, memory, endPoint, default).GetAwaiter().GetResult();
        }

        public ValueTask<Result<LinkOperationValue>> SendToAsync(Memory<byte> memory, EndPoint endPoint, CancellationToken token = default)
        {
            ThrowIfDisposed();
            var args = AwaitableSocketEventArgs.Get();
            return args.SendToAsync(Socket, memory, endPoint, token);
        }

        protected override ILinker Clone(Socket socket)
        {
            return new UdpLinker(socket);
        }
    }
}