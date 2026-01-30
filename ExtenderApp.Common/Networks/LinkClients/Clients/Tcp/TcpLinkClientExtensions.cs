using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal static class TcpLinkClientExtensions
    {
        public static IServiceCollection AddTcpLinkClient(this IServiceCollection services)
        {
            services.AddLinkerClient<ITcpLinkClient, TcpLinkClientFactory>();
            return services;
        }
    }
}