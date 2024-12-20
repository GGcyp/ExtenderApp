using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Service
{
    public class ServiceStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddSingleton<IServiceStore, ServiceStore>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ITemporarilyService, TemporarilyService>();
        }
    }
}
