using PacketDotNet;
using SharpPcap;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace ExtenderApp.LAN
{
    /// <summary>
    /// 负责通过 SharpPcap/PacketDotNet 发送与监听 ARP 数据包。
    /// 需要系统安装 Npcap/Libpcap，且通常需要管理员权限或特权。
    /// </summary>
    public class ArpCommunicator : Communicator<ArpPacket>
    {
        protected override string Filter => "arp";

        public ArpCommunicator(ILiveDevice device, IPAddress localIpAddress, PhysicalAddress localMacAddress) : base(device, localIpAddress, localMacAddress)
        {
        }

        public void SendArpRequest(IPAddress targetIpAddress)
        {
            ArpPacket arpRequest = new ArpPacket(ArpOperation.Request, PhysicalAddress.Parse("00-00-00-00-00-00"), targetIpAddress, LocalMacAddress, LocalIpAddress);
            EthernetPacket ethernetPacket = new EthernetPacket(LocalMacAddress, PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"), EthernetType.Arp);
            ethernetPacket.PayloadPacket = arpRequest;
            SendPacket(arpRequest);
        }

        protected override void PacketArrival(ArpPacket packet)
        {
            Debug.Print(packet.HeaderDataSegment.Length.ToString());
        }
    }
}