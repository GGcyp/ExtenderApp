using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                //var ipv4AddressInfo = dev.Addresses.First(a => a.Addr?.ipAddress?.AddressFamily == AddressFamily.InterNetwork);
                var ipv4AddressInfo = dev.Addresses.FirstOrDefault(a =>
                {
                    if (a.Addr?.ipAddress == null || a.Netmask == null)
                    {
                        return false;
                    }
                    if (a.Addr.ipAddress.AddressFamily != AddressFamily.InterNetwork)
                        return false;
                    // 过滤掉 APIPA 地址
                    if (a.Addr.ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && a.Addr.ipAddress.ToString().StartsWith("169.254."))
                    {
                        return false;
                    }
                    if (!a.Addr.ipAddress.GetAddressBytes().SequenceEqual(IPAddress.Parse("192.168.3.53").GetAddressBytes()))
                    {
                        return false;
                    }
                    return true;
                });
                if (ipv4AddressInfo == null)
                {
                    continue;
                }
                var ipAddress = ipv4AddressInfo.Addr.ipAddress;
                var netmask = ipv4AddressInfo.Netmask.ipAddress;

                ArpCommunicator arpCommunicator = new ArpCommunicator(dev, ServiceStore.ServiceProvider.GetRequiredService<ILogger<ArpCommunicator>>(), ipAddress);
                for (int i = 2; i < 255; i++)
                {
                    var targetIp = IPAddress.Parse($"{ipAddress.GetAddressBytes()[0]}.{ipAddress.GetAddressBytes()[1]}.{ipAddress.GetAddressBytes()[2]}.{i}");
                    try
                    {
                        arpCommunicator.SendArpRequest(targetIp);
                    }
                    catch
                    {
                        // Resolve会因超时而抛出异常，这里忽略即可
                    }
                }
            }
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