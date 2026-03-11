using ExtenderApp.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkChannels
{
    internal static class LinkClientBuilderExtensionos
    {
        public static IServiceCollection AddLinkClientBuilder(this IServiceCollection services)
        {
            //services.AddTransient(typeof(LinkClientBuilder<>));

            return services;
        }
    }
}