using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkChannels
{
    internal static class HttpLinkClientExtensions
    {
        public static IServiceCollection AddHttpLinkClient(this IServiceCollection services)
        {
            //services.AddLinkClient<IHttpLinkClient, HttpLinkClientFactory>();
            return services;
        }
    }
}