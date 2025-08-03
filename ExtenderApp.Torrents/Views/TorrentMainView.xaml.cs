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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.Torrents.ViewModels;
using ExtenderApp.Views;
using Microsoft.Win32;

namespace ExtenderApp.Torrents.Views
{
    /// <summary>
    /// TorrentMainView.xaml 的交互逻辑
    /// </summary>
    public partial class TorrentMainView : ExtenderAppView
    {
        public TorrentMainView(TorrentMainViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        private void AddTorrent(object sender, RoutedEventArgs e)
        {
            // 创建文件选择对话框实例
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 设置文件筛选器，仅显示 .torrent 文件
            openFileDialog.Filter = "Torrent 文件 (*.torrent)|*.torrent";
            // 打开文件选择对话框并获取用户选择结果
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // 用户选择了文件，获取文件路径
                string filePath = openFileDialog.FileName;
                // 这里可以添加处理选中种子文件的逻辑
                ViewModel<TorrentMainViewModel>()?.LoadTorrent(filePath);
            }
        }
    }
}
