using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Service
{
    public class ServiceStartup : Startup
    {
        protected override void AddService(IServiceCollection services)
        {
            services.AddSingleton<INavigationService, NavigationService>();
        }
    }
}
