using System.Collections.ObjectModel;
using System.ComponentModel;
using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Models;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents
{
    public class TorrentModel : INotifyPropertyChanged
    {
        public ClientEngine? Engine { get; set; }

        public string? SaveDirectory { get; set; }

        public TorrentInfo? SelectedTorrent { get; set; }

        public IView? TorrentListView { get; set; }

        public IView? TorrentDetailsView { get; set; }

        internal IView TorrentFileInfoView { get; set; }

        public ObservableCollection<TorrentInfo> DowloadTorrentCollection { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task SatrtTorrentAsync(TorrentInfo info)
        {
            TorrentManager? manager = info.Manager;
            if (manager != null)
            {
                await manager.StartAsync();
                return;
            }

            if (info.Torrent != null)
            {
                manager = await Engine.AddAsync(info.Torrent, SaveDirectory);
            }
            else if (info.MagnetLink != null)
            {
                manager = await Engine.AddAsync(info.MagnetLink, SaveDirectory);
            }
            else
            {
                throw new ArgumentNullException($"当前种子或磁力链接还未加载：{info.Torrent}");
            }

            info.Set(manager);
            await manager.StartAsync();
            await manager.LocalPeerAnnounceAsync();
            await manager.DhtAnnounceAsync();
        }

        public async Task PauseTorrentAsync(TorrentInfo info)
        {
            var manager = info.Manager;
            if (manager == null)
                return;
            await manager.PauseAsync();
        }

        public void UpdateTorrentInfo()
        {
            foreach (var torrent in DowloadTorrentCollection)
            {
                var manager = torrent.Manager;
                if (manager == null)
                    continue;

                torrent.Progress = manager.Progress;
                torrent.DownloadSpeed = manager.Monitor.DownloadRate;
                torrent.UploadSpeed = manager.Monitor.UploadRate;
                foreach (var info in torrent.Files)
                {
                    info.UpdetaProgress();
                }
            }
        }
    }
}
