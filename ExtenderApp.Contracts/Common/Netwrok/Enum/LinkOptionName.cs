namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 常用套接字选项名称枚举，用于在设置或获取套接字选项时指定具体选项。
    /// <para>枚举值对应底层协议/平台常量或本库的映射值，便于跨平台统一使用。</para>
    /// </summary>
    public enum LinkOptionName
    {
        /// <summary>
        /// 关闭套接字且不等待 lingering（优雅关闭但不延迟）。
        /// </summary>
        DontLinger = -129,

        /// <summary>
        /// 允许独占绑定（防止其它套接字绑定到相同地址/端口）。
        /// </summary>
        ExclusiveAddressUse = -5,

        /// <summary>
        /// 记录调试信息（IP 层调试选项）。
        /// </summary>
        Debug = 1,

        /// <summary>
        /// 指定插入到外发数据报的 IP 选项。
        /// </summary>
        IPOptions = 1,

        /// <summary>
        /// 发送 UDP 数据报时将校验和置零（覆盖校验行为）。
        /// </summary>
        NoChecksum = 1,

        /// <summary>
        /// 关闭 Nagle 算法以减少发送延迟（TCP_NODELAY）。
        /// </summary>
        NoDelay = 1,

        /// <summary>
        /// 指示套接字处于监听状态（用于被动接受连接）。
        /// </summary>
        AcceptConnection = 2,

        /// <summary>
        /// 使用 BSD 风格的紧急数据（仅能设置一次，之后无法关闭）。
        /// </summary>
        BsdUrgent = 2,

        /// <summary>
        /// 使用加速（expedited）数据（仅能设置一次，之后无法关闭）。
        /// </summary>
        Expedited = 2,

        /// <summary>
        /// 指示应用提供 IP 头部（HeaderIncluded）。
        /// </summary>
        HeaderIncluded = 2,

        /// <summary>
        /// TCP 空闲多长时间后开始发送 keepalive 探测（秒）。
        /// </summary>
        TcpKeepAliveTime = 3,

        /// <summary>
        /// 更改 IP 头部服务类型字段（Type of Service）。
        /// </summary>
        TypeOfService = 3,

        /// <summary>
        /// 设置 IP 头部的生存时间（TTL）。
        /// </summary>
        IpTimeToLive = 4,

        /// <summary>
        /// 允许地址重用（SO_REUSEADDR）。
        /// </summary>
        ReuseAddress = 4,

        /// <summary>
        /// 使用 TCP keep-alive 机制。
        /// </summary>
        KeepAlive = 8,

        /// <summary>
        /// 设置用于外发组播包的接口。
        /// </summary>
        MulticastInterface = 9,

        /// <summary>
        /// IP 组播生存时间（TTL）。
        /// </summary>
        MulticastTimeToLive = 10,

        /// <summary>
        /// IP 组播回环开关。
        /// </summary>
        MulticastLoopback = 11,

        /// <summary>
        /// 添加 IP 组播成员。
        /// </summary>
        AddMembership = 12,

        /// <summary>
        /// 移除 IP 组播成员。
        /// </summary>
        DropMembership = 13,

        /// <summary>
        /// 禁止对 IP 数据包进行分片。
        /// </summary>
        DontFragment = 14,

        /// <summary>
        /// 添加源组成员（源特定组播）。
        /// </summary>
        AddSourceMembership = 15,

        /// <summary>
        /// 指定路由行为：直接发送到接口地址（不经过常规路由）。
        /// </summary>
        DontRoute = 16,

        /// <summary>
        /// 移除源组成员。
        /// </summary>
        DropSourceMembership = 16,

        /// <summary>
        /// TCP keepalive 在终止连接前发送的重试次数。
        /// </summary>
        TcpKeepAliveRetryCount = 16,

        /// <summary>
        /// 屏蔽来自某源的数据（阻止来源）。
        /// </summary>
        BlockSource = 17,

        /// <summary>
        /// TCP keepalive 探测间隔（秒）。
        /// </summary>
        TcpKeepAliveInterval = 17,

        /// <summary>
        /// 解除对某源的屏蔽（允许来源）。
        /// </summary>
        UnblockSource = 18,

        /// <summary>
        /// 返回接收数据包的附加信息（例如接收地址/接口信息）。
        /// </summary>
        PacketInformation = 19,

        /// <summary>
        /// 设置或获取 UDP 校验范围（Checksum coverage）。
        /// </summary>
        ChecksumCoverage = 20,

        /// <summary>
        /// IPv6 中类似于 TTL 的跳数限制（Hop Limit）。
        /// </summary>
        HopLimit = 21,

        /// <summary>
        /// 限制 IPv6 套接字的作用范围（IPProtectionLevel）。
        /// </summary>
        IPProtectionLevel = 23,

        /// <summary>
        /// 指示 AF_INET6 套接字是否仅限于 IPv6 通信（IPv6Only）。
        /// </summary>
        IPv6Only = 27,

        /// <summary>
        /// 允许在套接字上发送广播消息。
        /// </summary>
        Broadcast = 32,

        /// <summary>
        /// 使用回环（绕过硬件）行为（实现相关）。
        /// </summary>
        UseLoopback = 64,

        /// <summary>
        /// 关闭时对未发送数据进行 linger（延迟关闭）控制。
        /// </summary>
        Linger = 128,

        /// <summary>
        /// 在正常数据流中接收带外数据（Out-of-band inline）。
        /// </summary>
        OutOfBandInline = 256,

        /// <summary>
        /// 指定套接字发送缓冲区大小（字节）。
        /// </summary>
        SendBuffer = 4097,

        /// <summary>
        /// 指定套接字接收缓冲区大小（字节）。
        /// </summary>
        ReceiveBuffer = 4098,

        /// <summary>
        /// 发送操作的低水位标志（发送队列阈值）。
        /// </summary>
        SendLowWater = 4099,

        /// <summary>
        /// 接收操作的低水位标志（接收队列阈值）。
        /// </summary>
        ReceiveLowWater = 4100,

        /// <summary>
        /// 同步发送超时（毫秒）。仅对同步方法有效，对异步方法无效。
        /// </summary>
        SendTimeout = 4101,

        /// <summary>
        /// 同步接收超时（毫秒）。仅对同步方法有效，对异步方法无效。
        /// </summary>
        ReceiveTimeout = 4102,

        /// <summary>
        /// 获取并清除套接字错误状态。
        /// </summary>
        Error = 4103,

        /// <summary>
        /// 获取套接字类型（如 Stream、Dgram 等）。
        /// </summary>
        Type = 4104,

        /// <summary>
        /// 延迟分配短暂端口（类似于 Winsock2 的 SO_REUSE_UNICASTPORT）。
        /// </summary>
        ReuseUnicastPort = 12295,

        /// <summary>
        /// 使用已存在套接字的属性更新被接受的套接字（SO_UPDATE_ACCEPT_CONTEXT）。
        /// </summary>
        UpdateAcceptContext = 28683,

        /// <summary>
        /// 使用已存在套接字的属性更新已连接套接字（SO_UPDATE_CONNECT_CONTEXT）。
        /// </summary>
        UpdateConnectContext = 28688,

        /// <summary>
        /// 不受支持的选项（将抛出 <see cref="System.Net.Sockets.SocketException"/>）。
        /// </summary>
        MaxConnections = int.MaxValue
    }
}