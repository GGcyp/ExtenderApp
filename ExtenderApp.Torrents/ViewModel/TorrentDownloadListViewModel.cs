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
        public GenericCommand<TorrentInfo> StartCommand { get; set; }

        public TorrentDownloadListViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            StartCommand = new(TorrentStateChang);
        }

        private void TorrentStateChang(TorrentInfo info)
        {
            if (!info.IsDownloading)
            {
                Task.Run(async () =>
                {
                    await Model.SatrtTorrentAsync(info);
                    info.IsDownloading = true;
                    InfoHashes infoHashes = info.Torrent == null ? info.MagnetLink.InfoHashes : info.Torrent.InfoHashes;
                    Info($"开始下载: 种子名字：{info.Name}，种子哈希值：{infoHashes.V1OrV2.ToHex()}");
                });
            }
            else
            {
                Task.Run(async () =>
                {
                    await Model.PauseTorrentAsync(info);
                    info.IsDownloading = false;
                    InfoHashes infoHashes = info.Torrent == null ? info.MagnetLink.InfoHashes : info.Torrent.InfoHashes;
                    Info($"暂停下载: 种子名字：{info.Name}，种子哈希值：{infoHashes.V1OrV2.ToHex()}");
                });
            }
        }
    }
}
