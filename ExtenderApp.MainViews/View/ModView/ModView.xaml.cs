using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Views;
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

namespace ExtenderApp.MainViews
{
    /// <summary>
    /// ModView.xaml 的交互逻辑
    /// </summary>
    public partial class ModView : ExtenderAppView
    {
        private readonly ModViewModle _viewModle;

        public ModView(ModViewModle viewModle)
        {
            InitializeComponent();
            _viewModle = viewModle;
            _viewModle.InjectView(this);
            ModTab.Callback = viewModle.OpenMod;
        }
    }
}
