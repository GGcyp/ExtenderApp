using System.Buffers;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LAN
{
    /// <summary>
    /// 局域网扫描器：根据本机 IP / 子网掩码 / 网关生成可用主机位图，执行 ARP
    /// / ICMP 探测并标记在线主机。 使用 <see
    /// cref="BitFieldData"/> 保存“可用地址”与“在线状态”。
    /// </summary>
    public sealed class LANScanning : IDisposable
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(uint destIp, uint srcIp, byte[] pMacAddr, ref uint phyAddrLen);

        private readonly IPAddress _localIp;
        private readonly IPAddress _subnetMask;
        private readonly IPAddress _gateway;
        private readonly uint _networkAddress;     // 主机字节序
        private readonly uint _broadcastAddress;   // 主机字节序
        private readonly int _hostCount;           // 可扫描主机数量（去除网络/广播）
        private BitFieldData _aliveBits;           // 在线位图：1=在线
        private bool _disposed;

        /// <summary>
        /// 可扫描的总主机数（不含网络地址与广播地址）。
        /// </summary>
        public int HostCount => _hostCount;

        /// <summary>
        /// 已探测到在线的主机数量。
        /// </summary>
        public int AliveCount => _aliveBits.TrueCount;

        /// <summary>
        /// 在线百分比。
        /// </summary>
        public double AlivePercent => _aliveBits.PercentComplete;

        /// <summary>
        /// 创建扫描器。
        /// </summary>
        private LANScanning(IPAddress localIp, IPAddress subnetMask, IPAddress gateway)
        {
            _localIp = localIp ?? throw new ArgumentNullException(nameof(localIp));
            _subnetMask = subnetMask ?? throw new ArgumentNullException(nameof(subnetMask));
            _gateway = gateway ?? IPAddress.Any;

            if (_localIp.AddressFamily != AddressFamily.InterNetwork ||
                _subnetMask.AddressFamily != AddressFamily.InterNetwork)
                throw new NotSupportedException("仅支持 IPv4。");

            uint mask = ToHostOrder(_subnetMask);
            uint ip = ToHostOrder(_localIp);

            _networkAddress = ip & mask;
            _broadcastAddress = _networkAddress | ~mask;

            // 可用主机数量（排除网络 + 广播）
            long possible = (long)_broadcastAddress - _networkAddress - 1;
            if (possible <= 0 || possible > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(subnetMask), "子网过小或过大。");

            _hostCount = (int)possible;
            _aliveBits = new BitFieldData(_hostCount);
        }

        /// <summary>
        /// 从指定网卡创建扫描器（自动选取首个 IPv4 单播地址与其子网掩码、网关）。
        /// </summary>
        public static LANScanning? CreateFromInterface(NetworkInterface nic)
        {
            if (nic == null) return null;
            var props = nic.GetIPProperties();
            var uni = props.UnicastAddresses
                .FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork);
            if (uni == null) return null;
            var gateway = props.GatewayAddresses
                .FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork)?.Address
                ?? IPAddress.Any;

            return new LANScanning(uni.Address, uni.IPv4Mask!, gateway);
        }

        /// <summary>
        /// 枚举该子网中所有可扫描的主机地址（不含网络与广播）。
        /// </summary>
        public IEnumerable<IPAddress> EnumerateHosts()
        {
            for (uint host = _networkAddress + 1; host < _broadcastAddress; host++)
            {
                yield return FromHostOrder(host);
            }
        }

        /// <summary>
        /// 根据主机地址获取其在位图中的索引。不存在则返回 -1。
        /// </summary>
        public int GetIndex(IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork) return -1;
            uint host = ToHostOrder(ip);
            if (host <= _networkAddress || host >= _broadcastAddress) return -1;
            return (int)(host - _networkAddress - 1);
        }

        /// <summary>
        /// 获取位图索引对应的主机地址。
        /// </summary>
        public IPAddress GetAddress(int index)
        {
            if (index < 0 || index >= _hostCount) throw new ArgumentOutOfRangeException(nameof(index));
            uint host = _networkAddress + (uint)index + 1;
            return FromHostOrder(host);
        }

        /// <summary>
        /// 判断某地址是否被标记为在线。
        /// </summary>
        public bool IsAlive(IPAddress ip)
        {
            int idx = GetIndex(ip);
            return idx >= 0 && _aliveBits.Get(idx);
        }

        /// <summary>
        /// 执行一次扫描（ARP 或 ICMP 可选）。返回已标记在线的地址列表（快照）。
        /// </summary>
        /// <param name="useIcmp">
        /// 是否使用 ICMP Ping 辅助探测。
        /// </param>
        /// <param name="parallelism">并发度。</param>
        /// <param name="timeoutMs">
        /// 单次 ARP/Ping 超时（毫秒）。
        /// </param>
        /// <param name="ct">取消令牌。</param>
        public async Task<IReadOnlyList<IPAddress>> ScanAsync(
            bool useIcmp = false,
            int parallelism = 128,
            int timeoutMs = 300,
            CancellationToken ct = default)
        {
            var aliveAddresses = ArrayPool<IPAddress>.Shared.Rent(_hostCount);
            int aliveWrite = 0;

            using var sem = new SemaphoreSlim(parallelism);
            var tasks = new List<Task>(_hostCount);

            for (int i = 0; i < _hostCount; i++)
            {
                ct.ThrowIfCancellationRequested();
                int index = i;
                var ip = GetAddress(index);

                tasks.Add(Task.Run(async () =>
                {
                    await sem.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        bool ok = ProbeArp(ip) ||
                                  (useIcmp && await ProbeIcmpAsync(ip, timeoutMs).ConfigureAwait(false));
                        if (ok)
                        {
                            lock (_aliveBits)
                            {
                                _aliveBits.Set(index);
                                aliveAddresses[aliveWrite++] = ip;
                            }
                        }
                        else
                        {
                            lock (_aliveBits)
                            {
                                _aliveBits.Clear(index);
                            }
                        }
                    }
                    finally
                    {
                        sem.Release();
                    }
                }, ct));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var result = aliveAddresses.Take(aliveWrite).ToArray();
            ArrayPool<IPAddress>.Shared.Return(aliveAddresses, clearArray: false);
            return result;
        }

        /// <summary>
        /// 仅执行 ARP 探测（同步），适合快速刷新。
        /// </summary>
        public IReadOnlyList<IPAddress> ScanArpOnly()
        {
            var list = new List<IPAddress>();
            for (int i = 0; i < _hostCount; i++)
            {
                var ip = GetAddress(i);
                if (ProbeArp(ip))
                {
                    _aliveBits.Set(i);
                    list.Add(ip);
                }
                else
                {
                    _aliveBits.Clear(i);
                }
            }
            return list;
        }

        /// <summary>
        /// ARP 探测指定 IPv4 地址。
        /// </summary>
        private bool ProbeArp(IPAddress ip)
        {
            try
            {
                var addrBytes = ip.GetAddressBytes();
                if (BitConverter.IsLittleEndian) Array.Reverse(addrBytes);
                uint dest = BitConverter.ToUInt32(addrBytes, 0);

                uint macLen = 6;
                byte[] mac = new byte[6];
                int r = SendARP(dest, 0, mac, ref macLen);
                return r == 0 && macLen > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ICMP Ping 探测（异步）。
        /// </summary>
        private static async Task<bool> ProbeIcmpAsync(IPAddress ip, int timeoutMs)
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
        /// 清空所有在线标记。
        /// </summary>
        public void ResetAlive()
        {
            _aliveBits.ClearAll();
        }

        /// <summary>
        /// 获取当前在线地址快照。
        /// </summary>
        public IReadOnlyList<IPAddress> GetAliveSnapshot()
        {
            var list = new List<IPAddress>(_aliveBits.TrueCount);
            for (int i = 0; i < _hostCount; i++)
            {
                if (_aliveBits.Get(i))
                    list.Add(GetAddress(i));
            }
            return list;
        }

        private static uint ToHostOrder(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private static IPAddress FromHostOrder(uint host)
        {
            var bytes = BitConverter.GetBytes(host);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return new IPAddress(bytes);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _aliveBits.Dispose();
            _disposed = true;
        }
    }
}