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
using ExtenderApp.Service;

namespace ExtenderApp.MainView
{
    /// <summary>
    /// MainViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainViewControl : UserControl, IMainView
    {
        private readonly MainViewModel _viewModel;
        public IViewModel ViewModel => ViewModel;

        public MainViewControl(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
        }

        public void Enter(IView oldView)
        {
            
        }

        public void Exit(IView newView)
        {
            
        }
    }
}
