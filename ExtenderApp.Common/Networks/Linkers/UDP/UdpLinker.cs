using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Networks;
using ExtenderApp.Abstract.Options;
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
            RegisterOption(LinkOptions.DontFragmentIdentifier);
            RegisterOption(LinkOptions.EnableBroadcastIdentifier);
        }

        protected override void OnRegisterOption(OptionIdentifier identifier, OptionValue optionValue)
        {
            base.OnRegisterOption(identifier, optionValue);
            if (LinkOptions.DontFragmentIdentifier.TryBindChangedHandler(optionValue, static (o, item) => ((SocketLinker)o!).Socket.DontFragment = item.Item2))
                return;
            if (LinkOptions.EnableBroadcastIdentifier.TryBindChangedHandler(optionValue, static (o, item) => ((SocketLinker)o!).Socket.EnableBroadcast = item.Item2))
                return;
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