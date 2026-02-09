using ExtenderApp.Abstract;
using ExtenderApp.Contracts;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : ExtenderAppWindow, IMainWindow
    {
        public MainWindow(IMessageService messageService) : base(messageService)
        {
            InitializeComponent();
        }

        public void DisplayMessageToMainWindow(string message,
            ExHorizontalAlignment horizontalAlignment,
            ExVerticalAlignment verticalAlignment,
            ExThickness messageThickness)
        {
            //ViewModel<MainWindowViewModel>()!.Model.ShowMessage(
            //    message,
            //    horizontalAlignment,
            //    verticalAlignment,
            //    messageThickness);
            //messageBehavior.ToggleVisibility();
            //messageFontBehavior.ToggleVisibility();
        }
    }
}