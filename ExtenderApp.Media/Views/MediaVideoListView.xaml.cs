using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExtenderApp.Media.ViewModels;
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

        private void AddLocalVideo_Click(object sender, RoutedEventArgs e)
        {
            // 添加本地视频的逻辑
            var openFileDialog = new OpenFileDialog
            {
                Title = "打开视频文件",
                Filter = "视频文件|*.mp4;*.avi;*.mkv;*.mov;*.wmv|所有文件|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFilePaths = openFileDialog.FileNames;
                // 将选中的视频路径添加到 GetViewModel
                GetViewModel<MediaMainViewModel>()!.AddMediaInfo(selectedFilePaths);
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
    }
}