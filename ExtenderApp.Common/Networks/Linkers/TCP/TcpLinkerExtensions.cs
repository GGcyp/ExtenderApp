using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 针对 TCP 链路器相关类型的扩展方法集合。
    /// </summary>
    internal static class TcpLinkerExtensions
    {
        /// <summary>
        /// 向依赖注入服务集合注册 TCP 链路器相关的工厂与解析规则。
        /// </summary>
        /// <param name="services">要注册服务的 <see cref="IServiceCollection"/> 实例。</param>
        /// <returns>返回同一 <see cref="IServiceCollection"/>，以便链式调用。</returns>
        internal static IServiceCollection AddTcpLinker(this IServiceCollection services)
        {
            services.AddLinker<ITcpLinker, TcpLinkerFactory>();
            //services.AddSingleton<ITcpListenerLinkerFactory, TcpListenerLinkerFactory>();
            //// 每次解析 IListenerLinker<TLinkClient> 时通过工厂创建
            //services.AddTransient(provider =>
            //{
            //    return provider.GetRequiredService<ITcpListenerLinkerFactory>().CreateListenerLinker();
            //});
            return services;
        }

        /// <summary>
        /// 若给定的 <see cref="ILinker"/> 实例实现了 <see cref="ITcpLinker"/>，则返回其对应的 <see cref="TcpLinkerStream"/>；否则返回 null。
        /// </summary>
        /// <param name="linker">要从中获取流的链路器实例。</param>
        /// <returns>成功时返回 <see cref="TcpLinkerStream"/>，否则返回 <c>null</c>。</returns>
        public static TcpLinkerStream? GetStream(this ILinker linker)
        {
            if (linker is not ITcpLinker tcpLinker)
            {
                return null;
            }
            return tcpLinker.GetStream();
        }

        /// <summary>
        /// 将指定的 <see cref="ITcpLinker"/> 适配为 <see cref="TcpLinkerStream"/>。
        /// </summary>
        /// <param name="linker">要适配的 TCP 链路器（不得为 null）。</param>
        /// <returns>包装后的 <see cref="TcpLinkerStream"/> 实例。</returns>
        public static TcpLinkerStream GetStream(this ITcpLinker linker)
        {
            return new TcpLinkerStream(linker);
        }
    }
}