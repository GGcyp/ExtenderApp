using AppHost.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Extensions.Hosting
{
    public static class HostedServiceExtensions
    {
        public static IServiceCollection AddHosted<THosted>(this IServiceCollection services) where THosted : class, IHostedService
        {
            services.AddSingleton<THosted>();
            return services;
        }
    }
}
