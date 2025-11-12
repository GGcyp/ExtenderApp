using System.Buffers;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Common.Networks.Formatters;
using ExtenderApp.Common.Networks.LinkClients;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 网络扩展类，提供网络服务注册与常用地址计算辅助方法。
    /// </summary>
    internal static class NetworkExtensions
    {
        /// <summary>
        /// IPv4 地址字节长度（4 字节）。
        /// </summary>
        public const int IPv4BytesLength = 4;

        /// <summary>
        /// IPv6 地址字节长度（16 字节）。
        /// </summary>
        public const int IPv6BytesLength = 16;

        /// <summary>
        /// 向 DI 服务集合中注册网络相关组件。
        /// </summary>
        public static IServiceCollection AddNetwork(this IServiceCollection services)
        {
            services.AddFormatter();
            services.AddLinker();
            services.AddUdpLinker();
            services.AddLinkerClient();
            services.AddFileSegmenter();
            return services;
        }

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
                ? IPv4BytesLength
                : IPv6BytesLength;

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

            byte[] rented = ArrayPool<byte>.Shared.Rent(IPv4BytesLength * 3);
            Span<byte> ipSpan = rented.AsSpan(0, IPv4BytesLength);
            Span<byte> maskSpan = rented.AsSpan(IPv4BytesLength, IPv4BytesLength);

            try
            {
                if (!ip.TryWriteBytes(ipSpan, out _) || !subnetMask.TryWriteBytes(maskSpan, out _))
                    return null;

                Span<byte> resultSpan = rented.AsSpan(IPv4BytesLength * 2, IPv4BytesLength);
                for (int i = 0; i < IPv4BytesLength; i++)
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