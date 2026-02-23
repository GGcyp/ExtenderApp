using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract.Networks
{
    /// <summary>
    /// 链路相关的可用选项标识集合。 用于集中定义与 ILinkInfo 相关的所有 OptionIdentifier，包含可见性设置。
    /// </summary>
    public static partial class LinkOptions
    {
        /// <summary>
        /// 接收缓冲区大小（字节）。
        /// </summary>
        public static readonly OptionIdentifier<int> ReceiveBufferSizeIdentifier = new(nameof(ILinkInfo.ReceiveBufferSize), 65536);

        /// <summary>
        /// 发送缓冲区大小（字节）。
        /// </summary>
        public static readonly OptionIdentifier<int> SendBufferSizeIdentifier = new(nameof(ILinkInfo.SendBufferSize), 65536);

        /// <summary>
        /// 接收超时时间（毫秒）。
        /// </summary>
        public static readonly OptionIdentifier<int> ReceiveTimeoutIdentifier = new(nameof(ILinkInfo.ReceiveTimeout), 0);

        /// <summary>
        /// 发送超时时间（毫秒）。
        /// </summary>
        public static readonly OptionIdentifier<int> SendTimeoutIdentifier = new(nameof(ILinkInfo.SendTimeout), 0);

        /// <summary>
        /// 套接字类型（例如 Stream、Dgram）。 设置可见性为 Protected，只有受保护/派生范围可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<SocketType> SocketTypeIdentifier = new(nameof(ILinkInfo.SocketType), setVisibility: OptionVisibility.Protected);

        /// <summary>
        /// 协议类型（例如 Tcp、Udp）。 设置可见性为 Protected，只有受保护/派生范围可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<ProtocolType> ProtocolTypeIdentifier = new(nameof(ILinkInfo.ProtocolType), setVisibility: OptionVisibility.Protected);

        /// <summary>
        /// 本地端点信息。 设置可见性为 Protected，只有受保护/派生范围可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<EndPoint> LocalEndPointIdentifier = new(nameof(ILinkInfo.LocalEndPoint), setVisibility: OptionVisibility.Protected);

        /// <summary>
        /// 远程端点信息。 设置可见性为 Protected，只有受保护/派生范围可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<EndPoint> RemoteEndPointIdentifier = new(nameof(ILinkInfo.RemoteEndPoint), setVisibility: OptionVisibility.Protected);

        /// <summary>
        /// 链路连接状态（已连接/未连接）。 设置可见性为 Protected，只有受保护/派生范围可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<bool> ConnectedIdentifier = new(nameof(ILinkInfo.Connected), setVisibility: OptionVisibility.Protected);

        /// <summary>
        /// 地址族（例如 InterNetwork, InterNetworkV6）。 设置可见性为 Protected，只有受保护/派生范围可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<AddressFamily> AddressFamilyIdentifier = new(nameof(ILinkInfo.AddressFamily), setVisibility: OptionVisibility.Protected);

        /// <summary>
        /// 容量限制器，用于限制链路的容量/并发等。 设置可见性为 Initial，仅在初始化阶段可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<CapacityLimiter> CapacityLimiterIdentifier = new(nameof(ILinkInfo.CapacityLimiter), setVisibility: OptionVisibility.Initial);

        /// <summary>
        /// 发送计数器，用于统计已发送数据量或次数。 设置可见性为 Initial，仅在初始化阶段可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<ValueCounter> SendCounterIdentifier = new(nameof(ILinkInfo.SendCounter), setVisibility: OptionVisibility.Initial);

        /// <summary>
        /// 接收计数器，用于统计已接收数据量或次数。 设置可见性为 Initial，仅在初始化阶段可设置该选项。
        /// </summary>
        public static readonly OptionIdentifier<ValueCounter> ReceiveCounterIdentifier = new(nameof(ILinkInfo.ReceiveCounter), setVisibility: OptionVisibility.Initial);
    }
}