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
    public partial class MainViewWindow : ExtenderAppWindow, IMainWindow
    {
        public MainViewWindow(MainWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public void DisplayMessageToMainWindow(string message,
            ExHorizontalAlignment horizontalAlignment,
            ExVerticalAlignment verticalAlignment,
            ExThickness messageThickness)
        {
            ViewModel<MainWindowViewModel>().Model.ShowMessage(
                message,
                horizontalAlignment,
                verticalAlignment,
                messageThickness);
            messageBehavior.ToggleVisibility();
            messageFontBehavior.ToggleVisibility();
        }
    }
}
