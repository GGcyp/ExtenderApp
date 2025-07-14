using AppHost.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.Limiters
{
    internal static class LimiterExtensions
    {
        public static IServiceCollection AddLimiter(this IServiceCollection services)
        {
            services.AddSingleton<ResourceLimiter>();
            services.AddSingleton<ResourceLimitConfig>();
            return services;
        }
    }
}
