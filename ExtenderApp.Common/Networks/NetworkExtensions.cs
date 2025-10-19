using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks.LinkClients;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 网络扩展类
    /// </summary>
    internal static class NetworkExtensions
    {
        /// <summary>
        /// 向服务集合中添加网络相关的服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddNetwork(this IServiceCollection services)
        {
            services.AddLinker();
            services.AddUdpLinker();
            services.AddLinkerClient();
            services.AddFileSegmenter();
            return services;
        }
    }
}
