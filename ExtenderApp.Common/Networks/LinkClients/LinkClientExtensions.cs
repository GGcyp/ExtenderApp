using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal static class LinkClientExtensions
    {
        public static IServiceCollection AddLinkerClient(this IServiceCollection services)
        {
            services.AddScoped<LinkClientFactory>();
            //services.AddSingleton<LinkParser, ExtenderLinkParser>();
            //services.AddTransient(typeof(LinkClient<,>), typeof(LinkClient<,>));

            return services;
        }
    }
}
