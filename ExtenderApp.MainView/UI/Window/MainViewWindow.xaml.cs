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

namespace ExtenderApp.MainView
{
    /// <summary>
    /// MainViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainViewWindow : Window, IMainWindow
    {
        public IView View
        {
            get
            {
                if (!active) return null;
                return viewControl.Content as IView;
            }

            set
            {
                //if(value is not ContentControl contentControl)
                //    throw new ArgumentNullException("The view not be ContentControl");

                ArgumentNullException.ThrowIfNull(value, "The view null");

                if (!active)
                    new ArgumentNullException("MainWiodow not active");

                viewControl.Content = value;
            }
        }

        private bool active;

        public MainViewWindow()
        {
            active = false;
            InitializeComponent();
            active = true;
        }

    }
}
