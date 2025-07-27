using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Torrents.Models;
using ExtenderApp.ViewModels;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents
{
    public class TorrentMainViewModel : ExtenderAppViewModel<TorrentMainView, TorrentModel>
    {
        private readonly CancellationTokenSource _cts;
        public ObservableCollection<TorrentInfo> TorrentCollection { get; }
        public TorrentInfo SelectedTorrent
        {
            get => Model.SelectedTorrent;
            set => Model.SelectedTorrent = value;
        }

        public double TargetWidth { get; set; } = 350;

        public double ListItemWidth { get; set; }

        public TorrentMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            _cts = new();
            TorrentCollection = new();

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
            Model.Engine.DiskManager;
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
                TorrentCollection.Add(info);
            });
        }

        public async Task DownloadTorrentAsync(string torrentPath, string downloadFolder)
        {
            //// 1. 创建引擎配置
            //var settings = new EngineSettings
            //{
            //    ListenEndPoint = new IPEndPoint(IPAddress.Any, 6881),
            //    DiskIOBufferSize = 64 * 1024, // 64 KB
            //    MaximumDiskIOBufferSize = 1024 * 1024, // 1 MB
            //    UseDht = true // 启用分布式哈希表
            //};
            //var settings = new EngineSettings();

            //// 2. 初始化引擎
            //engine = new ClientEngine(settings);

            ////3.加载 torrent 文件
            //Torrent torrent = await torrent(torrentPath);

            ////4.创建下载配置
            //var managerSettings = new TorrentSettings
            //{
            //    MaximumConnections = 100,
            //    MaximumDownloadSpeed = 0, // 无限制
            //    MaximumUploadSpeed = 0, // 无限制
            //    UploadSlots = 4
            //};

            //// 5. 创建 torrent 管理器
            //manager = new TorrentManager(torrent, downloadFolder, managerSettings);

            //// 6. 注册事件监听
            //_manager.PeerConnected += Manager_PeerConnected;
            //_manager.PeerDisconnected += Manager_PeerDisconnected;
            //_manager.TorrentStateChanged += Manager_TorrentStateChanged;
            //_manager.StatsUpdated += Manager_StatsUpdated;

            //// 7. 注册到引擎并开始下载
            //await engine.Register(_manager);
            //cancellationToken = new CancellationTokenSource();

            //try
            //{
            //    await _manager.StartAsync();
            //    Console.WriteLine($"开始下载: {torrent.Name}");

            //    // 等待下载完成或取消
            //    await Task.Delay(-1, cancellationToken.Token);
            //}
            //catch (OperationCanceledException)
            //{
            //    Console.WriteLine("下载已取消");
            //}
            //finally
            //{
            //    // 清理资源
            //    await _manager.StopAsync();
            //    engine.Dispose();
            //}
        }
    }
}
