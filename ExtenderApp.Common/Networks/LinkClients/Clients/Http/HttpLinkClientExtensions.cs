using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    internal static class HttpLinkClientExtensions
    {
        public static IServiceCollection AddHttpLinkClient(this IServiceCollection services)
        {
            services.AddSingleton<ILinkClientFactory<IHttpLinkClient>, HttpLinkClientFactory>();
            services.AddTransient<IHttpLinkClient>(p =>
            {
                return p.GetRequiredService<ILinkClientFactory<IHttpLinkClient>>().CreateLinkClient();
            });
            return services;
        }
    }
}
