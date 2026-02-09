using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentAddFileInfoViewModel : ExtenderAppViewModel<TorrentAddFileInfoView, TorrentModel>
    {
        public TorrentInfo? CurrentTorrentInfo { get; set; }

        #region Command

        public RelayCommand StartDownloadCommand { get; set; }
        public RelayCommand SelectedAllCommand { get; set; }

        #endregion Command

        public TorrentAddFileInfoViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            StartDownloadCommand = new(SatrtDownload);
            SelectedAllCommand = new(() =>
            {
                CurrentTorrentInfo!.SelecrAllFiles();
            });
        }

        protected override void EnterProtected(ViewInfo oldViewInfo)
        {
            var torrentAddViewModel = oldViewInfo.ViewModel as TorrentAddViewModel;
            CurrentTorrentInfo = torrentAddViewModel?.CurrentTorrentInfo;
            View.Window.Closed += (s, e) =>
            {
                CurrentTorrentInfo.UpdateDownloadState(false);
                MainWindowTopmost();
            };
        }

        private void SatrtDownload()
        {
            Task.Run(async () =>
            {
                if (Model.ContainsHash(CurrentTorrentInfo))
                {
                    DispatcherInvoke(() =>
                    {
                        var box = MessageBox.Show("已存在相同的种子，是否重新添加？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        View.Window?.Close();
                    });
                    MainWindowTopmost();
                    return;
                }

                await Model.SatrtTorrentAsync(CurrentTorrentInfo!);
                // 复制种子文件到保存目录
                var torrentSavePath = Path.Combine(CurrentTorrentInfo.SavePath, Path.GetFileName(CurrentTorrentInfo.TorrentPath));
                File.Copy(CurrentTorrentInfo.TorrentPath, torrentSavePath, true);
                CurrentTorrentInfo.TorrentPath = torrentSavePath;
                DispatcherInvoke(() =>
                {
                    View.Window?.Close();
                    Model.DowloadTorrentCollection!.Add(CurrentTorrentInfo!);
                    Model.SelectedTorrent = CurrentTorrentInfo;
                });
                MainWindowTopmost();
            });
        }
    }
}