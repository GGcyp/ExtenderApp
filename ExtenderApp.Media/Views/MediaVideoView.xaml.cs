using ExtenderApp.Media.ViewModles;
using ExtenderApp.Views;

namespace ExtenderApp.Media.Views
{
    /// <summary>
    /// MediaVideoView.xaml 的交互逻辑
    /// </summary>
    public partial class MediaVideoView : ExtenderAppView
    {
        public MediaVideoView(MediaVideoViewModle viewModle) : base(viewModle)
        {
            InitializeComponent();
        }
    }
}