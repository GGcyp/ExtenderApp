using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    internal static class UdpLinkClientExtensions
    {
        public static IServiceCollection AddUdpLinkClient(this IServiceCollection services)
        {
            services.AddSingleton<ILinkClientFactory<IUdpLinkClient>, UdpLinkClientFactory>();
            services.AddTransient(p =>
            {
                return p.GetRequiredService<ILinkClientFactory<IUdpLinkClient>>().CreateLinkClient();
            });
            return services;
        }
    }
}
