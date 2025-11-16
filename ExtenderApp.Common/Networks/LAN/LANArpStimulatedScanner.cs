using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LAN
{
    /// <summary>
    /// 不直接使用系统原生 ARP API、不依赖第三方库的“近似 ARP 扫描”实现：
    /// 通过向子网内每个 IPv4 地址发送 ICMP/UDP 探测刺激目标主机产生 ARP，
    /// 再解析系统命令 “arp -a” 输出获取 ARP 缓存，从而判断哪些主机在线。
    /// 受限：无法构造原始以太网帧，结果依赖操作系统是否填充 ARP 缓存。
    /// </summary>
    public sealed class LANArpStimulatedScanner : IDisposable
    {
        private readonly IPAddress _localIp;
        private readonly IPAddress _mask;
        private readonly uint _network;
        private readonly uint _broadcast;
        private readonly int _hostCount;
        private BitFieldData _alive;
        private bool _disposed;

        private readonly Regex _arpLineRegex = new(
            // 典型 Windows: "192.168.1.10       00-11-22-33-44-55     动态"
            @"^\s*(?<ip>\d{1,3}(?:\.\d{1,3}){3})\s+(?<mac>[0-9a-fA-F\-]{11,17})\s+(?<type>\S+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public int HostCount => _hostCount;
        public int AliveCount => _alive.TrueCount;
        public double AlivePercent => _alive.PercentComplete;

        private LANArpStimulatedScanner(IPAddress localIp, IPAddress mask)
        {
            _localIp = localIp;
            _mask = mask;

            if (_localIp.AddressFamily != AddressFamily.InterNetwork ||
                _mask.AddressFamily != AddressFamily.InterNetwork)
                throw new NotSupportedException("仅支持 IPv4。");

            uint ipHost = ToHostOrder(_localIp);
            uint maskHost = ToHostOrder(_mask);

            _network = ipHost & maskHost;
            _broadcast = _network | ~maskHost;

            long possible = (long)_broadcast - _network - 1;
            if (possible <= 0 || possible > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(mask), "子网范围无效。");

            _hostCount = (int)possible;
            _alive = new BitFieldData(_hostCount);
        }

        /// <summary>
        /// 基于网卡自动创建扫描器（选取首个 IPv4 单播地址）。
        /// </summary>
        public static LANArpStimulatedScanner? CreateFromInterface(NetworkInterface nic)
        {
            if (nic == null) return null;
            var uni = nic.GetIPProperties().UnicastAddresses
                .FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork);
            if (uni == null) return null;
            if (uni.IPv4Mask == null) return null;
            return new LANArpStimulatedScanner(uni.Address, uni.IPv4Mask);
        }

        /// <summary>
        /// 手动创建。
        /// </summary>
        public static LANArpStimulatedScanner Create(IPAddress localIp, IPAddress mask)
            => new LANArpStimulatedScanner(localIp, mask);

        /// <summary>
        /// 异步扫描：1) 发送刺激包 (ICMP+UDP) 2) 等待 3) 解析 ARP 缓存 4) 返回在线列表。
        /// </summary>
        public async Task<IReadOnlyList<IPAddress>> ScanAsync(
            bool useIcmp = true,
            bool useUdp = true,
            int parallelism = 128,
            int timeoutPerProbeMs = 250,
            int settleDelayMs = 600,
            CancellationToken ct = default)
        {
            ResetAlive();

            var targets = EnumerateHosts().ToArray();
            using var sem = new SemaphoreSlim(parallelism);
            var tasks = new List<Task>();

            foreach (var ip in targets)
            {
                ct.ThrowIfCancellationRequested();
                tasks.Add(Task.Run(async () =>
                {
                    await sem.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        if (useIcmp)
                            _ = ProbeIcmp(ip, timeoutPerProbeMs);
                        if (useUdp)
                            _ = ProbeUdp(ip);
                    }
                    finally
                    {
                        sem.Release();
                    }
                }, ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            // 等待内核将 ARP 缓存填充
            await Task.Delay(settleDelayMs, ct);

            var arpEntries = GetArpTableEntries();
            MarkAlive(arpEntries);

            return GetAliveSnapshot();
        }

        /// <summary>
        /// 仅刺激（不解析），用于分阶段处理。
        /// </summary>
        public async Task StimulateAsync(bool useIcmp = true, bool useUdp = true,
            int parallelism = 128, int timeoutPerProbeMs = 250, CancellationToken ct = default)
        {
            var targets = EnumerateHosts().ToArray();
            using var sem = new SemaphoreSlim(parallelism);
            var tasks = new List<Task>();
            foreach (var ip in targets)
            {
                ct.ThrowIfCancellationRequested();
                tasks.Add(Task.Run(async () =>
                {
                    await sem.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        if (useIcmp) _ = ProbeIcmp(ip, timeoutPerProbeMs);
                        if (useUdp) _ = ProbeUdp(ip);
                    }
                    finally { sem.Release(); }
                }, ct));
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// 解析当前 ARP 缓存并更新在线位图。
        /// </summary>
        public void RefreshFromArpCache()
        {
            var entries = GetArpTableEntries();
            MarkAlive(entries);
        }

        /// <summary>
        /// 获取在线快照。
        /// </summary>
        public IReadOnlyList<IPAddress> GetAliveSnapshot()
        {
            var list = new List<IPAddress>(_alive.TrueCount);
            for (int i = 0; i < _hostCount; i++)
            {
                if (_alive.Get(i))
                    list.Add(GetAddress(i));
            }
            return list;
        }

        /// <summary>
        /// 枚举子网所有可扫描主机（不含网络/广播）。
        /// </summary>
        public IEnumerable<IPAddress> EnumerateHosts()
        {
            for (uint host = _network + 1; host < _broadcast; host++)
                yield return FromHostOrder(host);
        }

        public bool IsAlive(IPAddress ip)
        {
            int idx = GetIndex(ip);
            return idx >= 0 && _alive.Get(idx);
        }

        public int GetIndex(IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork) return -1;
            uint host = ToHostOrder(ip);
            if (host <= _network || host >= _broadcast) return -1;
            return (int)(host - _network - 1);
        }

        public IPAddress GetAddress(int index)
        {
            if (index < 0 || index >= _hostCount) throw new ArgumentOutOfRangeException(nameof(index));
            uint host = _network + (uint)index + 1;
            return FromHostOrder(host);
        }

        public void ResetAlive() => _alive.ClearAll();

        // ---- 内部：探测与解析 ----

        private static async Task<bool> ProbeIcmp(IPAddress ip, int timeoutMs)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, timeoutMs).ConfigureAwait(false);
                return reply.Status == IPStatus.Success;
            }
            catch { return false; }
        }

        private static bool ProbeUdp(IPAddress ip)
        {
            // 向一个通常未开放的端口发送空数据，触发 ARP 解析
            try
            {
                using var udp = new UdpClient();
                udp.Client.SendTimeout = 50;
                udp.Client.ReceiveTimeout = 50;
                var ep = new IPEndPoint(ip, 65530); // 高端口
                byte[] buf = new byte[1] { 0x00 };
                udp.Send(buf, buf.Length, ep);
                return true;
            }
            catch { return false; }
        }

        private List<IPAddress> GetArpTableEntries()
        {
            var list = new List<IPAddress>();
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "arp",
                    Arguments = "-a",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc == null) return list;
                string? line;
                while ((line = proc.StandardOutput.ReadLine()) != null)
                {
                    var m = _arpLineRegex.Match(line);
                    if (!m.Success) continue;
                    var ipStr = m.Groups["ip"].Value;
                    if (IPAddress.TryParse(ipStr, out var ip))
                    {
                        // 过滤：只收集与本子网匹配的地址
                        int idx = GetIndex(ip);
                        if (idx >= 0) list.Add(ip);
                    }
                }
                proc.WaitForExit(1000);
            }
            catch
            {
                // 忽略解析失败
            }
            return list;
        }

        private void MarkAlive(IEnumerable<IPAddress> ips)
        {
            foreach (var ip in ips)
            {
                int idx = GetIndex(ip);
                if (idx >= 0)
                    _alive.Set(idx);
            }
        }

        private static uint ToHostOrder(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private static IPAddress FromHostOrder(uint host)
        {
            var bytes = BitConverter.GetBytes(host);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return new IPAddress(bytes);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _alive.Dispose();
            _disposed = true;
        }
    }
}