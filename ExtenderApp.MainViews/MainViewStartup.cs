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
            services.AddViewModel<MainWindowViewModel>();

            services.AddViewModel<PluginViewModle>();

            services.AddView<IMainView, MainView, MainViewModel>();

            services.AddViewModel<MainViewModel>(ServiceLifetime.Singleton);
            services.AddTransient<MainViewRunViewModel>();

            services.AddTransient<SettingsView>();
            services.AddTransient<SettingsViewModel>();

            services.AddSingleton<MainViewNavigation>();
        }
    }
}