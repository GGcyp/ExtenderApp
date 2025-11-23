

using ExtenderApp.Abstract;
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

        public static ILinkClientFormatterManager AddBinaryLinkClientFormatters<T>(this ILinkClientFormatterManager manager, IServiceProvider provider, Action<T>? action = null)
        {
            var formatter = provider.GetRequiredService<BinaryLinkClientFormatter<T>>();

            if (action != null)
            {
                formatter.Receive += action;
            }

            manager.AddFormatter(formatter);
            return manager;
        }
    }
}
