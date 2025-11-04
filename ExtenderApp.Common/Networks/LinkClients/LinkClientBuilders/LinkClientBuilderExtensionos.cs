using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    public static class LinkClientBuilderExtensionos
    {
        public static IServiceCollection AddLinkClientBuilder(this IServiceCollection services)
        {
            //services.AddTransient(typeof(ClientBuilder<>), (p, o) =>
            //{
            //    if (o is not StartupType[] types)
            //        return null;

            //});

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

        #endregion FormatterManagerBuilder
    }
}