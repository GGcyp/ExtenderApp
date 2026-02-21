using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    public static class LinkOptions
    {
        public static readonly OptionIdentifier<int> ReceiveBufferSizeIdentifier = new(nameof(ILinkInfo.ReceiveBufferSize));
        public static readonly OptionIdentifier<int> SendBufferSizeIdentifier = new(nameof(ILinkInfo.SendBufferSize));
        public static readonly OptionIdentifier<int> ReceiveTimeoutIdentifier = new(nameof(ILinkInfo.ReceiveTimeout));
        public static readonly OptionIdentifier<int> SendTimeoutIdentifier = new(nameof(ILinkInfo.SendTimeout));

        public static readonly OptionIdentifier<SocketType> SocketTypeIdentifier = new(nameof(ILinkInfo.SocketType), setVisibility: OptionVisibility.Protected);
        public static readonly OptionIdentifier<ProtocolType> ProtocolTypeIdentifier = new(nameof(ILinkInfo.ProtocolType), setVisibility: OptionVisibility.Protected);

        public static readonly OptionIdentifier<EndPoint> LocalEndPointIdentifier = new(nameof(ILinkInfo.LocalEndPoint), setVisibility: OptionVisibility.Protected);
        public static readonly OptionIdentifier<EndPoint> RemoteEndPointIdentifier = new(nameof(ILinkInfo.RemoteEndPoint), setVisibility: OptionVisibility.Protected);

        public static readonly OptionIdentifier<bool> ConnectedIdentifier = new(nameof(ILinkInfo.Connected), setVisibility: OptionVisibility.Protected);

        public static readonly OptionIdentifier<AddressFamily> AddressFamilyIdentifier = new(nameof(ILinkInfo.AddressFamily), setVisibility: OptionVisibility.Protected);

        public static readonly OptionIdentifier<CapacityLimiter> CapacityLimiterIdentifier = new(nameof(ILinkInfo.CapacityLimiter), setVisibility: OptionVisibility.Internal);
        public static readonly OptionIdentifier<ValueCounter> SendCounterIdentifier = new(nameof(ILinkInfo.SendCounter), setVisibility: OptionVisibility.Internal);
        public static readonly OptionIdentifier<ValueCounter> ReceiveCounterIdentifier = new(nameof(ILinkInfo.ReceiveCounter), setVisibility: OptionVisibility.Internal);

        //public static readonly OptionIdentifier<bool> NoDelayIdentifier = new(nameof(ILinkInfo.NoDelay));
    }
}