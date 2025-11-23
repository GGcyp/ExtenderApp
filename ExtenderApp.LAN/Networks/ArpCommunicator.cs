using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;

namespace ExtenderApp.LAN
{
    /// <summary>
    /// 负责通过 SharpPcap/PacketDotNet 发送与监听 ARP 数据包。
    /// 需要系统安装 Npcap/Libpcap，且通常需要管理员权限或特权。
    /// </summary>
    public class ArpCommunicator : Communicator<ArpPacket>
    {
        private static readonly PhysicalAddress NoneMacAddress = PhysicalAddress.Parse("00-00-00-00-00-00");
        private static readonly PhysicalAddress BroadcastMacAddress = PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF");
        protected override string Filter => "arp";

        protected override EthernetType EthernetType => EthernetType.Arp;

        private readonly IPAddress _localIpAddress;

        public override PhysicalAddress DestinationHardwareAddress
        {
            get => base.DestinationHardwareAddress;
            set => throw new Exception("ARP协议无法修改目标地址");
        }

        public ArpCommunicator(ILiveDevice device, ILogger<ArpCommunicator> logger, IPAddress localIpAddress) : base(device, logger)
        {
            _localIpAddress = localIpAddress;
            CommunicatorPacket.SenderProtocolAddress = _localIpAddress;
        }

        protected override EthernetPacket CreateEthernetPacket()
        {
            return new EthernetPacket(LocalMacAddress, BroadcastMacAddress, EthernetType);
        }

        public void SendArpRequest(IPAddress targetIpAddress)
        {
            CommunicatorPacket.TargetProtocolAddress = targetIpAddress;
            SendPacket();
        }

        protected override void PacketArrival(ArpPacket packet)
        {
            if (packet.Operation == ArpOperation.Request)
            {
                // 忽略请求包
                return;
            }

            StringBuilder sb = new();
            sb.AppendLine("收到ARP数据包:");
            sb.AppendLine($"操作: {packet.Operation}");
            sb.AppendLine($"发送方硬件地址: {packet.SenderHardwareAddress}");
            sb.AppendLine($"目标硬件地址: {packet.TargetHardwareAddress}");
            sb.AppendLine($"发送方协议地址: {packet.SenderProtocolAddress}");
            sb.AppendLine($"目标协议地址: {packet.TargetProtocolAddress}");
            sb.AppendLine();

            Debug.Print(sb.ToString());
        }

        protected override ArpPacket CreateCommunicatorPacket()
        {
            return new ArpPacket(ArpOperation.Request, NoneMacAddress, IPAddress.None, LocalMacAddress, IPAddress.None);
        }
    }
}