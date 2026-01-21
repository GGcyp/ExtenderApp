using ExtenderApp.Abstract;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews.Windows
{
    /// <summary>
    /// ExtenderDefaultWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExtenderDefaultWindow : ExtenderAppWindow
    {
        public ExtenderDefaultWindow(IMessageService messageService) : base(messageService)
        {
            InitializeComponent();
        }
    }
}