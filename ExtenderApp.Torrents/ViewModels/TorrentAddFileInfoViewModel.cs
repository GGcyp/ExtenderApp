﻿using System.IO;
using System.Windows;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentAddFileInfoViewModel : ExtenderAppViewModel<TorrentAddFileInfoView, TorrentModel>
    {
        public TorrentInfo? CurrentTorrentInfo { get; set; }

        #region Command

        public NoValueCommand StartDownloadCommand { get; set; }
        public NoValueCommand SelectedAllCommand { get; set; }

        #endregion

        public TorrentAddFileInfoViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            StartDownloadCommand = new(SatrtDownload);
            SelectedAllCommand = new(() =>
            {
                CurrentTorrentInfo!.SelecrAllFiles();
            });
        }

        public override void Enter(ViewInfo oldViewInfo)
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
