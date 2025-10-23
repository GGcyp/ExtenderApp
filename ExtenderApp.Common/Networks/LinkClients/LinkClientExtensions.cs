using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal static class LinkClientExtensions
    {
        public static IServiceCollection AddLinkerClient(this IServiceCollection services)
        {
            services.AddScoped<LinkClientFactory>();
            services.AddTransient(typeof(BinaryClientFormatter<>), (p, o) =>
            {
                if (o is not Type[] types)
                    return null;

                IByteBufferFactory byteBuffer = p.GetRequiredService<IByteBufferFactory>();
                IBinaryFormatter formatter = p.GetRequiredService<IBinaryFormatterResolver>().GetFormatter(types[0]);
                return Activator.CreateInstance(typeof(BinaryClientFormatter<>).MakeGenericType(types[0]), byteBuffer, formatter);
            });
            //services.AddSingleton<LinkParser, ExtenderLinkParser>();
            //services.AddTransient(typeof(LinkClient<,>), typeof(LinkClient<,>));

            return services;
        }
    }
}
