using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.MainViews.ViewModels;
using ExtenderApp.MainViews.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.MainViews
{
    internal class MainViewStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddView<IMainWindow, MainWindow, MainWindowViewModel>();
            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<PluginViewModle>();

            services.AddView<IMainView, MainView, MainViewModel>();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<MainViewRunViewModel>();

            services.AddTransient<SettingsView>();
            services.AddTransient<SettingsViewModel>();

            services.AddSingleton<MainViewNavigation>();
        }
    }
}