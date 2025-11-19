using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    internal static class LinkClientExtensions
    {
        public static IServiceCollection AddLinkerClient(this IServiceCollection services)
        {
            services.AddTransient(typeof(BinaryLinkClientFormatter<>), typeof(BinaryLinkClientFormatter<>));
            services.AddTcpLinkClient();
            services.AddUdpLinkClient();
            services.AddHttpLinkClient();

            return services;
        }
    }
}