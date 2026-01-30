using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 提供用于配置和操作 ILinker 和 ILinkClient 的扩展方法。
    /// </summary>
    internal static class LinkClientExtensions
    {
        /// <summary>
        /// 将所有与链接客户端相关的服务注册到依赖注入容器中。
        /// </summary>
        /// <param name="services">要向其添加服务的 IServiceCollection。</param>
        /// <returns>返回配置后的 IServiceCollection，以支持链式调用。</returns>
        public static IServiceCollection AddLinkerClient(this IServiceCollection services)
        {
            services.AddTcpLinkClient();
            services.AddUdpLinkClient();
            services.AddHttpLinkClient();

            return services;
        }
    }
}