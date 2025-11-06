using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
