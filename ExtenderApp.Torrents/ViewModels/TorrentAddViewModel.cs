using System.Windows;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using Microsoft.Win32;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentAddViewModel : ExtenderAppViewModel<TorrentAddView, TorrentModel>
    {
        private readonly IMainWindow _mainWindow;

        #region Command

        public NoValueCommand LoadTorrentCommand { get; set; }
        public NoValueCommand StartTorrentCommand { get; set; }

        #endregion

        private Lazy<ValueOrList<MagnetLink>> torrentsLazy;

        public string? MagnetLinksSting { get; set; }

        public TorrentInfo? CurrentTorrentInfo { get; private set; }

        public TorrentAddViewModel(IMainWindow mainWindow, IServiceStore serviceStore) : base(serviceStore)
        {
            _mainWindow = mainWindow;
            torrentsLazy = new();
            LoadTorrentCommand = new(LoadTorrent);
            StartTorrentCommand = new(StartTorrent);
        }

        private void LoadTorrent()
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
                try
                {
                    Task.Run(async () =>
                    {
                        CurrentTorrentInfo = await Model.LoadTorrentAsync(filePath);
                        DispatcherInvoke(() =>
                        {
                            var window = NavigateToWindow<TorrentAddFileInfoView>()!;
                            window.MinHeight = 400;
                            window.MinWidth = 400;
                            window.Height = 400;
                            window.Width = 350;
                            window.Owner = _mainWindow;
                            window.WindowStartupLocation = 2;
                            window.Show();
                            View.Window?.Close();
                        });
                    });
                }
                catch (Exception ex)
                {
                    Error($"种子文件数据有误：{openFileDialog.FileName}", ex);
                    MessageBox.Show("种子文件数据有误");
                }
            }
        }

        private void StartTorrent()
        {
            var list = torrentsLazy.Value;
            for (int i = 0; i < list.Count; i++)
            {
                //TorrentInfo info = torrents[i];
                //Model.DowloadTorrentCollection.Add(info);
            }
            View.Window?.Close();
        }
    }
}
