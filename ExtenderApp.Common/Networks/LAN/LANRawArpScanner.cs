using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ExtenderApp.Data;
using PacketDotNet;
using SharpPcap;

namespace ExtenderApp.Common.Networks.LAN
{
    /// <summary>
    /// 通过构造原始以太网 ARP 请求并捕获应答的扫描器（不依赖 SendARP）。
    /// 需要管理员权限与 Npcap/WinPcap 驱动支持。
    /// </summary>
    public sealed class LANRawArpScanner : IDisposable
    {
        private readonly ICaptureDevice _device;
        private readonly PhysicalAddress _localMac;
        private readonly IPAddress _localIp;
        private readonly IPAddress _mask;
        private readonly uint _network;
        private readonly uint _broadcast;
        private readonly int _hostCount;
        private BitFieldData _alive;
        private bool _disposed;

        public int HostCount => _hostCount;
        public int AliveCount => _alive.TrueCount;
        public double AlivePercent => _alive.PercentComplete;

        private LANRawArpScanner(ICaptureDevice dev,
                                 PhysicalAddress mac,
                                 IPAddress ip,
                                 IPAddress mask)
        {
            _device = dev;
            _localMac = mac;
            _localIp = ip;
            _mask = mask;

            uint ipHost = ToUIntHostOrder(ip);
            uint maskHost = ToUIntHostOrder(mask);
            _network = ipHost & maskHost;
            _broadcast = _network | ~maskHost;
            long possible = (long)_broadcast - _network - 1;
            if (possible <= 0 || possible > int.MaxValue)
                throw new ArgumentException("子网无有效主机范围。");

            _hostCount = (int)possible;
            _alive = new BitFieldData(_hostCount);

            _device.OnPacketArrival += OnPacket;
        }

        /// <summary>
        /// 从网络接口创建扫描器（选择指定索引或第一个满足条件的设备）。
        /// </summary>
        public static LANRawArpScanner? Create(int deviceIndex = 0)
        {
            var devices = CaptureDeviceList.Instance;
            if (devices.Count == 0) return null;
            if (deviceIndex < 0 || deviceIndex >= devices.Count) return null;

            var dev = devices[deviceIndex];
            dev.Open(DeviceModes.Promiscuous, 1000);

            // 通过 IP 属性匹配
            var ni = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n =>
                {
                    try
                    {
                        return dev.Name.Contains(n.Id, StringComparison.OrdinalIgnoreCase);
                    }
                    catch { return false; }
                });

            if (ni == null)
            {
                // 回退：遍历 unicast 找到与设备地址匹配
                var addr = dev.Addresses.FirstOrDefault(a => a.Addr?.ipAddress != null && a.Addr.ipAddress.AddressFamily == AddressFamily.InterNetwork);
                if (addr?.Addr?.ipAddress == null)
                    return null;

                var localIp = addr.Addr.ipAddress;
                var mask = addr.Netmask?.ipAddress;
                if (mask == null) return null;
                var macFallback = PhysicalAddress.None;
                var niCandidate = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.GetIPProperties().UnicastAddresses.Any(u => u.Address.Equals(localIp)));
                if (niCandidate != null)
                    macFallback = niCandidate.GetPhysicalAddress();

                return new LANRawArpScanner(dev, macFallback, localIp, mask);
            }

            var unicast = ni.GetIPProperties().UnicastAddresses
                .FirstOrDefault(u => u.Address.AddressFamily == AddressFamily.InterNetwork);
            if (unicast == null) return null;

            var mac = ni.GetPhysicalAddress();
            var local = unicast.Address;
            var maskIp = unicast.IPv4Mask;
            if (maskIp == null) return null;

            return new LANRawArpScanner(dev, mac, local, maskIp);
        }

        /// <summary>
        /// 异步执行 ARP 扫描，返回在线主机列表。
        /// </summary>
        public async Task<IReadOnlyList<IPAddress>> ScanAsync(int batchSize = 64, int delayPerBatchMs = 50, int timeoutMs = 2000, CancellationToken ct = default)
        {
            ResetAlive();

            var targets = EnumerateHosts().ToArray();
            int total = targets.Length;

            // 捕获启动
            _device.StartCapture();

            // 分批发送，减少突发流量
            for (int offset = 0; offset < total; offset += batchSize)
            {
                ct.ThrowIfCancellationRequested();
                int count = Math.Min(batchSize, total - offset);
                for (int i = 0; i < count; i++)
                {
                    SendArpRequest(targets[offset + i]);
                }
                await Task.Delay(delayPerBatchMs, ct);
            }

            // 等待应答窗口
            await Task.Delay(timeoutMs, ct);

            _device.StopCapture();

            return GetAliveSnapshot();
        }

        /// <summary>
        /// 发送单个 ARP Request。
        /// </summary>
        private void SendArpRequest(IPAddress target)
        {
            var ethernet = new EthernetPacket(_localMac, PhysicalAddress.Parse("FFFFFFFFFFFF"), EthernetType.Arp);

            var arp = new ARPPacket(ARPOperation.Request,
                                    PhysicalAddress.Parse("000000000000"),
                                    target,
                                    _localMac,
                                    _localIp);

            ethernet.PayloadPacket = arp;

            var bytes = ethernet.Bytes;
            _device.SendPacket(bytes);
        }

        /// <summary>
        /// 捕获回调：处理 ARP Reply。
        /// </summary>
        private void OnPacket(object sender, CaptureEventArgs e)
        {
            try
            {
                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                var eth = packet.Extract<EthernetPacket>();
                if (eth == null || eth.Type != EthernetType.Arp) return;

                var arp = packet.Extract<ARPPacket>();
                if (arp == null) return;
                if (arp.Operation != ARPOperation.Response) return;

                var senderIp = arp.SenderProtocolAddress;
                int idx = GetIndex(senderIp);
                if (idx >= 0)
                {
                    _alive.Set(idx);
                }
            }
            catch
            {
                // 忽略解析错误
            }
        }

        /// <summary>
        /// 枚举子网内所有可扫描主机（不含网络/广播）。
        /// </summary>
        public IEnumerable<IPAddress> EnumerateHosts()
        {
            for (uint host = _network + 1; host < _broadcast; host++)
            {
                yield return FromUIntHostOrder(host);
            }
        }

        public bool IsAlive(IPAddress ip)
        {
            int idx = GetIndex(ip);
            return idx >= 0 && _alive.Get(idx);
        }

        public int GetIndex(IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork) return -1;
            uint host = ToUIntHostOrder(ip);
            if (host <= _network || host >= _broadcast) return -1;
            return (int)(host - _network - 1);
        }

        public IPAddress GetAddress(int index)
        {
            if (index < 0 || index >= _hostCount) throw new ArgumentOutOfRangeException(nameof(index));
            uint host = _network + (uint)index + 1;
            return FromUIntHostOrder(host);
        }

        public void ResetAlive() => _alive.ClearAll();

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

        private static uint ToUIntHostOrder(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private static IPAddress FromUIntHostOrder(uint host)
        {
            var bytes = BitConverter.GetBytes(host);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return new IPAddress(bytes);
        }

        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                _device.OnPacketArrival -= OnPacket;
                if (_device.Started) _device.StopCapture();
                _device.Close();
            }
            catch { }
            _alive.Dispose();
            _disposed = true;
        }
    }
}