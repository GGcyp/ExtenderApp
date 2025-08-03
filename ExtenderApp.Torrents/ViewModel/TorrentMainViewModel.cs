using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentMainViewModel : ExtenderAppViewModel<TorrentMainView, TorrentModel>
    {
        private readonly ScheduledTask _task;

        public TorrentMainViewModel(IMainWindow window, TorrentDownloadListView torrentDownloadListView, IServiceStore serviceStore) : base(serviceStore)
        {
            _task = new();
            TimeSpan outTime = TimeSpan.FromSeconds(1);
            _task.StartCycle(o => Model.UpdateTorrentInfo(), outTime, outTime);

            Model.DowloadTorrentCollection = new();
            Model.TorrentListView = torrentDownloadListView;

            Model.TorrentFileInfoView = NavigateTo<TorrentFileInfoView>();
            Model.TorrentDetailsView = Model.TorrentFileInfoView;

            //_serviceStore.MainWindow.MinWidth = 800;
            //_serviceStore.MainWindow.MinHeight = 600;
            window.MinWidth = 800;
            window.MinHeight = 600;

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
            Model.Engine = new ClientEngine(builder.ToSettings());
            //Model.Engine.DiskManager.PendingReadBytes;
            Model.SaveDirectory = _serviceStore.PathService.CreateFolderPathForAppRootFolder("test");
        }

        public void LoadTorrent(string torrentPath)
        {
            Task.Run(() => LoadTorrentAsync(torrentPath));
        }

        public async Task LoadTorrentAsync(string torrentPath)
        {
            var torrent = await Torrent.LoadAsync(torrentPath);
            var info = new TorrentInfo(torrent);
            _serviceStore.DispatcherService.Invoke(() =>
            {
                Model.DowloadTorrentCollection.Add(info);
            });
        }
    }
}
