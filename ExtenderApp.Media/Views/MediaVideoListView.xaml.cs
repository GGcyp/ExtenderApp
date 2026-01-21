using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExtenderApp.Media.ViewModles;
using ExtenderApp.Views;
using Microsoft.Win32;

namespace ExtenderApp.Media.Views
{
    /// <summary>
    /// MediaVideoListView.xaml 的交互逻辑
    /// </summary>
    public partial class MediaVideoListView : ExtenderAppView
    {
        public MediaVideoListView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 打开视频列表的方法
        /// </summary>
        /// <param name="sender">事件触发源</param>
        /// <param name="e">事件参数</param>
        private void OpenVideoForList(object sender, MouseButtonEventArgs e)
        {
            //if (sender is TextBlock textBlock && textBlock.DataContext is VideoInfo videoInfo)
            //{
            //}
        }

        private void AddLocalVideo_Click(object sender, RoutedEventArgs e)
        {
            // 添加本地视频的逻辑
            var openFileDialog = new OpenFileDialog
            {
                Title = "打开视频文件",
                Filter = "视频文件|*.mp4;*.avi;*.mkv;*.mov;*.wmv|所有文件|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFilePaths = openFileDialog.FileNames;
                // 将选中的视频路径添加到 GetViewModel
                GetViewModel<VideoListViewModel>()!.AddVideoPaths(selectedFilePaths);
            }
        }

        private void AddOnlineVideo_Click(object sender, RoutedEventArgs e)
        {
            // 添加网络视频的逻辑
        }

        private void DeleteVideo_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void DeleteSelectedVideo_Click(object sender, RoutedEventArgs e)
        {
            // 删除选中视频的逻辑
        }

        private void DeleteAllVideos_Click(object sender, RoutedEventArgs e)
        {
            // 删除所有视频的逻辑
        }
    }
}