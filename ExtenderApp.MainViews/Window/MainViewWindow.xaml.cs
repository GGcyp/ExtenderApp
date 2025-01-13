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
using ExtenderApp.Views;

namespace ExtenderApp.MainViews
{
    /// <summary>
    /// MainViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainViewWindow : Window, IMainWindow
    {
        private bool active;

        public MainViewWindow()
        {
            active = false;
            InitializeComponent();
            active = true;
        }

        public void ShowView(IView view)
        {
            ArgumentNullException.ThrowIfNull(view, "The view null");

            if(view is not IMainView)
                throw new InvalidCastException(nameof(IView));

            if (!active)
                 new ArgumentNullException("MainWiodow not active");

            viewControl.Content = view;
        }
    }
}
