using System.Windows;
using AppHost.Extensions.Hosting;

namespace ExtenderApp
{
    internal class App : Application
    {
        private IMainThreadContext? mainThreadContext;

        public App(IMainThreadContext? mainThreadContext)
        {
            this.mainThreadContext = mainThreadContext;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            mainThreadContext.InitMainThreadContext();
            //Resources.MergedDictionaries.Add(new() { Source = new("pack://application:,,,/ExtenderApp.Views;component/Themes/Global/DarkTheme.xaml") });
        }
    }
}
