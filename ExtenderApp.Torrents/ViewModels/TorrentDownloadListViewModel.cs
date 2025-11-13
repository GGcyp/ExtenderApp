using System.IO;
using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using MonoTorrent;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentDownloadListViewModel : ExtenderAppViewModel<TorrentDownloadListView, TorrentModel>
    {
        internal IList<object>? Selecteds { get; set; }

        public string? SeletedState { get; set; }

        private bool downloadingState;

        #region Command

        public RelayCommand<TorrentInfo> StartCommand { get; set; }

        public NoValueCommand StartSelectedsCommand { get; set; }

        public NoValueCommand DeleteCommand { get; set; }

        public NoValueCommand OpenFolderCommand { get; set; }

        public NoValueCommand CopyMagnetLinkCommand { get; set; }

        public NoValueCommand PermanentlyDeleteCommand { get; set; }

        #endregion

        public TorrentDownloadListViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            StartCommand = new(TorrentStateChang);
            StartSelectedsCommand = new(StartSelecteds);
            DeleteCommand = new(DeleteSelecteds);
            OpenFolderCommand = new(OpenFolder);
            CopyMagnetLinkCommand = new(CopyMagnetLink);
            PermanentlyDeleteCommand = new(PermanentlyDelete);
        }

        private void PermanentlyDelete()
        {
            if (Selecteds == null || Selecteds.Count == 0)
            {
                return;
            }

            for (int i = 0; i < Selecteds.Count; i++)
            {
                if (Selecteds[i] is TorrentInfo info)
                {
                    Task.Run(async () =>
                    {
                        LogInformation($"开始删除种子: 种子名字：{info.Name}，种子哈希值：{info.V1orV2}");
                        await Model.RemoveTorrentAsync(info);
                        try
                        {
                            if (Directory.Exists(info.SavePath))
                            {
                                Directory.Delete(info.SavePath, true);
                                LogInformation($"删除种子文件夹成功: 种子名字：{info.Name}，种子哈希值：{info.V1orV2}");
                                DispatcherBeginInvoke(() => { Model.DowloadTorrentCollection.Remove(info); });
                                return;
                            }
                            LogInformation("未找到种子文件夹，可能已被手动删除。");
                        }
                        catch (IOException ex)
                        {
                            LogError(ex, $"删除种子文件夹失败: 种子名字：{info.Name}，种子哈希值：{info.V1orV2}，错误信息：{ex.Message}");
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            LogError(ex, $"删除种子文件夹失败: 种子名字：{info.Name}，种子哈希值：{info.V1orV2}，错误信息：{ex.Message}");
                        }
                    });
                }
            }
        }

        private void CopyMagnetLink()
        {
            if (Selecteds == null || Selecteds.Count == 0)
            {
                return;
            }
            var info = Selecteds[0] as TorrentInfo;

            ClipboardSetText(info.TorrentMagnetLink);
        }

        private void OpenFolder()
        {
            if (Selecteds == null || Selecteds.Count == 0)
            {
                return;
            }
            var info = Selecteds[0] as TorrentInfo;

            OpenFolder(info!.SavePath);
        }

        private void DeleteSelecteds()
        {
            if (Selecteds == null || Selecteds.Count == 0)
            {
                return;
            }

            for (int i = 0; i < Selecteds.Count; i++)
            {
                if (Selecteds[i] is TorrentInfo info)
                {
                    Task.Run(async () =>
                    {
                        LogInformation($"开始删除种子: 种子名字：{info.Name}，种子哈希值：{info.V1orV2}");
                        DispatcherBeginInvoke(() =>
                        {
                            Model.DowloadTorrentCollection.Remove(info);
                            Model.RecycleBinCollection.Add(info);
                        });
                        await Model.RemoveTorrentAsync(info);
                    });
                }
            }
        }

        public void UpdateSeletedState()
        {
            if (Selecteds == null || Selecteds.Count == 0)
            {
                return;
            }

            for (int i = 0; i < Selecteds.Count; i++)
            {
                if (Selecteds[i] is TorrentInfo info)
                {
                    if (!info.IsDownloading)
                    {
                        SeletedState = "开始下载";
                        downloadingState = true;
                        return;
                    }
                }
            }
            downloadingState = false;
            SeletedState = "暂停下载";
        }

        private void StartSelecteds()
        {
            Task.Run(async () =>
            {
                for (int i = 0; i < Selecteds.Count; i++)
                {
                    if (Selecteds[i] is TorrentInfo info)
                    {
                        TorrentStateChang(info, downloadingState);
                    }
                }
            });
        }

        private void TorrentStateChang(TorrentInfo info)
        {
            TorrentStateChang(info, !info.IsDownloading);
        }

        private void TorrentStateChang(TorrentInfo info, bool downloadState)
        {
            if (info.IsDownloading == downloadState)
            {
                return;
            }

            if (downloadState)
            {
                Task.Run(async () =>
                {
                    await Model.SatrtTorrentAsync(info);
                    LogInformation($"开始下载: 种子名字：{info.Name}，种子哈希值：{info.V1orV2}");
                });
            }
            else
            {
                Task.Run(async () =>
                {
                    await Model.PauseTorrentAsync(info);
                    DispatcherBeginInvoke(() =>
                    {
                        info.ConnectPeers.Clear();
                        info.Trackers.Clear();
                    });
                    LogInformation($"暂停下载: 种子名字：{info.Name}，种子哈希值：{info.V1orV2}");
                });
            }
        }
    }
}
