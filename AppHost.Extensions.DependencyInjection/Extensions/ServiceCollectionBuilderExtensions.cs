using System.Collections;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost.Extensions.DependencyInjection
{
    public static class ServiceCollectionBuilderExtensions
    {
        /// <summary>
        /// 用<see cref="IServiceCollection"/>创建一个服务提供器<see cref="ServiceProvider"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns><see cref="ServiceProvider"/></returns>
        public static IServiceProvider BuildServiceProvider(this IServiceCollection services)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }


            return new ServiceProvider(services);
        }
    }
}
