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

            var modDetails = new Mod.ModDetails(new Mods.ModeInfo() { ModTitle = "测试", ModDescription = "简介", ModVersion = "0.0.1" });
            modGrid.Children.Add(CreateModTab(modDetails));
            modGrid.Children.Add(CreateModTab(modDetails));
            modGrid.Children.Add(CreateModTab(modDetails));
            modGrid.Children.Add(CreateModTab(modDetails));
            modGrid.Children.Add(CreateModTab(modDetails));
        }

        private ModTab CreateModTab(ModDetails modDetails)
        {
            ModTab modTab = new ModTab(modDetails);
            return modTab;
        }

        public void Enter(IView oldView)
        {
            
        }

        public void Exit(IView newView)
        {
            
        }
    }
}
