using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using MainApp.Abstract;

namespace MainApp.MainView
{
    internal class MainViewStartup : Startup
    {
        protected override void AddService(IServiceCollection services)
        {
            services.AddSingleton<IMainViewWindow, MainViewWindow>();
            services.AddSingleton<MainViewModel>();
        }
    }
}
