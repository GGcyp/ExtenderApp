using System.Net;
using System.Net.Sockets;
using ExtenderApp.Common.Networks.Formatters;
using ExtenderApp.Common.Networks.LinkClients;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 网络扩展类，提供网络服务注册与常用地址计算辅助方法。
    /// </summary>
    internal static class NetworkExtensions
    {
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

        public static uint ToUInt32(this IPAddress ipAddress)
        {
            ValueIPAddress valueIPAddress = ValueIPAddress.FromIPAddress(ipAddress);
            var result = valueIPAddress.ToUInt32();
            valueIPAddress.Dispose();
            return result;
        }

        public static uint ToUInt32(this ValueIPAddress ipAddress)
        {
            if (ipAddress.IsEmpty)
                throw new ArgumentNullException(nameof(ipAddress));
            if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                throw new ArgumentException("仅支持 IPv4 地址转换为 UInt32");

            ReadOnlySpan<byte> span = ipAddress.AsSpan();
            return (uint)(span[0] << 24 | span[1] << 16 | span[2] << 8 | span[3]);
        }

        public static ValueIPAddress ToIPAddress(this uint ipAddress)
        {
            Span<byte> bytes = stackalloc byte[4];
            bytes[0] = (byte)((ipAddress >> 24) & 0xFF);
            bytes[1] = (byte)((ipAddress >> 16) & 0xFF);
            bytes[2] = (byte)((ipAddress >> 8) & 0xFF);
            bytes[3] = (byte)(ipAddress & 0xFF);
            return new ValueIPAddress(bytes);
        }
    }
}