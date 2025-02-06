using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ExtenderApp.Common.ObjectPools;


namespace ExtenderApp.Common
{
    /// <summary>
    /// 局域网接口信息类
    /// </summary>
    public class LANInteraceInfo
    {
        /// <summary>
        /// 创建一个默认的对象池，用于存储<see cref="LANHostInfo"/>对象
        /// </summary>
        private readonly static ObjectPool<LANHostInfo> _pool = ObjectPool.CreateDefaultPool<LANHostInfo>();

        /// <summary>
        /// 初始数量
        /// </summary>
        private const int InitialCount = 50;

        /// <summary>
        /// 网络接口
        /// </summary>
        public NetworkInterface Interface { get; private set; }

        /// <summary>
        /// 局域网主机信息集合
        /// </summary>
        public ConcurrentBag<LANHostInfo>? LANHostInfos { get; private set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        private int timeout;

        /// <summary>
        /// 网络字节数组
        /// </summary>
        private byte[]? networkBytes;

        /// <summary>
        /// 广播字节数组
        /// </summary>
        private byte[]? broadcastBytes;

        /// <summary>
        /// 初始化<see cref="LANInteraceInfo"/>实例
        /// </summary>
        /// <param name="interface">网络接口</param>
        public LANInteraceInfo(NetworkInterface @interface)
        {
            Interface = @interface;
            timeout = 100;
        }

        /// <summary>
        /// 扫描本地网络
        /// </summary>
        public void ScanLocalNetwork()
        {
            Task.Run(ScanLocalNetworkAsync);
        }

        /// <summary>
        /// 异步扫描本地网络
        /// </summary>
        /// <returns>异步任务</returns>
        public async Task ScanLocalNetworkAsync()
        {
            await ScanLocalNetworkAsync(timeout);
        }

        /// <summary>
        /// 异步扫描本地网络
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns>异步任务</returns>
        public async Task ScanLocalNetworkAsync(int timeout)
        {
            //检查接口状态和网络接口类型，如果不满足条件则返回。
            if (Interface.OperationalStatus != OperationalStatus.Up ||
                Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                return;

            LANHostInfos = new();
            networkBytes = new byte[4];
            broadcastBytes = new byte[4];
            this.timeout = timeout;

            // 使用信号量控制并发数（最大50）
            using var semaphore = new SemaphoreSlim(InitialCount);
            var tasks = new List<Task>(InitialCount);

            foreach (var addr in Interface.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily != AddressFamily.InterNetwork || addr.IPv4Mask == null)
                    continue;

                var ip = addr.Address;
                var mask = addr.IPv4Mask;

                // 计算有效IP范围（排除网络地址和广播地址）
                var (startIp, endIp) = CalculateValidIpRange(ip, mask);

                if (startIp is null || endIp is null)
                    continue;

                // 转换为数值形式便于比较
                var current = startIp.IpToUint();
                var end = endIp.IpToUint();
                byte[] uintBytes = new byte[4];

                while (current <= end)
                {
                    await semaphore.WaitAsync();
                    var targetIp = current.UintToIp(uintBytes);
                    current++;

                    tasks.Add(Task.Run(async () =>
                    {
                        var ping = _pool.Get();
                        try
                        {
                            var reply = await ping.PingHostAsync(targetIp, this.timeout);

                            if (reply.Status == IPStatus.Success)
                            {
                                LANHostInfos.Add(ping);
                            }
                            else
                            {
                                _pool.Release(ping);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 计算有效IP范围（排除网络地址和广播地址）
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="mask">子网掩码</param>
        /// <returns>有效IP范围</returns>
        private (IPAddress?, IPAddress?) CalculateValidIpRange(IPAddress? ip, IPAddress? mask)
        {
            if (ip == null || mask == null)
                return (null, null);

            var ipBytes = ip.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();

            for (int i = 0; i < 4; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
            }

            // 起始地址+1，结束地址-1
            var start = new IPAddress(networkBytes).Increment();
            var end = new IPAddress(broadcastBytes).Decrement();

            return (start, end);
        }
    }
}
