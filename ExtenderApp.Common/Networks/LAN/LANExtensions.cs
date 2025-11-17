using System.Buffers;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks.LAN
{
    public static class LANExtensions
    {
        /// <summary>
        /// 计算网络地址（逐字节 AND 运算：IP &amp; Mask）。支持 IPv4 / IPv6。
        /// </summary>
        public static IPAddress? CalculateNetworkAddress(IPAddress ip, IPAddress subnetMask)
        {
            ArgumentNullException.ThrowIfNull(ip);
            ArgumentNullException.ThrowIfNull(subnetMask);
            if (ip.AddressFamily != subnetMask.AddressFamily)
                throw new ArgumentException("IP地址和子网掩码必须属于同一地址族");

            int length = ip.AddressFamily == AddressFamily.InterNetwork
                ? ValueIPAddress.IPv4Length
                : ValueIPAddress.IPv6Length;

            byte[] rented = ArrayPool<byte>.Shared.Rent(length * 3);
            Span<byte> ipSpan = rented.AsSpan(0, length);
            Span<byte> maskSpan = rented.AsSpan(length, length);

            try
            {
                if (!ip.TryWriteBytes(ipSpan, out _) || !subnetMask.TryWriteBytes(maskSpan, out _))
                    return null;

                Span<byte> resultSpan = rented.AsSpan(length * 2, length);
                for (int i = 0; i < length; i++)
                    resultSpan[i] = (byte)(ipSpan[i] & maskSpan[i]);

                return new IPAddress(resultSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        /// <summary>
        /// 计算 IPv4 广播地址：broadcast = ip | (~mask)。仅 IPv4 有广播概念。
        /// </summary>
        public static IPAddress? CalculateBroadcastAddressIPv4(IPAddress ip, IPAddress subnetMask)
        {
            ArgumentNullException.ThrowIfNull(ip);
            ArgumentNullException.ThrowIfNull(subnetMask);

            if (ip.AddressFamily != subnetMask.AddressFamily)
                throw new ArgumentException("IP地址和子网掩码必须属于同一地址族");
            if (ip.AddressFamily != AddressFamily.InterNetwork)
                throw new NotSupportedException("仅 IPv4 支持广播地址计算");

            byte[] rented = ArrayPool<byte>.Shared.Rent(ValueIPAddress.IPv4Length * 3);
            Span<byte> ipSpan = rented.AsSpan(0, ValueIPAddress.IPv4Length);
            Span<byte> maskSpan = rented.AsSpan(ValueIPAddress.IPv4Length, ValueIPAddress.IPv4Length);

            try
            {
                if (!ip.TryWriteBytes(ipSpan, out _) || !subnetMask.TryWriteBytes(maskSpan, out _))
                    return null;

                Span<byte> resultSpan = rented.AsSpan(ValueIPAddress.IPv4Length * 2, ValueIPAddress.IPv4Length);
                for (int i = 0; i < ValueIPAddress.IPv4Length; i++)
                    resultSpan[i] = (byte)(ipSpan[i] | (byte)~maskSpan[i]);

                return new IPAddress(resultSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }
}