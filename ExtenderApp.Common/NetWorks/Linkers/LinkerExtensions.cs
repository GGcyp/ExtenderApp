using System.Runtime.CompilerServices;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Common.Networks;
using ExtenderApp.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// 提供与链接（ILinker）及其依赖注册相关的扩展方法。
    /// </summary>
    public static class LinkerExtensions
    {
        /// <summary>
        /// 向服务集合中添加通用链接相关服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <returns>原服务集合（便于链式调用）。</returns>
        public static IServiceCollection AddLinker(this IServiceCollection services)
        {
            services.AddTcpLinker();
            services.AddUdpLinker();
            return services;
        }

        /// <summary>
        /// 添加指定类型的链接器及其工厂到服务集合中。
        /// </summary>
        /// <typeparam name="TLinker">指定类型连接器</typeparam>
        /// <typeparam name="TLinkerFactory">指定类型连接器工厂</typeparam>
        /// <param name="services">服务集合。</param>
        /// <returns>原服务集合（便于链式调用）。</returns>
        public static IServiceCollection AddLinker<TLinker, TLinkerFactory>(this IServiceCollection services)
            where TLinker : class, ILinker
            where TLinkerFactory : class, ILinkerFactory<TLinker>
        {
            services.AddSingleton<ILinkerFactory<TLinker>, TLinkerFactory>();
            services.AddTransient(provider =>
            {
                return provider.GetRequiredService<ILinkerFactory<TLinker>>().CreateLinker();
            });
            return services;
        }
    }
}