using ExtenderApp.Abstract;
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

        /// <summary>
        /// 创建并返回当前链接器实例的一个强类型副本。
        /// </summary>
        /// <typeparam name="T">链接器的具体类型，必须实现 ILinker。</typeparam>
        /// <param name="linker">要克隆的链接器实例。</param>
        /// <returns>返回类型为 <typeparamref name="T"/> 的新链接器实例。</returns>
        public static T Clone<T>(this T linker) where T : ILinker
        {
            return (T)linker.Clone();
        }
    }
}