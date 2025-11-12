using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个局域网主机的基础信息（IP、主机名、MAC），
    /// 并提供 MAC 格式化与基于 IP+MAC 的相等性比较。
    /// </summary>
    [DebuggerDisplay("PhysicalAddress = {MacAddress}, IP = {Address}, Host = {HostName}")]
    internal class LANHostInfo : IEquatable<LANHostInfo>
    {
        public static LANHostInfo Empty { get; } = new LANHostInfo(IPAddress.Loopback);

        /// <summary>
        /// 获取主机的物理地址（MAC）。若未知则为 <see cref="PhysicalAddress.None"/>。
        /// </summary>
        public PhysicalAddress MacAddress { get; }

        /// <summary>
        /// 获取主机的 IP 地址。若未知则为 <see cref="IPAddress.None"/>。
        /// </summary>
        public IPAddress Address { get; }

        /// <summary>
        /// 获取主机名（通常来自 DNS 反查）。不可用时为 null 或空字符串。
        /// </summary>
        public string? HostName { get; }

        /// <summary>
        /// 指示 <see cref="Address"/> 是否为 IPv4。
        /// </summary>
        public bool IsIPv4 => Address.AddressFamily == AddressFamily.InterNetwork;

        /// <summary>
        /// 指示 <see cref="Address"/> 是否为 IPv6。
        /// </summary>
        public bool IsIPv6 => Address.AddressFamily == AddressFamily.InterNetworkV6;

        /// <summary>
        /// 指示是否存在有效的 MAC 地址。
        /// </summary>
        public bool HasMac => !MacAddress.Equals(PhysicalAddress.None);

        /// <summary>
        /// 指示该实例是否为“空”主机（无 IP、无 MAC、无主机名）。
        /// </summary>
        public bool IsEmpty => Address.Equals(IPAddress.None) &&
            MacAddress.Equals(PhysicalAddress.None) &&
            string.IsNullOrEmpty(HostName);

        /// <summary>
        /// 初始化一个空实例：
        /// <see cref="Address"/> = <see cref="IPAddress.None"/>，
        /// <see cref="HostName"/> = 空字符串，
        /// <see cref="MacAddress"/> = <see cref="PhysicalAddress.None"/>。
        /// </summary>
        public LANHostInfo(IPAddress address) : this(address, string.Empty, PhysicalAddress.None)
        {
        }

        /// <summary>
        /// 使用指定的 IP、主机名与 MAC 初始化实例。
        /// </summary>
        /// <param name="address">主机 IP 地址，不能为空。</param>
        /// <param name="hostName">主机名，可为 null 或空。</param>
        /// <param name="macAddress">物理地址，null 时回退为 <see cref="PhysicalAddress.None"/>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> 为 null 时抛出。</exception>
        public LANHostInfo(IPAddress address, string? hostName, PhysicalAddress? macAddress)
        {
            ArgumentNullException.ThrowIfNull(address);
            if (address.Equals(IPAddress.None))
                throw new ArgumentException("局域网内设备的地址不能为None", nameof(address));
            Address = address;
            HostName = hostName;
            MacAddress = macAddress ?? PhysicalAddress.None;
        }

        /// <summary>
        /// 以指定分隔符格式化 MAC 地址字符串。
        /// </summary>
        /// <param name="separator">字节间分隔符。为空或空字符串时，返回不含分隔符的大写十六进制。</param>
        /// <returns>格式化后的 MAC 字符串；若无有效 MAC 则返回空字符串。</returns>
        public string GetMacString(string separator = "")
        {
            if (!HasMac)
                return string.Empty;
            if (string.IsNullOrEmpty(separator))
                return MacAddress.ToString();

            var bytes = MacAddress.GetAddressBytes();
            return string.Join(separator, bytes.Select(b => b.ToString("X2")));
        }

        /// <summary>
        /// 返回便于日志与调试的字符串表示，形如：
        /// "192.168.1.10 (host) [00:11:22:33:44:55]"。
        /// </summary>
        public override string ToString()
        {
            var mac = GetMacString(":");
            var host = string.IsNullOrWhiteSpace(HostName) ? "<unknown>" : HostName;
            return $"{Address} ({host}){(string.IsNullOrEmpty(mac) ? "" : $" [{mac}]")}";
        }

        /// <summary>
        /// 判断与另一实例是否相等（按 IP 与 MAC 比较，主机名不参与）。
        /// </summary>
        public bool Equals(LANHostInfo? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null) return false;

            // 以 IP 和 MAC 为主进行相等性判断（HostName 常为可变信息）
            if (!Address.Equals(other.Address)) return false;

            return MacAddress.Equals(other.MacAddress);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is LANHostInfo other && Equals(other);

        /// <summary>
        /// 获取哈希码（与 <see cref="Equals(LANHostInfo?)"/> 一致，基于 IP + MAC）。
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Address, MacAddress);
        }

        /// <summary>
        /// 相等运算符，等价于 <see cref="Equals(LANHostInfo?)"/>。
        /// </summary>
        public static bool operator ==(LANHostInfo? left, LANHostInfo? right) => Equals(left, right);

        /// <summary>
        /// 不相等运算符。
        /// </summary>
        public static bool operator !=(LANHostInfo? left, LANHostInfo? right) => !Equals(left, right);
    }
}