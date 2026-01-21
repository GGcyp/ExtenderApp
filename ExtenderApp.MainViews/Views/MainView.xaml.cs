using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.View;
using ExtenderApp.MainViews.ViewModels;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews.Views
{
    /// <summary>
    /// MainViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : ExtenderAppView, IMainView
    {
        private IThemeManager themeManager;

        public MainView(IThemeManager themeManager)
        {
            InitializeComponent();
            this.themeManager = themeManager;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            themeManager.ApplyTheme("DarkTheme");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            themeManager.ApplyTheme("LightTheme");
            var item = Application.Current.Resources.MergedDictionaries.Count;
        }
    }
}