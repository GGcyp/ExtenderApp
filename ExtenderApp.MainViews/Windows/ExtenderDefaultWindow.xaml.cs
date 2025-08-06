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
using ExtenderApp.Abstract;
using ExtenderApp.MainViews.ViewModels;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews.Windows
{
    /// <summary>
    /// ExtenderDefaultWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExtenderDefaultWindow : ExtenderAppWindow
    {
        public ExtenderDefaultWindow(ExtenderDefaultWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
