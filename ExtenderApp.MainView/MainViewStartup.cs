using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.MainView
{
    internal class MainViewStartup : Startup
    {
        protected override void AddService(IServiceCollection services)
        {
            services.AddSingleton<IMainWindow, MainViewWindow>();
            services.AddSingleton<IMainView, MainViewControl>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainModel>();
        }
    }
}
