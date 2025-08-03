using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using ExtenderApp.Abstract;
using ExtenderApp.MainViews.ViewModels;
using ExtenderApp.Common;
using ExtenderApp.MainViews.Models;

namespace ExtenderApp.MainViews
{
    internal class MainViewStartup : Startup
    {
        public static string MainViewTitle = "Main";

        public override void AddService(IServiceCollection services)
        {
            services.AddTransient<IMainWindow, MainViewWindow>();
            services.AddSingleton<DisplayDetailsList>();

            AddMainView(services);

            services.AddTransient<PluginView>();
            services.AddTransient<PluginViewModle>();
            services.AddTransient<MianWindowViewModel>();
        }

        private void AddMainView(IServiceCollection services)
        {
            services.AddTransient<IMainView, MainView>();
            services.AddTransient<MainView>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainView_Run>();
            services.AddTransient<MainView_RunViewModel>();
            services.AddSingleton<MainModel>();
        }
    }
}
