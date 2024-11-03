using System.Windows;

namespace MainApp
{
    internal class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //this.Resources.MergedDictionaries.Add(new() { Source = new("pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml") });
            //this.Resources.MergedDictionaries.Add(new() { Source = new("pack://application:,,,/HandyControl;component/Themes/Theme.xaml") });
            //this.Resources.MergedDictionaries.Add(new() { Source = new("pack://application:,,,/MainApp.Mods.PPR;PPRDictionary.xaml") });
        }
    }
}
