using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;

namespace AppHost.Builder.Extensions
{
    internal static class HostedServiceExecutorExtensions
    {
        public static IHostApplicationBuilder AddHostedServiceExecutor(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<HostedServiceExecutor>();

            return builder; 
        }
    }
}
