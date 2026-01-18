using ExtenderApp.Media.ViewModles;
using ExtenderApp.Views;

namespace ExtenderApp.Media.Views
{
    /// <summary>
    /// MediaControlBarView.xaml 的交互逻辑
    /// </summary>
    public partial class MediaControlBarView : ExtenderAppView
    {
        public MediaControlBarView(MediaControlBarViewModle viewModle) : base(viewModle)
        {
            InitializeComponent();
        }
    }
}