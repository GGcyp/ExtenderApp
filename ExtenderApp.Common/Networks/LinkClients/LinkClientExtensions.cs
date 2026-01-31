using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 提供用于配置和操作 <see cref="ILinker"/> 与 <see cref="ILinkClient"/> 的 DI 扩展方法集合。
    /// </summary>
    internal static class LinkClientExtensions
    {
        /// <summary>
        /// 将所有内置的链接客户端相关服务（TCP / UDP / HTTP）注册到指定的依赖注入容器中。
        /// </summary>
        /// <param name="services">要向其添加服务的 <see cref="IServiceCollection"/> 实例。</param>
        /// <returns>返回传入的 <see cref="IServiceCollection"/> 实例，以支持方法链式调用。</returns>
        public static IServiceCollection AddLinkerClient(this IServiceCollection services)
        {
            services.AddTcpLinkClient();
            services.AddUdpLinkClient();
            services.AddHttpLinkClient();

            return services;
        }

        /// <summary>
        /// 将指定类型的链接客户端及其工厂注册到依赖注入容器中。
        /// </summary>
        /// <typeparam name="TLinkClient">要注册的链接客户端实现类型，必须实现 <see cref="ILinkClient"/>。</typeparam>
        /// <typeparam name="TLinkClientFactory">用于创建链接客户端实例的工厂类型，必须实现 <see cref="ILinkClientFactory{TLinkClient}"/>。</typeparam>
        /// <param name="services">要向其添加服务的 <see cref="IServiceCollection"/> 实例。</param>
        /// <returns>返回传入的 <see cref="IServiceCollection"/> 实例，以支持链式调用。</returns>
        public static IServiceCollection AddLinkClient<TLinkClient, TLinkClientFactory>(this IServiceCollection services)
            where TLinkClient : class, ILinkClient
            where TLinkClientFactory : class, ILinkClientFactory<TLinkClient>
        {
            services.AddSingleton<ILinkClientFactory<TLinkClient>, TLinkClientFactory>();
            services.AddTransient(provider =>
            {
                var factory = provider.GetRequiredService<ILinkClientFactory<TLinkClient>>();
                return factory.CreateLinkClient();
            });
            return services;
        }

        /// <summary>
        /// 创建并返回当前链接器实例的一个强类型副本（克隆）。
        /// </summary>
        /// <typeparam name="T">链接器的具体类型，必须实现 <see cref="ILinker"/>。</typeparam>
        /// <param name="linker">要克隆的链接器实例。</param>
        /// <returns>返回与 <paramref name="linker"/> 相同具体类型的克隆实例。</returns>
        public static T Clone<T>(this T linker) where T : ILinker
        {
            return (T)linker.Clone();
        }
    }
}