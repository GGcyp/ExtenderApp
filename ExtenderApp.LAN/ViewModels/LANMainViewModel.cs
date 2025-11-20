using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using SharpPcap;
using SharpPcap.LibPcap;

namespace ExtenderApp.LAN
{
    public class LANMainViewModel : ExtenderAppViewModel<LANMainView, LANModel>
    {
        public LANMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            // 将扫描操作放入后台任务，避免阻塞UI线程
            //Task.Run(ScanLanForDevices);
            Task.Run(ScanLan);
        }

        private void ScanLan()
        {
            var activeDevice = LibPcapLiveDeviceList.Instance;
            foreach (var dev in activeDevice)
            {
                var ipv4AddressInfo = dev.Addresses.First(a => a.Addr?.ipAddress?.AddressFamily == AddressFamily.InterNetwork);
                if(ipv4AddressInfo == null)
                {
                    continue;
                }
                var ipAddress = ipv4AddressInfo.Addr.ipAddress;
                var netmask = ipv4AddressInfo.Netmask.ipAddress;

                ArpCommunicator arpCommunicator = new ArpCommunicator(dev, ipAddress, dev.MacAddress);
            }
        }

        private void ScanLanForDevices()
        {
            // 1. 筛选出已连接并启用的网络设备
            const uint PCAP_IF_UP = 2; // 0x2
            const uint PCAP_IF_RUNNING = 4; // 0x4
            var activeDevice = LibPcapLiveDeviceList.Instance.FirstOrDefault(dev =>
                !dev.Loopback &&
                (dev.Flags & PCAP_IF_UP) == PCAP_IF_UP &&
                (dev.Flags & PCAP_IF_RUNNING) == PCAP_IF_RUNNING &&
                dev.Addresses.Any(a => a.Addr?.ipAddress.AddressFamily == AddressFamily.InterNetwork && a.Netmask != null));

            if (activeDevice == null)
            {
                LogWarning("未找到活动的网络设备。");
                return;
            }

            LogInformation($"使用设备进行扫描: {activeDevice.Description}");

            // 获取设备的IPv4地址和子网掩码
            var ipv4AddressInfo = activeDevice.Addresses.First(a => a.Addr.ipAddress.AddressFamily == AddressFamily.InterNetwork);
            var ipAddress = ipv4AddressInfo.Addr.ipAddress;
            var netmask = ipv4AddressInfo.Netmask.ipAddress;

            // 2. 计算要扫描的IP地址范围
            //var ipList = GetIpRange(ipAddress, netmask);
            //LogInformation($"开始扫描 {ipList.Count} 个IP地址...");

            // 打开设备以进行后续操作
            activeDevice.Open(DeviceModes.Promiscuous, 1000);

            ArpCommunicator arpCommunicator = new ArpCommunicator(activeDevice, ipAddress, activeDevice.MacAddress);

            // 3. 并行发送ARP请求以扫描局域网
            //Parallel.ForEach(ipList, ip =>
            //{
            //    try
            //    {
            //        arpCommunicator.SendArpRequest(ip);
            //    }
            //    catch
            //    {
            //        // Resolve会因超时而抛出异常，这里忽略即可
            //    }
            //});

            //// 操作完成后关闭设备
            //activeDevice.Close();

        }

        /// <summary>
        /// 根据IP地址和子网掩码计算出整个网段的IP地址列表。
        /// </summary>
        private List<IPAddress> GetIpRange(IPAddress ip, IPAddress subnetMask)
        {
            byte[] ipBytes = ip.GetAddressBytes();
            byte[] maskBytes = subnetMask.GetAddressBytes();

            // 计算网络地址
            byte[] networkAddressBytes = new byte[ipBytes.Length];
            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkAddressBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }

            // 计算广播地址
            byte[] broadcastAddressBytes = new byte[ipBytes.Length];
            for (int i = 0; i < ipBytes.Length; i++)
            {
                broadcastAddressBytes[i] = (byte)(networkAddressBytes[i] | ~maskBytes[i]);
            }

            var ipList = new List<IPAddress>();
            // 从网络地址+1开始，到广播地址-1结束
            for (uint i = 1; i < 0xFFFFFFFF; i++)
            {
                var currentIpBytes = (byte[])networkAddressBytes.Clone();
                // 增加IP地址
                uint ipAsUint = (uint)networkAddressBytes[0] << 24 | (uint)networkAddressBytes[1] << 16 | (uint)networkAddressBytes[2] << 8 | networkAddressBytes[3];
                ipAsUint += i;
                currentIpBytes[0] = (byte)(ipAsUint >> 24);
                currentIpBytes[1] = (byte)(ipAsUint >> 16);
                currentIpBytes[2] = (byte)(ipAsUint >> 8);
                currentIpBytes[3] = (byte)ipAsUint;

                var currentIp = new IPAddress(currentIpBytes);

                // 如果当前IP等于广播地址，则停止
                if (currentIp.Equals(new IPAddress(broadcastAddressBytes)))
                {
                    break;
                }

                ipList.Add(currentIp);
            }

            return ipList;
        }
    }
}