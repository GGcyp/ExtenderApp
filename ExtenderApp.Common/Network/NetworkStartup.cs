using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Network
{
    public static class NetworkStartup
    {
        public static IServiceCollection AddNetwork(this IServiceCollection services)
        {
            //services.AddSingleton<INetworkClient, HttpClient>();
            services.AddSingleton<INetWorkProider, NetWorkProider>();
            return services;
        }
    }
}
