using System.Net;
using ExtenderApp.Common.Caches;
using ExtenderApp.Common.Hash;

namespace ExtenderApp.Common.Caches
{
    /// <summary>
    /// IP地址缓存类，继承自EvictionCache泛型类，用于缓存int键和IPAddress值。
    /// </summary>
    public class IPAddressCache : EvictionCache<int, IPAddress>
    {
        /// <summary>
        /// 通过字节数组获取对应的IP地址字符串。
        /// </summary>
        /// <param name="bytes">包含IP地址的字节数组。</param>
        /// <returns>返回对应的IP地址字符串。</returns>
        public IPAddress GetIpAddress(byte[] bytes)
        {
            return GetIpAddress(bytes.AsSpan());
        }

        /// <summary>
        /// 通过字节序列获取对应的IP地址字符串。
        /// </summary>
        /// <param name="span">包含IP地址的字节序列。</param>
        /// <returns>返回对应的IP地址字符串。</returns>
        public IPAddress GetIpAddress(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return IPAddress.Any;

            int hash = span.ComputeHash_FNV_1a();
            if (TryGet(hash, out var result))
                return result;

            lock (this)
            {
                if (TryGet(hash, out result))
                    return result;

                result = new IPAddress(span);
                AddOrUpdate(hash, result);
            }
            return result;
        }
    }
}
