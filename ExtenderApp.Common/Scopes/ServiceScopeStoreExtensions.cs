using ExtenderApp.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Scopes
{
    internal static class ServiceScopeStoreExtensions
    {
        public static IServiceCollection AddServiceScopeStore(this IServiceCollection services)
        {
            services.AddSingleton<IServiceScopeStore, ServiceScopeStore>();
            return services;
        }
    }
}