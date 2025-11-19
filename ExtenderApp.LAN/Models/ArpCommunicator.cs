using System.Collections.Concurrent;
using System.Formats.Tar;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace ExtenderApp.LAN.Networks.Arp
{
    public sealed class ArpEntry
    {
        public IPAddress IP { get; }
        public PhysicalAddress Mac { get; }
        public DateTime Timestamp { get; }

        public ArpEntry(IPAddress ip, PhysicalAddress mac)
        {
            IP = ip;
            Mac = mac;
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString() => $"{IP} -> {FormatMac(Mac)}";

        public static string FormatMac(PhysicalAddress mac)
            => string.Join(":", mac.GetAddressBytes().Select(b => b.ToString("X2")));
    }

    /// <summary>
    /// 负责通过 SharpPcap/PacketDotNet 发送与监听 ARP 数据包。
    /// 需要系统安装 Npcap/Libpcap，且通常需要管理员权限或特权。
    /// </summary>
    public sealed class ArpCommunicator : IDisposable
    {
        private ICaptureDevice? _device;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentDictionary<IPAddress, PhysicalAddress> _cache = new();
        private readonly object _openLock = new();

        public event EventHandler<ArpEntry>? ArpReplyReceived;

        public bool IsOpened => _device is not null;

        public IReadOnlyDictionary<IPAddress, PhysicalAddress> Cache => _cache;

        /// <summary>
        /// 打开指定索引的网卡（通过 CaptureDeviceList.Instance 枚举）。
        /// </summary>
        public void OpenDevice(int index)
        {
            lock (_openLock)
            {
                if (IsOpened)
                    throw new InvalidOperationException("设备已打开。");
                var devices = CaptureDeviceList.Instance;
                if (index < 0 || index >= devices.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), "设备索引无效。");

                _device = devices[index];
                _device.OnPacketArrival += Device_OnPacketArrival;
                _device.Open(DeviceModes.Promiscuous, readTimeoutMilliseconds: 1000);
            }
        }

        /// <summary>
        /// 自动选择首个包含 IPv4 的以太网设备。
        /// </summary>
        public void OpenDefaultDevice()
        {
            var devices = CaptureDeviceList.Instance;
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i] is LibPcapLiveDevice live)
                {
                    if (live.Interface.Addresses.Any(a => a.Addr?.ipAddress is IPAddress ip && ip.AddressFamily == AddressFamily.InterNetwork))
                    {
                        OpenDevice(i);
                        return;
                    }
                }
            }
            throw new InvalidOperationException("未找到可用的 IPv4 网卡。");
        }

        /// <summary>
        /// 开始后台监听 ARP 包。
        /// </summary>
        public void StartCapture()
        {
            if (_device is null)
                throw new InvalidOperationException("设备未打开。");
            if (_cts is not null)
                return;

            _cts = new CancellationTokenSource();
            _device.StartCapture();
        }

        /// <summary>
        /// 停止监听。
        /// </summary>
        public void StopCapture()
        {
            _cts?.Cancel();
            _cts = null;
            if (_device is { Capturing: true })
                _device.StopCapture();
        }

        /// <summary>
        /// 发送一个 ARP 请求，尝试解析目标 IP 的 MAC 地址。
        /// </summary>
        public PhysicalAddress? Resolve(IPAddress targetIp, TimeSpan timeout)
        {
            var localInfo = GetLocalInterfaceInfo();
            if (localInfo is null)
                throw new InvalidOperationException("无法获取本地接口信息。");

            if (_cache.TryGetValue(targetIp, out var mac))
                return mac;

            var tcs = new TaskCompletionSource<PhysicalAddress>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Handler(object? s, ArpEntry e)
            {
                if (e.IP.Equals(targetIp))
                {
                    tcs.TrySetResult(e.Mac);
                }
            }

            ArpReplyReceived += Handler;

            SendArpRequest(localInfo.Value.Mac, localInfo.Value.IP, targetIp);

            using var reg = new CancellationTokenSource(timeout);
            reg.Token.Register(() => tcs.TrySetCanceled());

            try
            {
                var result = tcs.Task.GetAwaiter().GetResult();
                _cache[targetIp] = result;
                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                ArpReplyReceived -= Handler;
            }
        }

        /// <summary>
        /// 异步解析目标 IP 的 MAC。
        /// </summary>
        public async Task<PhysicalAddress?> ResolveAsync(IPAddress targetIp, TimeSpan timeout, CancellationToken token = default)
        {
            var localInfo = GetLocalInterfaceInfo();
            if (localInfo is null)
                throw new InvalidOperationException("无法获取本地接口信息。");

            if (_cache.TryGetValue(targetIp, out var mac))
                return mac;

            var tcs = new TaskCompletionSource<PhysicalAddress>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Handler(object? s, ArpEntry e)
            {
                if (e.IP.Equals(targetIp))
                {
                    tcs.TrySetResult(e.Mac);
                }
            }

            ArpReplyReceived += Handler;

            SendArpRequest(localInfo.Value.Mac, localInfo.Value.IP, targetIp);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(timeout);
            try
            {
                var result = await tcs.Task.WaitAsync(cts.Token);
                _cache[targetIp] = result;
                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                ArpReplyReceived -= Handler;
            }
        }

        /// <summary>
        /// 构造并发送 ARP 请求（广播）。
        /// </summary>
        private void SendArpRequest(PhysicalAddress senderMac, IPAddress senderIp, IPAddress targetIp)
        {
            if (_device is null)
                throw new InvalidOperationException("设备未打开。");

            var broadcastMac = PhysicalAddress.Parse("FFFFFFFFFFFF");
            var unknownMac = new PhysicalAddress(new byte[] { 0, 0, 0, 0, 0, 0 });

            var arpPacket = new ArpPacket(
                ArpOperation.Request,
                senderMac,
                senderIp,
                unknownMac,
                targetIp
            );

            var ethernetPacket = new EthernetPacket(senderMac, broadcastMac, EthernetType.Arp)
            {
                PayloadPacket = arpPacket
            };

            var bytes = ethernetPacket.Bytes;
            _device.SendPacket(bytes);
        }

        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var raw = e.GetPacket();
                var packet = PacketDotNet.Packet.ParsePacket(raw.LinkLayerType, raw.Data);

                if (packet.Extract<EthernetPacket>() is not EthernetPacket eth)
                    return;
                if (eth.Type != EthernetType.Arp)
                    return;
                if (eth.PayloadPacket is not ArpPacket arp)
                    return;
                if (arp.Operation != ArpOperation.Response)
                    return;

                var ip = arp.SenderProtocolAddress;
                var mac = arp.SenderHardwareAddress;
                var entry = new ArpEntry(ip, mac);
                _cache[ip] = mac;
                ArpReplyReceived?.Invoke(this, entry);
            }
            catch
            {
                // 忽略解析异常
            }
        }

        /// <summary>
        /// 获取与捕获设备匹配的本地 IPv4 和 MAC。
        /// </summary>
        private (IPAddress IP, PhysicalAddress Mac)? GetLocalInterfaceInfo()
        {
            if (_device is not LibPcapLiveDevice live)
                return null;

            var ip = live.Interface.Addresses
                .Select(a => a.Addr?.ipAddress)
                .FirstOrDefault(p => p is IPAddress i && i.AddressFamily == AddressFamily.InterNetwork) as IPAddress;

            if (ip is null)
                return null;

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var props = nic.GetIPProperties();
                if (props.UnicastAddresses.Any(u => u.Address.Equals(ip)))
                {
                    return (ip, nic.GetPhysicalAddress());
                }
            }
            return null;
        }

        public void Dispose()
        {
            StopCapture();
            if (_device is not null)
            {
                _device.OnPacketArrival -= Device_OnPacketArrival;
                _device.Close();
                _device = null;
            }
            _cts?.Dispose();
        }
    }
}