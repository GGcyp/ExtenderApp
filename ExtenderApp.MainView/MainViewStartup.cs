using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;

namespace ExtenderApp.MainView
{
    internal class MainViewStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddSingleton<IMainWindow, MainViewWindow>();
            services.AddTransient<IMainView, MainViewControl>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<DisplayDetailsStore>();



            services.AddTransient<ModView>();
            services.AddTransient<ModViewModle>();
        }

        private void AddMainDisplayDetails(IServiceCollection services)
        {
            services.AddSingleton<DisplayDetailsStore>();
        }
    }
}
