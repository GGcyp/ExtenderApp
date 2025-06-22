using AppHost.Extensions.DependencyInjection;

namespace ExtenderApp.Common
{
    public static class TcpLinkerExtensions
    {
        public static IServiceCollection AddTcpLinker(this IServiceCollection services)
        {
            return services;
        }
    }
}
