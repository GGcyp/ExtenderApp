using System.Buffers.Binary;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LAN
{
    /// <summary>
    /// 通过构造原始以太网 ARP 请求并捕获应答的扫描器（不依赖 SendARP）。
    /// 需要管理员权限与 Npcap/WinPcap 驱动支持。
    /// </summary>
    public class LANArpScanner : DisposableObject
    {
        // 以太网/ARP 常量
        private const int EthernetHeaderLength = 14;

        private const ushort EtherTypeArp = 0x0806;
        private const ushort ArpHTypeEthernet = 0x0001;
        private const ushort ArpPTypeIPv4 = 0x0800;
        private const byte ArpHLenMac = 6;
        private const byte ArpPLenIPv4 = 4;
        private const ushort ArpOpRequest = 0x0001;
        private static readonly byte[] BroadcastMacBytes = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        private static readonly PhysicalAddress BroadcastMac = new(BroadcastMacBytes);

        private readonly PhysicalAddress _localMac;
        private readonly IPAddress _localIp;
        private readonly IPAddress _mask;

        private readonly uint _network;   // 网络地址（网络序对应值）
        private readonly uint _broadcast; // 广播地址（网络序对应值）
        private readonly int _hostCount;
        private BitFieldData _alive;
        private readonly byte[] _localMacBytes;

        public event Action<LANHostInfo>? HostAlive;

        public LANArpScanner(PhysicalAddress localMac, IPAddress localIp, IPAddress mask)
        {
            ArgumentNullException.ThrowIfNull(localMac);
            ArgumentNullException.ThrowIfNull(localIp);
            ArgumentNullException.ThrowIfNull(mask);

            if (localIp.AddressFamily != AddressFamily.InterNetwork || mask.AddressFamily != AddressFamily.InterNetwork)
                throw new NotSupportedException("仅支持 IPv4 ARP。");
            if (localMac.GetAddressBytes().Length != ArpHLenMac)
                throw new ArgumentException("仅支持 6 字节以太网 MAC。", nameof(localMac));

            _localMac = localMac;
            _localIp = localIp;
            _mask = mask;
            _localMacBytes = _localMac.GetAddressBytes();

            // 子网参数计算
            uint ip32 = ToUInt32(localIp);
            uint mask32 = ToUInt32(mask);
            _network = ip32 & mask32;
            _broadcast = _network | ~mask32;

            long hosts = (long)_broadcast - (long)_network - 1; // 排除网络/广播
            _hostCount = hosts > 0 ? (int)hosts : 0;
            _alive = _hostCount > 0 ? new BitFieldData(_hostCount) : BitFieldData.Empty;
        }

        public void FindLANHost()
        {

        }

        public void WriteArpRequest(ref ByteBlock block, ValueIPAddress targetIp)
        {
            ArgumentNullException.ThrowIfNull(targetIp);
            if (targetIp.AddressFamily != AddressFamily.InterNetwork)
                throw new NotSupportedException("仅支持 IPv4 ARP。");

            // 可选：确保目标在本子网内（避免非必要校验可移除）
            uint tip = targetIp.ToUInt32();
            if (tip == _network || tip == _broadcast)
                return;

            // 以太网头部
            block.Write(BroadcastMacBytes);
            block.Write(_localMacBytes);
            block.Write(EtherTypeArp);

            // ARP payload
            block.Write(ArpHTypeEthernet);
            block.Write(ArpPTypeIPv4);
            block.Write(ArpHLenMac);
            block.Write(ArpPLenIPv4);
            block.Write(ArpOpRequest);

            // sender hardware address (sha)
            block.Write(_localMacBytes);

            // sender protocol address (spa)
            Span<byte> localIpBytes = block.GetSpan(4);
            _localIp.TryWriteBytes(localIpBytes, out int written);
            block.WriteAdvance(written);

            // target hardware address (tha) = 00:00:00:00:00:00
            block.WriteAdvance(6);

            // target protocol address (tpa)
            Span<byte> targetIpBytes = block.GetSpan(4);
            targetIp.TryWriteBytes(localIpBytes, out written);
            block.WriteAdvance(written);
        }

        private static uint ToUInt32(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();            // 网络序
            return BinaryPrimitives.ReadUInt32BigEndian(bytes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_alive.IsEmpty)
                    _alive.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}