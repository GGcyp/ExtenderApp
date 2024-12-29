using System;
using System.Collections.Generic;
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
using ExtenderApp.Views;

namespace ExtenderApp.MainViews
{
    /// <summary>
    /// MainView_Run.xaml 的交互逻辑
    /// </summary>
    public partial class MainView_Run : ExtenderAppView, IMainView
    {
        private MainViewModel _viewModel;
        public MainView_Run(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _viewModel.InjectView(this);
        }

        public void ShowView(IView view)
        {
            viewContenControl.Content = view;
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GoHomeView();
        }
    }
}
