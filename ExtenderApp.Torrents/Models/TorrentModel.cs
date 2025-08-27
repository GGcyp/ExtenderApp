using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Models;
using ExtenderApp.Torrents.Models;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents
{
    public class TorrentModel : ExtenderAppModel
    {
        public ClientEngine Engine { get; set; }

        public string? SaveDirectory { get; set; }

        public TorrentInfo? SelectedTorrent { get; set; }

        public IView? TorrentListView { get; set; }

        public IView? TorrentDetailsView { get; set; }

        public ObservableCollection<TorrentInfo>? DowloadTorrentCollection { get; set; }
        public ObservableCollection<TorrentInfo>? DowloadCompletedTorrentCollection { get; set; }

        public void CreateTorrentClientEngine()
        {
            // 1. 创建引擎配置
            EngineSettingsBuilder builder = new()
            {
                AllowPortForwarding = true,
                AutoSaveLoadDhtCache = true,
                AutoSaveLoadFastResume = true,
                AutoSaveLoadMagnetLinkMetadata = true,
                ListenEndPoints = new Dictionary<string, IPEndPoint> {
                    { "ipv4", new IPEndPoint (IPAddress.Any, 55123) },
                    { "ipv6", new IPEndPoint (IPAddress.IPv6Any, 55123) }
                },
                DhtEndPoint = new IPEndPoint(IPAddress.Any, 55123),
            };

            // 2. 初始化引擎
            Engine = new ClientEngine(builder.ToSettings());
        }

        public async Task SatrtTorrentAsync(TorrentInfo info)
        {
            info.UpdateDownloadState();

            TorrentManager? manager = info.Manager;

            if (info.Torrent == null)
            {
                if (!File.Exists(info.TorrentPath))
                {
                    if (!MagnetLink.TryParse(info.TorrentMagnetLink, out var magnetLink))
                    {
                        throw new FileNotFoundException($"{info.Name}下载任务发生错误，无法找到种子文件及磁力链接，无法继续下载");
                    }
                    info.MagnetLink = magnetLink;
                }
                else
                {
                    info.Torrent = await Torrent.LoadAsync(info.TorrentPath);
                }
            }

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
                throw new ArgumentNullException($"种子下载任务{info.Name}的种子或磁力链接不能为空");
            }


            if (!string.IsNullOrEmpty(manager!.ContainingDirectory) && !Directory.Exists(manager.ContainingDirectory))
            {
                Directory.CreateDirectory(manager.ContainingDirectory);
            }
            info.Set(manager);
            info.IsDownloading = true;
            await manager.StartAsync();
            //await manager.LocalPeerAnnounceAsync();
            //await manager.DhtAnnounceAsync();
        }

        public async Task PauseTorrentAsync(TorrentInfo info)
        {
            var manager = info.Manager;
            if (manager == null)
                return;
            info.IsDownloading = false;
            await manager.PauseAsync();
        }

        public void UpdateTorrentInfo()
        {
            foreach (var torrent in DowloadTorrentCollection)
            {
                torrent.SimlpeUpdateInfo();
            }

            if (SelectedTorrent != null)
            {
                SelectedTorrent.UpdateInfo();
            }
        }

        public async Task<TorrentInfo> LoadTorrentAsync(string torrentPath, IDispatcherService service)
        {
            var torrent = await Torrent.LoadAsync(torrentPath);
            var info = new TorrentInfo(torrent, service);
            return info;
        }
    }
}
