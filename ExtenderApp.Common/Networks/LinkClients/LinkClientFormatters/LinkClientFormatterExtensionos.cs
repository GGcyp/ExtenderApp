

using ExtenderApp.Abstract;
using ExtenderApp.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal static class LinkClientFormatterExtensionos
    {
        internal static IServiceCollection AddService(this IServiceCollection services)
        {
            services.AddTransient(typeof(BinaryLinkClientFormatter<>));
            services.AddSingleton<JsonLinkClientFormatterFactory>();
            return services;
        }

        public static ILinkClientFormatterManager AddBinaryFormatter<T>(this ILinkClientFormatterManager manager, Action<LinkClientReceivedValue<T>> callback)
        {

            return manager;
        }
    }
}
