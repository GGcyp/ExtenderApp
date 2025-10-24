using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public static class LinkClientBuilderExtensionos
    {
        public static IServiceCollection AddLinkClientBuilder(this IServiceCollection services)
        {
            //services.AddTransient(typeof(ClientBuilder<>), (p, o) =>
            //{
            //    if (o is not Type[] types)
            //        return null;

            //});

            return services;
        }

        #region FormatterManagerBuilder

        public static FormatterManagerBuilder AddBinaryFormatter<T>(this FormatterManagerBuilder builder)
        {
            var formatter = builder.Provider.GetRequiredService<BinaryLinkClientFormatter<T>>();
            builder.Manager.AddFormatter(formatter);
            return builder;
        }

        #endregion
    }
}
