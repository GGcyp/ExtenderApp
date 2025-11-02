using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal static class LinkClientExtensions
    {
        public static IServiceCollection AddLinkerClient(this IServiceCollection services)
        {
            services.AddTransient(typeof(BinaryLinkClientFormatter<>), (p, o) =>
            {
                if (o is not Type[] types)
                    return null;

                IBinaryFormatter formatter = p.GetRequiredService<IBinaryFormatterResolver>().GetFormatter(types[0]);
                return Activator.CreateInstance(typeof(BinaryLinkClientFormatter<>).MakeGenericType(types[0]), formatter);
            });
            services.AddTcpLinkClient();
            services.AddHttpLinkClient();

            return services;
        }
    }
}