using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews
{
    /// <summary>
    /// MainViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainViewWindow : ExtenderAppWindow, IMainWindow
    {
        public MainViewWindow(IMessageService messageService) : base(messageService)
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