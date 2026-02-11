using System.Net;
using ExtenderApp.Common.Networks.LinkClients;
using ExtenderApp.Contracts;
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
            services.AddLinker();
            services.AddLinkerClient();
            return services;
        }

        public static uint ToUInt32(this IPAddress ipAddress)
        {
            //ValueIPAddress valueIPAddress = ValueIPAddress.FromIPAddress(ipAddress);
            //var result = valueIPAddress.ToUInt32();
            //valueIPAddress.Dispose();
            //return result;
            return 0;
        }
    }
}