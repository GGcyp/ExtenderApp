

using System.Net;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 提供处理 IP 地址的静态方法。
    /// </summary>
    public static class IPAddressExtensions
    {
        /// <summary>
        /// 将 IPv4 地址递增一个单位。
        /// </summary>
        /// <param name="ip">要递增的 IPv4 地址。</param>
        /// <returns>递增后的 IPv4 地址。</returns>
        /// <exception cref="ArgumentNullException">当传入的 IP 地址为 null 时抛出。</exception>
        /// <exception cref="NotSupportedException">当传入的地址不是 IPv4 地址时抛出。</exception>
        public static IPAddress Increment(this IPAddress ip)
        {
            if (ip == null)
            {
                throw new ArgumentNullException(nameof(ip), "传入的 IP 地址不能为 null。");
            }

            if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new NotSupportedException("此方法仅支持 IPv4 地址。");
            }

            var bytes = ip.GetAddressBytes();
            for (var i = bytes.Length - 1; i >= 0; i--)
            {
                if (bytes[i] == 255)
                {
                    bytes[i] = 0;
                }
                else
                {
                    bytes[i]++;
                    return new IPAddress(bytes);
                }
            }
            // 处理 255.255.255.255 递增的情况
            return new IPAddress(bytes);
        }

        /// <summary>
        /// 将 IPv4 地址递减一个单位（扩展方法）。
        /// </summary>
        /// <param name="ip">要递减的 IPv4 地址。</param>
        /// <returns>递减后的 IPv4 地址。</returns>
        /// <exception cref="ArgumentNullException">当传入的 IP 地址为 null 时抛出。</exception>
        /// <exception cref="NotSupportedException">当传入的地址不是 IPv4 地址时抛出。</exception>
        public static IPAddress Decrement(this IPAddress ip)
        {
            if (ip == null)
            {
                throw new ArgumentNullException(nameof(ip), "传入的 IP 地址不能为 null。");
            }

            if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new NotSupportedException("此方法仅支持 IPv4 地址。");
            }

            var bytes = ip.GetAddressBytes();
            for (var i = bytes.Length - 1; i >= 0; i--)
            {
                if (bytes[i] == 0)
                {
                    bytes[i] = 255;
                }
                else
                {
                    bytes[i]--;
                    return new IPAddress(bytes);
                }
            }
            // 处理 0.0.0.0 递减的情况
            return new IPAddress(bytes);
        }

        /// <summary>
        /// IP地址转换辅助方法
        /// </summary>
        /// <param name="ip">需要转换的IP地址</param>
        /// <returns>转换后的无符号整数</returns>
        public static uint IpToUint(this IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();
            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) |
                   ((uint)bytes[2] << 8) | bytes[3];
        }

        /// <summary>
        /// 将无符号整数转换为IP地址
        /// </summary>
        /// <param name="ipAddress">需要转换的无符号整数</param>
        /// <returns>转换后的IP地址</returns>
        public static IPAddress UintToIp(this uint ipAddress)
        {
            return ipAddress.UintToIp(new byte[4]);
        }

        /// <summary>
        /// 将无符号整数转换为IP地址。
        /// </summary>
        /// <param name="ipAddress">表示IP地址的无符号整数。</param>
        /// <param name="bytes">存储IP地址的字节数组。</param>
        /// <returns>转换后的IP地址。</returns>
        public static IPAddress UintToIp(this uint ipAddress, byte[] bytes)
        {
            bytes[0] = (byte)((ipAddress >> 24) & 0xFF);
            bytes[1] = (byte)((ipAddress >> 16) & 0xFF);
            bytes[2] = (byte)((ipAddress >> 8) & 0xFF);
            bytes[3] = (byte)(ipAddress & 0xFF);

            return new IPAddress(bytes);
        }

        /// <summary>
        /// 安全获取指定IP地址的主机名
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>返回主机名，如果无法获取则返回"Unknown"</returns>
        public static async Task<string> SafeGetHostName(this IPAddress ip)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(ip);
                return entry.HostName;
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
