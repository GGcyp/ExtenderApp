
using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    internal static class TcpLinkClientExtensions
    {
        public static IServiceCollection AddTcpLinkClient(this IServiceCollection services)
        {
            services.AddSingleton<ILinkClientFactory<ITcpLinkClient>, TcpLinkClientFactory>();
            services.AddTransient(p =>
            {
                return p.GetRequiredService<ILinkClientFactory<ITcpLinkClient>>().CreateLinkClient();
            });
            return services;
        }
    }
}