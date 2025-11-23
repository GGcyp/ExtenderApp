using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal static class LinkClientBuilderExtensionos
    {
        public static IServiceCollection AddLinkClientBuilder(this IServiceCollection services)
        {
            services.AddTransient(typeof(LinkClientBuilder<>));

            return services;
        }

        #region FormatterManagerBuilder

        public static FormatterManagerBuilder AddBinaryFormatter<T>(this FormatterManagerBuilder builder, Action<T>? callback = null)
        {
            var formatter = builder.Provider.GetRequiredService<BinaryLinkClientFormatter<T>>();
            formatter.Receive += callback;
            builder.Manager.AddFormatter(formatter);
            return builder;
        }

        public static FormatterManagerBuilder AddJsonFormatter<T>(this FormatterManagerBuilder builder, Action<T>? callback = null)
        {
            var factory = builder.Provider.GetRequiredService<JsonLinkClientFormatterFactory>();
            var formatter = factory.CreateFormatter<T>();
            formatter.Receive += callback;
            builder.Manager.AddFormatter(formatter);
            return builder;
        }

        #endregion FormatterManagerBuilder
    }
}