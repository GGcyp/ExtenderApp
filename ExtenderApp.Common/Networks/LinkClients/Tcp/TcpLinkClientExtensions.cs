

using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    internal static class TcpLinkClientExtensions
    {
        public static IServiceCollection AddTcpLinkClient(this IServiceCollection services)
        {
            services.AddSingleton<ILinkClientFactory<ITcpLinkClient>, TcpLinkClientFactory>();
            services.AddTransient<ITcpLinkClient>(p =>
            {
                return p.GetRequiredService<ILinkClientFactory<ITcpLinkClient>>().CreateLinkClient();
            });
            return services;
        }
    }
}
