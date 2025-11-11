using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ExtenderApp.Common.Networks.Formatters;
using ExtenderApp.Common.Networks.LinkClients;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 网络扩展类，提供网络服务注册与常用地址计算辅助方法。
    /// </summary>
    internal static class NetworkExtensions
    {
        /// <summary>
        /// IPv4 地址字节长度（4 字节）。
        /// </summary>
        public const int IPv4BytesLength = 4;

        /// <summary>
        /// IPv6 地址字节长度（16 字节）。
        /// </summary>
        public const int IPv6BytesLength = 16;

        /// <summary>
        /// 向 DI 服务集合中注册网络相关组件。
        /// </summary>
        public static IServiceCollection AddNetwork(this IServiceCollection services)
        {
            services.AddFormatter();
            services.AddLinker();
            services.AddUdpLinker();
            services.AddLinkerClient();
            services.AddFileSegmenter();
            return services;
        }

        /// <summary>
        /// 计算网络地址（逐字节 AND 运算：IP &amp; Mask）。支持 IPv4 / IPv6。
        /// </summary>
        public static IPAddress? CalculateNetworkAddress(IPAddress ip, IPAddress subnetMask)
        {
            ArgumentNullException.ThrowIfNull(ip);
            ArgumentNullException.ThrowIfNull(subnetMask);
            if (ip.AddressFamily != subnetMask.AddressFamily)
                throw new ArgumentException("IP地址和子网掩码必须属于同一地址族");

            int length = ip.AddressFamily == AddressFamily.InterNetwork
                ? IPv4BytesLength
                : IPv6BytesLength;

            byte[] rented = ArrayPool<byte>.Shared.Rent(length * 3);
            Span<byte> ipSpan = rented.AsSpan(0, length);
            Span<byte> maskSpan = rented.AsSpan(length, length);

            try
            {
                if (!ip.TryWriteBytes(ipSpan, out _) || !subnetMask.TryWriteBytes(maskSpan, out _))
                    return null;

                Span<byte> resultSpan = rented.AsSpan(length * 2, length);
                for (int i = 0; i < length; i++)
                    resultSpan[i] = (byte)(ipSpan[i] & maskSpan[i]);

                return new IPAddress(resultSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        /// <summary>
        /// 计算 IPv4 广播地址：broadcast = ip | (~mask)。仅 IPv4 有广播概念。
        /// </summary>
        public static IPAddress? CalculateBroadcastAddressIPv4(IPAddress ip, IPAddress subnetMask)
        {
            ArgumentNullException.ThrowIfNull(ip);
            ArgumentNullException.ThrowIfNull(subnetMask);

            if (ip.AddressFamily != subnetMask.AddressFamily)
                throw new ArgumentException("IP地址和子网掩码必须属于同一地址族");
            if (ip.AddressFamily != AddressFamily.InterNetwork)
                throw new NotSupportedException("仅 IPv4 支持广播地址计算");

            byte[] rented = ArrayPool<byte>.Shared.Rent(IPv4BytesLength * 3);
            Span<byte> ipSpan = rented.AsSpan(0, IPv4BytesLength);
            Span<byte> maskSpan = rented.AsSpan(IPv4BytesLength, IPv4BytesLength);

            try
            {
                if (!ip.TryWriteBytes(ipSpan, out _) || !subnetMask.TryWriteBytes(maskSpan, out _))
                    return null;

                Span<byte> resultSpan = rented.AsSpan(IPv4BytesLength * 2, IPv4BytesLength);
                for (int i = 0; i < IPv4BytesLength; i++)
                    resultSpan[i] = (byte)(ipSpan[i] | (byte)~maskSpan[i]);

                return new IPAddress(resultSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        /// <summary>
        /// 局域网设备信息。
        /// </summary>
        internal sealed record LanHostInfo(
            IPAddress Ip,
            string? HostName,
            PhysicalAddress? Mac,
            bool IsSelf,
            string InterfaceId,
            int? InterfaceIndex);

        /// <summary>
        /// 扫描本机所有活动 IPv4 网卡所在子网，探测在线设备（Ping 成功即视为在线）。
        /// 可选获取主机名与 MAC（MAC 仅在 Windows 下使用 ARP）。
        /// </summary>
        /// <param name="timeoutMs">单个 Ping 超时时间（毫秒）。</param>
        /// <param name="parallelism">最大并发数（避免过高压垮网络/CPU）。</param>
        /// <param name="maxHostCountPerSubnet">单子网最大扫描主机数，防止 /8 等超大段全扫。</param>
        /// <param name="includeUnresponsive">是否包含未响应的地址（标记为离线）。</param>
        /// <param name="ct">取消令牌。</param>
        /// <returns>在线（及可选离线）设备列表。</returns>
        public static async Task<List<LanHostInfo>> ScanLanDevicesAsync(
            int timeoutMs = 250,
            int parallelism = 128,
            int maxHostCountPerSubnet = 2048,
            bool includeUnresponsive = false,
            CancellationToken ct = default)
        {
            var results = new ConcurrentBag<LanHostInfo>();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up
                           && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
                           && nic.Supports(NetworkInterfaceComponent.IPv4));

            // 记录本机所有绑定的 IPv4 地址用于 IsSelf 标记
            var selfIps = interfaces
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses
                    .Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(u => u.Address))
                .ToHashSet();

            var allTargets = new List<(IPAddress ip, string ifaceId, int? ifaceIndex)>();

            foreach (var nic in interfaces)
            {
                var ipProps = nic.GetIPProperties();
                var ipv4Props = ipProps.GetIPv4Properties();
                var index = ipv4Props?.Index;
                var ifaceId = nic.Id;

                foreach (var uni in ipProps.UnicastAddresses.Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    if (uni.IPv4Mask is null) continue;

                    var network = CalculateNetworkAddress(uni.Address, uni.IPv4Mask);
                    var broadcast = CalculateBroadcastAddressIPv4(uni.Address, uni.IPv4Mask);
                    if (network is null || broadcast is null) continue;

                    uint netValue = ToUInt32(network);
                    uint broadValue = ToUInt32(broadcast);

                    // 主机区间（排除网络、广播）
                    if (broadValue <= netValue + 1) continue;
                    uint start = netValue + 1;
                    uint end = broadValue - 1;
                    uint hostCount = end - start + 1;

                    if (hostCount > (uint)maxHostCountPerSubnet)
                        continue; // 跳过过大子网避免阻塞

                    for (uint v = start; v <= end; v++)
                    {
                        if (ct.IsCancellationRequested) break;
                        var ip = FromUInt32(v);
                        allTargets.Add((ip, ifaceId, index));
                    }
                }
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = parallelism,
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(allTargets, parallelOptions, async (entry, token) =>
            {
                using var ping = new Ping();
                PingReply? reply = null;
                try
                {
                    reply = await ping.SendPingAsync(entry.ip, timeoutMs);
                }
                catch
                {
                    // 忽略单个异常（ICMP禁用等）
                }

                bool isAlive = reply?.Status == IPStatus.Success;

                if (!isAlive && !includeUnresponsive)
                    return;

                string? hostName = null;
                if (isAlive)
                {
                    try
                    {
                        hostName = Dns.GetHostEntry(entry.ip).HostName;
                    }
                    catch
                    {
                        // 无主机名
                    }
                }

                PhysicalAddress? mac = null;
                if (isAlive)
                {
                    mac = TryGetMacAddress(entry.ip);
                }

                bool isSelf = selfIps.Contains(entry.ip);

                results.Add(new LanHostInfo(
                    entry.ip,
                    hostName,
                    mac,
                    isSelf,
                    entry.ifaceId,
                    entry.ifaceIndex));
            });

            return results
                .OrderBy(r => r.Ip.GetAddressBytes(), Comparer<byte[]>.Create((a, b) =>
                {
                    for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
                    {
                        int cmp = a[i].CompareTo(b[i]);
                        if (cmp != 0) return cmp;
                    }
                    return a.Length.CompareTo(b.Length);
                }))
                .ThenBy(r => r.InterfaceIndex ?? int.MaxValue)
                .ToList();
        }

        /// <summary>
        /// 将 IPv4 地址转换为 UInt32（网络序）。用于范围计算。
        /// </summary>
        private static uint ToUInt32(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// 从 UInt32 生成 IPv4 地址（网络序）。
        /// </summary>
        private static IPAddress FromUInt32(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return new IPAddress(bytes);
        }

        /// <summary>
        /// 尝试获取指定 IPv4 的 MAC 地址（仅本地子网有效；Windows 通过 SendARP）。
        /// 非 Windows 返回 null。
        /// </summary>
        private static PhysicalAddress? TryGetMacAddress(IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork)
                return null;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    byte[] macBytes = new byte[6];
                    int len = macBytes.Length;
                    // SendARP 需要目标 IP 的网络序整数
                    var addrBytes = ip.GetAddressBytes();
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(addrBytes);
                    int dest = BitConverter.ToInt32(addrBytes, 0);

                    int result = SendARP(dest, 0, macBytes, ref len);
                    if (result == 0 && len >= 6 && macBytes.Any(b => b != 0))
                        return new PhysicalAddress(macBytes.Take(6).ToArray());
                }
            }
            catch
            {
                // 忽略
            }
            return null;
        }

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int destIp, int srcIp, byte[] macAddr, ref int phyAddrLen);
    }
}