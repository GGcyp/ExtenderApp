using System.Collections.ObjectModel;
using System.Reflection;
using AppHost.Builder;
using AppHost.Extensions.DependencyInjection;
using AppHost.Extensions.Hosting;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.View;
using ExtenderApp.Views.Clipboards;
using ExtenderApp.Views.CutsceneViews;
using ExtenderApp.Views.Themes;

namespace ExtenderApp.Views
{
    internal class ViewStartup : Startup
    {
        public override void AddService(IServiceCollection services)
        {
            services.AddHosted<MainViewHostedService>();
            services.AddSingleton<IClipboard, Clipboard_WPF>();
            services.AddTransient<CutsceneView>();
            AddThemeManager(services);


            services.Configuration<IBinaryFormatterStore>(s =>
            {
                s.AddFormatter(typeof(ObservableCollection<>), typeof(ObservableCollectionFormatter<>));
            });
        }

        private void AddThemeManager(IServiceCollection services)
        {
            ThemeManager themeManager = new();
            themeManager.RegisterTheme("DarkTheme", "Themes/Global/DarkTheme.xaml");
            themeManager.RegisterTheme("LightTheme", "Themes/Global/LightTheme.xaml");
            services.AddSingleton<IThemeManager>(themeManager);
        }
    }
}
