using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using ExtenderApp.Data;
using PacketDotNet;
using SharpPcap;

namespace ExtenderApp.LAN
{
    public abstract class Communicator<T> : DisposableObject
        where T : Packet
    {
        protected ILiveDevice Device { get; }
        protected IPAddress LocalIpAddress { get; }
        protected PhysicalAddress LocalMacAddress { get; }

        protected abstract string Filter { get; }

        protected Communicator(ILiveDevice device, IPAddress localIpAddress, PhysicalAddress localMacAddress)
        {
            Device = device;
            LocalIpAddress = localIpAddress;
            LocalMacAddress = localMacAddress;
            Device.Open(DeviceModes.Promiscuous);
            device.Filter = Filter;
            device.OnPacketArrival += OnPacketArrival;
            device.StartCapture();
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            RawCapture rawPacket = e.GetPacket();
            Packet packet = rawPacket.GetPacket();
            T specificPacket = packet.Extract<T>();
            if (specificPacket != null)
            {
                PacketArrival(specificPacket);
            }
        }

        protected abstract void PacketArrival(T packet);

        public void SendPacket(T packet)
        {
            Device.SendPacket(packet);
        }
    }
}
