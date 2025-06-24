using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common
{
    public static class TcpLinkerExtensions
    {
        public static IServiceCollection AddTcpLinker(this IServiceCollection services)
        {
            services.AddTransient<ITcpLinker>(p =>
            {
                var factory = p.GetRequiredService<ILinkerFactory>();
                return factory.CreateLinker<ITcpLinker>();
            });
            return services;
        }
    }
}
