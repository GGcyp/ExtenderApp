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
            ViewModel<MainWindowViewModel>()!.Model.ShowMessage(
                message,
                horizontalAlignment,
                verticalAlignment,
                messageThickness);
            messageBehavior.ToggleVisibility();
            messageFontBehavior.ToggleVisibility();
        }
    }
}