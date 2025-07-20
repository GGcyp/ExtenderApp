using AppHost.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.Limiters
{
    /// <summary>
    /// 限流器扩展类
    /// </summary>
    internal static class LimiterExtensions
    {
        /// <summary>
        /// 向服务集合中添加限流器服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <returns>返回添加服务后的服务集合</returns>
        public static IServiceCollection AddLimiter(this IServiceCollection services)
        {
            services.AddSingleton<ResourceLimiter>();
            services.AddSingleton<ResourceLimitConfig>();
            return services;
        }
    }
}
