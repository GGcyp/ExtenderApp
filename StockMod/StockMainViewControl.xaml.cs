using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

namespace StockMod
{
    /// <summary>
    /// StockMainViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class StockMainViewControl : UserControl, IView
    {
        private readonly StockMainViewModel viewModel;
        public IViewModel ViewModel => viewModel;

        public StockMainViewControl(StockMainViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            Debug.Print("生成成功" + nameof(StockMainViewControl));
        }

        public void Enter(IView oldView)
        {

        }

        public void Exit(IView newView)
        {

        }
    }
}
