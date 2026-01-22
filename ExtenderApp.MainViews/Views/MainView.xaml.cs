using System.Windows;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.View;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews.Views
{
    /// <summary>
    /// MainViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : ExtenderAppView, IMainView
    {
        private IThemeManager? themeManager;

        public MainView()
        {
            InitializeComponent();
            //this.themeManager = themeManager;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //themeManager.ApplyTheme("DarkTheme");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //themeManager.ApplyTheme("LightTheme");
            //var item = Application.Current.Resources.MergedDictionaries.Count;
        }
    }
}