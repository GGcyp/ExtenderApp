using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal static class LinkerClientExtensions
    {
        public static IServiceCollection AddLinkerClient(this IServiceCollection services)
        {
            services.AddScoped<LinkerClientFactory>();
            services.AddSingleton<LinkParser, ExtenderLinkParser>();
            services.AddTransient(typeof(LinkClient<>), typeof(LinkClient<>));

            return services;
        }
    }
}
