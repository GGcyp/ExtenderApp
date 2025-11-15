using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks.SNMP;
using ExtenderApp.Data;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private UdpClient _udpClient;

        public TestMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            LogInformation("开始测试");

            SnmpPdu pdu = new(SnmpPduType.GetRequest, 50);
            pdu.AddVarBind(new SnmpVarBind("1.3.6.1.2.1.1.1.0"));
            SnmpMessage message = new(pdu);
            ByteBlock block = new();
            message.BEREncode(ref block);
            bool s = SnmpExtensions.TryBERDecode(ref block, out SnmpMessage decodeMessage);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }

        /// <summary>
        /// 简单的局域网扫描器：Ping 扫描 + ARP 获取 MAC + DNS 反查主机名（Windows）
        /// 仅针对 IPv4 子网。
        /// </summary>
        public static class LanScanner
        {
            [DllImport("iphlpapi.dll", ExactSpelling = true)]
            private static extern int SendARP(uint destIp, uint srcIp, byte[] pMacAddr, ref uint phyAddrLen);

            /// <summary>
            /// 扫描本机所在的第一个有效 IPv4 子网，返回发现的主机信息（不包含本机自身）。
            /// </summary>
            /// <param name="pingTimeoutMs">单次 Ping 超时（毫秒）。</param>
            /// <param name="maxDegreeOfParallelism">并发数。</param>
            public static async Task<IReadOnlyList<LANHostInfo>> DiscoverLocalSubnetAsync(int pingTimeoutMs = 200, int maxDegreeOfParallelism = 200, CancellationToken cancellationToken = default)
            {
                var net = GetFirstIpv4Network();
                if (net is null)
                    return Array.Empty<LANHostInfo>();

                var (localIp, netAddr, broadcast) = net.Value;

                var start = IpToUint(netAddr) + 1;
                var end = IpToUint(broadcast) - 1;
                if (end < start)
                    return Array.Empty<LANHostInfo>();

                var results = new List<LANHostInfo>();
                var sem = new SemaphoreSlim(maxDegreeOfParallelism);

                var tasks = new List<Task>();
                for (uint u = start; u <= end; u++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var ip = UintToIp(u);
                    // skip local ip
                    if (ip.Equals(localIp)) continue;

                    await sem.WaitAsync(cancellationToken).ConfigureAwait(false);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (await IsHostAliveAsync(ip, pingTimeoutMs).ConfigureAwait(false))
                            {
                                var mac = TryGetMac(ip);
                                string? hostName = null;
                                try
                                {
                                    var entry = await Dns.GetHostEntryAsync(ip).ConfigureAwait(false);
                                    hostName = entry.HostName;
                                }
                                catch { /* ignore DNS errors */ }

                                lock (results)
                                {
                                    results.Add(new LANHostInfo(ip, hostName ?? string.Empty, mac));
                                }
                            }
                        }
                        finally
                        {
                            sem.Release();
                        }
                    }, cancellationToken));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
                // 去重（按 IP+MAC）
                return results.Distinct().ToList();
            }

            private static async Task<bool> IsHostAliveAsync(IPAddress ip, int timeoutMs)
            {
                try
                {
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync(ip, timeoutMs).ConfigureAwait(false);
                    return reply.Status == IPStatus.Success;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// 调用 SendARP 获取 MAC（Windows）。如果失败返回 PhysicalAddress.None。
            /// </summary>
            private static System.Net.NetworkInformation.PhysicalAddress TryGetMac(IPAddress ip)
            {
                try
                {
                    if (ip.AddressFamily != AddressFamily.InterNetwork)
                        return System.Net.NetworkInformation.PhysicalAddress.None;

                    var dest = BitConverter.ToUInt32(ip.GetAddressBytes(), 0);
                    uint macAddrLen = 6;
                    var macAddr = new byte[6];
                    var res = SendARP(dest, 0, macAddr, ref macAddrLen);
                    if (res != 0 || macAddrLen == 0)
                        return System.Net.NetworkInformation.PhysicalAddress.None;

                    return System.Net.NetworkInformation.PhysicalAddress.Parse(string.Join("", macAddr.Take((int)macAddrLen).Select(b => b.ToString("X2"))));
                }
                catch
                {
                    return System.Net.NetworkInformation.PhysicalAddress.None;
                }
            }

            /// <summary>
            /// 返回第一个可用的 IPv4 (本机IP, networkAddress, broadcastAddress)
            /// </summary>
            private static (IPAddress LocalIp, IPAddress Network, IPAddress Broadcast)? GetFirstIpv4Network()
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus != OperationalStatus.Up)
                        continue;
                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback || nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        continue;

                    var unicast = nic.GetIPProperties()?.UnicastAddresses?
                        .FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(u.Address));
                    if (unicast is null) continue;

                    var ip = unicast.Address;
                    var mask = unicast.IPv4Mask ?? IPAddress.Parse("255.255.255.0");

                    var ipUint = IpToUint(ip);
                    var maskUint = IpToUint(mask);
                    var netUint = ipUint & maskUint;
                    var broadcastUint = netUint | ~maskUint;

                    return (ip, UintToIp(netUint), UintToIp(broadcastUint));
                }
                return null;
            }

            private static uint IpToUint(IPAddress ip)
            {
                var bytes = ip.GetAddressBytes();
                // 保持 little-endian 平台兼容：BitConverter.ToUInt32 可直接用，但为了明确性使用 BinaryPrimitives
                return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
            }

            private static IPAddress UintToIp(uint u)
            {
                var bytes = new byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(bytes, u);
                return new IPAddress(bytes);
            }
        }
    }
}