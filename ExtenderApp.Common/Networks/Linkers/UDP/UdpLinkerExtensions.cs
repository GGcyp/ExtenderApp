
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    /// <summary>
    /// UdpLinkerExtensions 类的静态扩展方法集合。
    /// </summary>
    public static class UdpLinkerExtensions
    {
        /// <summary>
        /// 为IServiceCollection添加UdpLinker服务。
        /// </summary>
        /// <param name="services">IServiceCollection实例。</param>
        /// <returns>返回IServiceCollection实例。</returns>
        public static IServiceCollection AddUdpLinker(this IServiceCollection services)
        {
            services.AddSingleton<ILinkerFactory<IUdpLinker>, UdpLinkerFactory>();
            services.AddTransient(provider =>
            {
                return provider.GetRequiredService<ILinkerFactory<IUdpLinker>>().CreateLinker();
            });
            return services;
        }
    }
}
