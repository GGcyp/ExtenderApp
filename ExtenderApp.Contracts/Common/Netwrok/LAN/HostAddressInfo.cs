namespace ExtenderApp.Contracts
{
    ///// <summary>
    ///// 主机地址信息，包含本机 IPv4 地址、子网掩码与物理地址（MAC）。
    ///// </summary>
    //public readonly struct HostAddressInfo : IEquatable<HostAddressInfo>, IDisposable
    //{
    //    /// <summary>
    //    /// 本机 IPv4 地址（sender protocol address, SPA）。
    //    /// </summary>
    //    public readonly ValueIPAddress IP;

    //    /// <summary>
    //    /// 子网掩码（可用于调用方进行网段校验，当前报文写入不使用）。
    //    /// </summary>
    //    public readonly ValueIPAddress Mask;

    //    /// <summary>
    //    /// 本机物理地址（sender hardware address, SHA）。
    //    /// </summary>
    //    public readonly ValuePhysicalAddress Mac;

    //    /// <summary>
    //    /// 如果本机 IP 地址为空，则表示该实例为空。
    //    /// </summary>
    //    public bool IsEmpty => IP.IsEmpty;

    //    public HostAddressInfo(ValueIPAddress ip) : this(ip, ValueIPAddress.None, ValuePhysicalAddress.None)
    //    {
    //    }

    //    public HostAddressInfo(ValueIPAddress ip, ValueIPAddress mask) : this(ip, mask, ValuePhysicalAddress.None)
    //    {
    //    }

    //    /// <summary>
    //    /// 使用本机 IP、目标 IP、子网掩码与本机 MAC 初始化 ARP 请求消息。
    //    /// </summary>
    //    /// <param name="IP">本机 IPv4 地址（SPA）。</param>
    //    /// <param name="targetIp">目标 IPv4 地址（TPA）。</param>
    //    /// <param name="mask">子网掩码（供外部校验，写入时不使用）。</param>
    //    /// <param name="mac">本机 MAC 地址（SHA）。</param>
    //    public HostAddressInfo(ValueIPAddress ip, ValueIPAddress mask, ValuePhysicalAddress mac)
    //    {
    //        IP = ip.IsEmpty ? throw new ArgumentNullException(nameof(ip)) : ip;
    //        Mask = mask;
    //        Mac = mac;
    //    }

    //    /// <summary>
    //    /// 将以太网广播 + ARP 请求（who-has）按顺序写入到 <paramref name="block"/> 末尾。
    //    /// </summary>
    //    /// <param name="block">目标写入缓冲，将追加 42 字节（Ethernet II + ARP）。</param>
    //    /// <exception cref="NotSupportedException">当目标地址非 IPv4 时抛出。</exception>
    //    /// <exception cref="ArgumentNullException">当目标地址未初始化时抛出。</exception>
    //    /// <remarks>
    //    /// 写入内容：
    //    /// - 以太网头：Dst=FF:FF:FF:FF:FF:FF，Src=本机MAC，Type=0x0806；<br/>
    //    /// - ARP 载荷：HTYPE=Ethernet，PTYPE=IPv4，HLEN=6，PLEN=4，OPER=Request；<br/>
    //    /// - SHA=本机MAC，SPA=本机IPv4，THA=全零(6)，TPA=目标IPv4。
    //    /// </remarks>
    //    public void CopyTo(ref ByteBlock block)
    //    {
    //        IP.CopyTo(ref block);
    //        Mask.CopyTo(ref block);
    //        Mac.CopyTo(ref block);
    //    }

    //    /// <summary>
    //    /// 释放内部基于 ArrayPool 的值类型资源（IP 与 MAC 的租借缓冲）。
    //    /// </summary>
    //    /// <remarks>
    //    /// 调用后本实例不应再被使用；多次调用不安全。
    //    /// </remarks>
    //    public void Dispose()
    //    {
    //        IP.Dispose();
    //        Mac.Dispose();
    //        Mask.Dispose();
    //    }

    //    public bool Equals(HostAddressInfo other)
    //    {
    //        if (IsEmpty && other.IsEmpty)
    //            return true;
    //        if (IsEmpty || other.IsEmpty)
    //            return false;

    //        return IP.Equals(other.IP)
    //            && Mask.Equals(other.Mask)
    //            && Mac.Equals(other.Mac);
    //    }

    //    public static bool operator ==(HostAddressInfo left, HostAddressInfo right) => left.Equals(right);
    //    public static bool operator !=(HostAddressInfo left, HostAddressInfo right) => !left.Equals(right);

    //    public override bool Equals(object? obj)
    //    {
    //        return obj is HostAddressInfo && Equals((HostAddressInfo)obj);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return HashCode.Combine(IP, Mask, Mac);
    //    }
    //}
}