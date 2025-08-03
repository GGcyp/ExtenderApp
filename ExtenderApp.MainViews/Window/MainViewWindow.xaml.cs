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
using ExtenderApp.Data;
using ExtenderApp.MainViews.ViewModels;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews
{
    /// <summary>
    /// MainViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainViewWindow : Window, IMainWindow, IView
    {
        /// <summary>
        /// 获取视图信息
        /// </summary>
        public ViewInfo ViewInfo { get; }

        /// <summary>
        /// 视图模型私有只读变量
        /// </summary>
        private readonly MianWindowViewModel _viewModel;

        public MainViewWindow(MianWindowViewModel viewModel)
        {
            ViewInfo = new ViewInfo(GetType().Name);
            DataContext = viewModel;
            _viewModel = viewModel;
            viewModel.InjectView(this);
            InitializeComponent();
        }

        public void Enter(ViewInfo oldViewInfo)
        {

        }

        public void Exit(ViewInfo newViewInfo)
        {

        }

        public void ShowView(IMainView mainView)
        {
            _viewModel.ShowView(mainView);
        }
    }
}
