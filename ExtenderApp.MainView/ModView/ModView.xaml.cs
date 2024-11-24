using ExtenderApp.Abstract;
using ExtenderApp.Mod;
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

namespace ExtenderApp.MainView
{
    /// <summary>
    /// ModView.xaml 的交互逻辑
    /// </summary>
    public partial class ModView : UserControl, IView
    {
        private readonly ModViewModle _viewModle;
        public IViewModel ViewModel => _viewModle;

        public ModView(ModViewModle viewModle)
        {
            InitializeComponent();
            _viewModle = viewModle;
            ModTab.Callback = viewModle.OpenMod;
        }

        public void Enter(IView oldView)
        {
            var modStore = _viewModle.ModStore;
            for (int i = 0; i < modStore.Count; i++)
            {
                ModTab modTab = new ModTab(modStore[i]);
                modGrid.Children.Add(modTab);
            }
        }

        public void Exit(IView newView)
        {

        }
    }
}
