using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Threads
{
    internal static class MainThreadContextExtensions
    {
        public static IServiceCollection AddMainThreadContext(this IServiceCollection services)
        {
            services.AddSingleton<IMainThreadContext, MainThreadContext>();
            return services;
        }
    }
}