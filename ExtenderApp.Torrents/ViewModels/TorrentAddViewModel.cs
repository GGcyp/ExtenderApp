using System.IO;
using System.Windows;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using Microsoft.Win32;
using MonoTorrent;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentAddViewModel : ExtenderAppViewModel<TorrentAddView, TorrentModel>
    {
        #region Command

        public NoValueCommand LoadTorrentCommand { get; set; }
        public NoValueCommand StartTorrentCommand { get; set; }

        #endregion

        private Lazy<ValueOrList<MagnetLink>> torrentsLazy;

        public string? MagnetLinksSting { get; set; }

        public TorrentInfo? CurrentTorrentInfo { get; private set; }

        public TorrentAddViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            torrentsLazy = new();
            LoadTorrentCommand = new(LoadTorrent);
            StartTorrentCommand = new(StartTorrent);
        }

        protected override void ProtectedEnter(ViewInfo newViewInfo)
        {
            View.Window.Closed += (s, e) => MainWindowTopmost();
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
                        CurrentTorrentInfo = await Model.LoadTorrentAsync(filePath, ServiceStore.DispatcherService);
                        DispatcherInvoke(() =>
                        {
                            var window = NavigateToWindow<TorrentAddFileInfoView>()!;
                            window.MinHeight = 400;
                            window.MinWidth = 400;
                            window.Height = 400;
                            window.Width = 350;
                            window.Owner = MainWindow;
                            window.WindowStartupLocation = 2;
                            window.Show();
                            View.Window?.Close();
                        });
                    });
                }
                catch (Exception ex)
                {
                    LogError(ex, $"种子文件数据有误：{openFileDialog.FileName}");
                    MessageBox.Show("种子文件数据有误");
                }
            }
        }

        private void StartTorrent()
        {
            if (!MagnetLink.TryParse(MagnetLinksSting, out var magnetLink))
            {
                throw new FileNotFoundException($"下载任务发生错误，无法解析磁力链接，无法继续下载");
            }

            Task.Run(async () =>
            {
                var manager = await Model.Engine.AddAsync(magnetLink, Model.SaveDirectory);
                await manager.WaitForMetadataAsync(Model.CancellationTokenSource.Token);
                View.Window?.Close();
            });
        }
    }
}
