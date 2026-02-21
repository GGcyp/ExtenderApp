using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal static class UdpLinkClientExtensions
    {
        public static IServiceCollection AddUdpLinkClient(this IServiceCollection services)
        {
            //services.AddLinkClient<IUdpTransferLinkClient, UdpLinkClientFactory>();
            return services;
        }
    }
}