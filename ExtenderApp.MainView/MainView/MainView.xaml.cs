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
        private readonly INavigationService _navigationService;
        public IViewModel ViewModel { get; }

        public MainViewControl(MainViewModel viewModel, INavigationService navigationService)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _navigationService = navigationService;
        }

        public void Enter(IView oldView)
        {
            navigationControl.Content = _navigationService.NavigateTo<ModView>();
        }

        public void Exit(IView newView)
        {
            
        }
    }
}
