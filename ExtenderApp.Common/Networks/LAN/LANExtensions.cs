using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Common.Networks.LAN
{
    public static class LANExtensions
    {


        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(uint destIp, uint srcIp, byte[] pMacAddr, ref uint phyAddrLen);

        public static System.Net.NetworkInformation.PhysicalAddress TryGetMac(IPAddress ip)
        {
            try
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork)
                    return System.Net.NetworkInformation.PhysicalAddress.None;

                var dest = BitConverter.ToUInt32(ip.GetAddressBytes(), 0);
                uint macAddrLen = 6;
                var macAddr = new byte[6];
                var res = SendARP(dest, 0, macAddr, ref macAddrLen);
                if (res != 0 || macAddrLen == 0)
                    return System.Net.NetworkInformation.PhysicalAddress.None;

                return System.Net.NetworkInformation.PhysicalAddress.Parse(string.Join("", macAddr.Take((int)macAddrLen).Select(b => b.ToString("X2"))));
            }
            catch
            {
                return System.Net.NetworkInformation.PhysicalAddress.None;
            }
        }
    }
}

namespace ArpScanner
{
    class Program
    {
        // 获取本机活跃的IPv4地址和对应的MAC地址
        private static (IPAddress ip, PhysicalAddress mac) GetLocalIpAndMac()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // 排除非活跃网卡和回环网卡
                if (nic.OperationalStatus != OperationalStatus.Up ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var props = nic.GetIPProperties();
                // 查找IPv4单播地址
                var unicast = props.UnicastAddresses.FirstOrDefault(a =>
                    a.Address.AddressFamily == AddressFamily.InterNetwork);
                if (unicast != null)
                {
                    return (unicast.Address, nic.GetPhysicalAddress());
                }
            }
            return (null, null);
        }

        // 将ushort转换为网络字节序（大端序）
        private static byte[] ToNetworkByteOrder(ushort value)
        {
            BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(new byte[2]), value);
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes); // 小端系统反转字节
            return bytes;
        }

        // 构造ARP请求包（以太网帧头+ARP数据）
        private static byte[] BuildArpRequest(PhysicalAddress senderMac, IPAddress senderIp, IPAddress targetIp)
        {
            byte[] packet = new byte[42]; // 总长度：14（以太网帧头）+ 28（ARP数据）

            // 以太网帧头（14字节）
            byte[] broadcastMac = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }; // 广播目的MAC
            byte[] srcMac = senderMac.GetAddressBytes();
            byte[] etherType = ToNetworkByteOrder((ushort)0x0806); // ARP类型标识

            Buffer.BlockCopy(broadcastMac, 0, packet, 0, 6); // 目的MAC
            Buffer.BlockCopy(srcMac, 0, packet, 6, 6);      // 源MAC
            Buffer.BlockCopy(etherType, 0, packet, 12, 2);  // 帧类型

            // ARP数据（28字节）
            byte[] hardwareType = ToNetworkByteOrder((ushort)1);       // 硬件类型（以太网）
            byte[] protocolType = ToNetworkByteOrder((ushort)0x0800);  // 协议类型（IPv4）
            byte hardwareLen = 6;  // MAC地址长度
            byte protocolLen = 4;  // IP地址长度
            byte[] operation = ToNetworkByteOrder((ushort)1);          // 操作码（1=请求）
            byte[] senderIpBytes = senderIp.GetAddressBytes();
            byte[] targetMac = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };  // 目标MAC（未知）
            byte[] targetIpBytes = targetIp.GetAddressBytes();

            Buffer.BlockCopy(hardwareType, 0, packet, 14, 2);   // 硬件类型
            Buffer.BlockCopy(protocolType, 0, packet, 16, 2);   // 协议类型
            packet[18] = hardwareLen;                           // 硬件地址长度
            packet[19] = protocolLen;                           // 协议地址长度
            Buffer.BlockCopy(operation, 0, packet, 20, 2);      // 操作码
            Buffer.BlockCopy(srcMac, 0, packet, 22, 6);         // 发送端MAC
            Buffer.BlockCopy(senderIpBytes, 0, packet, 28, 4);  // 发送端IP
            Buffer.BlockCopy(targetMac, 0, packet, 32, 6);      // 目标MAC
            Buffer.BlockCopy(targetIpBytes, 0, packet, 38, 4);  // 目标IP

            return packet;
        }

        // 发送ARP请求
        private static void SendArpRequest(Socket socket, byte[] request, IPAddress targetIp)
        {
            IPEndPoint targetEndPoint = new IPEndPoint(targetIp, 0);
            socket.SendTo(request, targetEndPoint);
        }

        // 接收并解析ARP应答
        private static Dictionary<IPAddress, PhysicalAddress> ReceiveArpResponses(Socket socket, int timeoutMs)
        {
            var devices = new Dictionary<IPAddress, PhysicalAddress>();
            socket.ReceiveTimeout = timeoutMs;
            byte[] buffer = new byte[1024];
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (true)
                {
                    int bytesRead = socket.ReceiveFrom(buffer, ref remoteEndPoint);
                    if (bytesRead < 42) continue; // 无效ARP包

                    // 解析以太网帧类型（判断是否为ARP包）
                    byte[] ethTypeBytes = new byte[2];
                    Buffer.BlockCopy(buffer, 12, ethTypeBytes, 0, 2);
                    ushort ethType = BitConverter.ToUInt16(ethTypeBytes, 0);
                    if (BitConverter.IsLittleEndian) ethType = (ushort)((ethType << 8) | (ethType >> 8));
                    if (ethType != 0x0806) continue; // 不是ARP包

                    // 解析ARP操作码（判断是否为应答）
                    byte[] opBytes = new byte[2];
                    Buffer.BlockCopy(buffer, 20, opBytes, 0, 2);
                    ushort operation = BitConverter.ToUInt16(opBytes, 0);
                    if (BitConverter.IsLittleEndian) operation = (ushort)((operation << 8) | (operation >> 8));
                    if (operation != 2) continue; // 不是应答包

                    // 提取发送端MAC（目标设备的MAC）
                    byte[] senderMac = new byte[6];
                    Buffer.BlockCopy(buffer, 22, senderMac, 0, 6);

                    // 提取发送端IP（目标设备的IP）
                    byte[] senderIpBytes = new byte[4];
                    Buffer.BlockCopy(buffer, 28, senderIpBytes, 0, 4);
                    IPAddress senderIp = new IPAddress(senderIpBytes);

                    if (!devices.ContainsKey(senderIp))
                    {
                        devices.Add(senderIp, new PhysicalAddress(senderMac));
                    }
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                // 超时退出
            }

            return devices;
        }

        static void Main(string[] args)
        {
            // 获取本机IP和MAC
            var (localIp, localMac) = GetLocalIpAndMac();
            if (localIp == null || localMac == null)
            {
                Console.WriteLine("无法获取本机网络信息");
                return;
            }
            Console.WriteLine($"本机IP: {localIp}, MAC: {BitConverter.ToString(localMac.GetAddressBytes()).Replace('-', ':')}");

            // 创建原始套接字（需要管理员权限）
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP))
            {
                try
                {
                    socket.Bind(new IPEndPoint(localIp, 0));
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    //socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

                    // 启用接收所有数据包
                    byte[] inValue = { 1, 0, 0, 0 }; // 1=启用
                    byte[] outValue = new byte[4];
                    socket.IOControl(IOControlCode.ReceiveAll, inValue, outValue);
                    
                    // 扫描局域网IP（假设子网为x.x.x.1-254）
                    string baseIp = localIp.ToString().Substring(0, localIp.ToString().LastIndexOf('.') + 1);
                    Console.WriteLine("\n开始发送ARP请求...");
                    for (int i = 1; i <= 254; i++)
                    {
                        if (IPAddress.TryParse($"{baseIp}{i}", out IPAddress targetIp))
                        {
                            byte[] request = BuildArpRequest(localMac, localIp, targetIp);
                            SendArpRequest(socket, request, targetIp);
                        }
                    }

                    // 等待应答（超时5秒）
                    Console.WriteLine("\n等待ARP应答...");
                    var devices = ReceiveArpResponses(socket, 5000);

                    // 输出结果
                    Console.WriteLine("\n局域网在线设备：");
                    foreach (var device in devices)
                    {
                        string macStr = BitConverter.ToString(device.Value.GetAddressBytes()).Replace('-', ':');
                        Console.WriteLine($"IP: {device.Key,-15} MAC: {macStr}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"错误：{ex.Message}（请以管理员身份运行程序）");
                }
            }

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}
