using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

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
            services.AddTransient<IUdpLinker>(p =>
            {
                var factory = p.GetRequiredService<ILinkerFactory>();
                return factory.CreateLinker<IUdpLinker>();
            });
            return services;
        }
    }
}
