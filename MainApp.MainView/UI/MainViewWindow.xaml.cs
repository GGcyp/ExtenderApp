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
using System.Windows.Shapes;
using MainApp.Abstract;

namespace MainApp.MainView
{
    /// <summary>
    /// MainViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainViewWindow : Window,IMainView
    {
        private readonly MainViewModel _model;
        public IViewModel ViewModel => _model;

        public MainViewWindow(MainViewModel model)
        {
            InitializeComponent();

            _model = model;
            DataContext = model;
        }

    }
}
