using System.Windows;
using MainApp.ViewModels;


namespace MainApp.Views
{
    /// <summary>
    /// PPRView.xaml 的交互逻辑
    /// </summary>
    public partial class PPRView : Window,IPPRView
    {
        public PPRView(IPPRViewModel viewModel)
        {
            InitializeComponent();
        }
    }
}
