using System.Collections.ObjectModel;
using System.IO;
using ExtenderApp.Abstract;
using ExtenderApp.Contracts;
using ExtenderApp.Models;
using ExtenderApp.Torrents.Models;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents
{
    /// <summary>
    /// 种子管理模型，负责管理种子任务、设置、状态及相关视图。
    /// 提供种子启动、暂停、停止、移除、加载、设置更新等核心操作。
    /// </summary>
    public class TorrentModel : ExtenderAppModel
    {
        /// <summary>
        /// 临时存储已完成的种子任务列表。
        /// </summary>
        private readonly List<TorrentInfo> _tempList;

        /// <summary>
        /// 已管理的种子哈希集合，用于去重和快速查找。
        /// </summary>
        internal HashSet<HashValue> InfoHashHashSet { get; set; }

        /// <summary>
        /// 取消令牌源，用于控制异步操作的取消。
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; } = new();

        /// <summary>
        /// 种子下载引擎实例。
        /// </summary>
        public ClientEngine Engine { get; private set; }

        /// <summary>
        /// 当前种子设置模型（用于编辑和应用设置）。
        /// </summary>
        public TorrentSettingsBuilderModel TorrentSettingsModel { get; set; }

        /// <summary>
        /// 展示用的种子设置模型（用于界面显示）。
        /// </summary>
        public TorrentSettingsBuilderModel DisplayTorrentSettingsModel { get; set; }

        /// <summary>
        /// 当前引擎设置模型（用于编辑和应用设置）。
        /// </summary>
        public EngineSettingsBuilderModel EngineSettingsModel { get; set; }

        /// <summary>
        /// 展示用的引擎设置模型（用于界面显示）。
        /// </summary>
        public EngineSettingsBuilderModel DisplayEngineSettingsModel { get; set; }

        /// <summary>
        /// 种子文件保存目录。
        /// </summary>
        public string? SaveDirectory { get; set; }

        /// <summary>
        /// 当前应用的种子设置。
        /// </summary>
        private TorrentSettings currentTorrentSettngs;

        /// <summary>
        /// 当前应用的引擎设置。
        /// </summary>
        private EngineSettings currentEngineSettings;

        /// <summary>
        /// 当前选中的种子任务。
        /// </summary>
        public TorrentInfo? SelectedTorrent { get; set; }

        /// <summary>
        /// 种子列表视图。
        /// </summary>
        public IView? TorrentListView { get; set; }

        /// <summary>
        /// 种子详情视图。
        /// </summary>
        public IView? TorrentDetailsView { get; set; }

        /// <summary>
        /// 正在下载的种子任务集合。
        /// </summary>
        public ObservableCollection<TorrentInfo>? DowloadTorrentCollection { get; set; }

        /// <summary>
        /// 已完成下载的种子任务集合。
        /// </summary>
        public ObservableCollection<TorrentInfo>? DowloadCompletedTorrentCollection { get; set; }

        /// <summary>
        /// 做种任务集合。
        /// </summary>
        public ObservableCollection<TorrentInfo>? SeedrTorrentCollection { get; set; }

        /// <summary>
        /// 回收站中的种子任务集合。
        /// </summary>
        public ObservableCollection<TorrentInfo>? RecycleBinCollection { get; set; }

        /// <summary>
        /// Tracker服务器地址集合。
        /// </summary>
        public ObservableCollection<Uri>? Trackers { get; set; }

        /// <summary>
        /// 构造函数，初始化引擎和相关设置模型。
        /// </summary>
        public TorrentModel()
        {
            Engine = new();
            _tempList = new();
            InfoHashHashSet = new();
            DisplayEngineSettingsModel = new();
            DisplayTorrentSettingsModel = new();
        }

        /// <summary>
        /// 初始化模型，加载设置、目录和种子集合。
        /// </summary>
        /// <param name="store">插件服务仓库</param>
        protected override void Init(IPuginServiceStore store)
        {
            EngineSettingsModel = EngineSettingsModel ?? new();
            TorrentSettingsModel = TorrentSettingsModel ?? new();
            RecycleBinCollection = RecycleBinCollection ?? new();
            SeedrTorrentCollection = SeedrTorrentCollection ?? new();
            DowloadTorrentCollection = DowloadTorrentCollection ?? new();
            DowloadCompletedTorrentCollection = DowloadCompletedTorrentCollection ?? new();

            InitCollection(DowloadTorrentCollection);
            InitCollection(DowloadCompletedTorrentCollection);
            InitCollection(RecycleBinCollection);

            SaveDirectory = string.IsNullOrEmpty(SaveDirectory) ? ProgramDirectory.ChekAndCreateFolder("TorrentSave") : SaveDirectory;

            // 初始化 Tracker 列表
            if (Trackers == null || Trackers.Count == 0)
            {
                Trackers = new ObservableCollection<Uri>();

                string path = store.PuginDetails.PluginFolderPath;
                if (File.Exists(path))
                {
                    using (StreamReader reader = new(path))
                    {
                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (string.IsNullOrEmpty(line))
                                break;
                            if (Uri.TryCreate(line, UriKind.Absolute, out var uri))
                            {
                                Trackers.Add(uri);
                            }
                        }
                    }
                }
                else
                {
                    Trackers.Add(new Uri("udp://tracker.openbittorrent.com:80/announce"));
                    Trackers.Add(new Uri("udp://tracker.opentrackr.org:1337/announce"));
                    Trackers.Add(new Uri("udp://tracker.coppersurfer.tk:6969/announce"));
                    Trackers.Add(new Uri("udp://9.rarbg.to:2710/announce"));
                    Trackers.Add(new Uri("udp://tracker.leechers-paradise.org:6969/announce"));
                    Trackers.Add(new Uri("udp://tracker.internetwarriors.net:1337/announce"));
                    Trackers.Add(new Uri("http://tracker.opentrackr.org:1337/announce"));
                    Trackers.Add(new Uri("http://explodie.org:6969/announce"));
                }
            }

            // 初始化种子集合的状态
            void InitCollection(ObservableCollection<TorrentInfo> torrents)
            {
                for (var i = 0; i < torrents.Count; i++)
                {
                    var info = torrents[i];
                    info.SimlpeUpdateInfo();
                    info.UpdateInfo();
                }
            }
        }

        /// <summary>
        /// 启动指定种子任务，自动加载种子或磁力链接并启动下载。
        /// </summary>
        /// <param name="info">种子任务信息</param>
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

            await SetTrackersToManager(manager);

            if (!string.IsNullOrEmpty(manager!.ContainingDirectory) && !Directory.Exists(manager.ContainingDirectory))
            {
                Directory.CreateDirectory(manager.ContainingDirectory);
            }
            info.StartTorrent(manager);
            info.IsDownloading = true;
            InfoHashHashSet.Add(info.V1orV2);
            await manager.StartAsync();
        }

        /// <summary>
        /// 暂停指定种子任务。
        /// </summary>
        /// <param name="info">种子任务信息</param>
        public async Task PauseTorrentAsync(TorrentInfo info)
        {
            var manager = info.Manager;
            if (manager == null)
                return;
            info.IsDownloading = false;
            await manager.PauseAsync();
            info.RemainingTime = TimeSpan.Zero;
            info.UploadSpeed = 0;
            info.DownloadSpeed = 0;
        }

        /// <summary>
        /// 停止指定种子任务。
        /// </summary>
        /// <param name="info">种子任务信息</param>
        public async Task StopTorrentAsync(TorrentInfo info)
        {
            var manager = info.Manager;
            info.IsDownloading = false;
            info.RemainingTime = TimeSpan.Zero;
            info.UploadSpeed = 0;
            info.DownloadSpeed = 0;
            info.Seeds = 0;
            info.Leechs = 0;
            info.PeerCount = 0;
            info.Available = 0;
            if (manager == null)
                return;
            await manager.StopAsync();
        }

        /// <summary>
        /// 移除指定种子任务，并释放相关资源。
        /// </summary>
        /// <param name="info">种子任务信息</param>
        public async Task RemoveTorrentAsync(TorrentInfo info)
        {
            var manager = info.Manager;
            if (manager == null)
            {
                InfoHashHashSet.Remove(info.V1orV2);
                return;
            }
            await StopTorrentAsync(info);
            await Engine.RemoveAsync(manager);
            info.Manager = null;
            InfoHashHashSet.Remove(info.V1orV2);
        }

        /// <summary>
        /// 更新所有种子任务的状态和进度。
        /// </summary>
        public void UpdateTorrentInfo()
        {
            for (var i = 0; i < DowloadTorrentCollection.Count; i++)
            {
                var info = DowloadTorrentCollection[i];
                if (info.IsDownloading)
                {
                    info.UpdateInfo();
                    if (info.Progress == 100)
                    {
                        info.IsDownloading = false;
                        _tempList.Add(info);
                        DowloadCompletedTorrentCollection?.Add(info);
                    }
                }
            }

            for (var i = 0; i < _tempList.Count; i++)
            {
                var info = _tempList[i];
                DowloadTorrentCollection?.Remove(info);
            }
            _tempList.Clear();

            if (SelectedTorrent != null)
            {
                SelectedTorrent.UpdateInfo();
            }
        }

        /// <summary>
        /// 异步加载种子文件并创建种子任务信息。
        /// </summary>
        /// <param name="torrentPath">种子文件路径</param>
        /// <param name="service">调度服务实例</param>
        /// <returns>种子任务信息</returns>
        public async Task<TorrentInfo> LoadTorrentAsync(string torrentPath, IDispatcherService service)
        {
            var torrent = await Torrent.LoadAsync(torrentPath);

            string savePath = string.Empty;
            int fileCount = torrent.Files.Count;
            if (fileCount == 0)
            {
                throw new InvalidDataException("种子文件中不包含任何文件，无法下载");
            }
            else if (fileCount == 1)
            {
                savePath = Path.Combine(SaveDirectory, Path.GetFileNameWithoutExtension(torrent.Name));
            }
            else
            {
                savePath = Path.Combine(SaveDirectory, torrent.Name);
            }

            var info = new TorrentInfo(torrent, torrentPath, savePath, service);
            return info;
        }

        /// <summary>
        /// 判断指定种子任务是否已存在于哈希集合中。
        /// </summary>
        /// <param name="info">种子任务信息</param>
        /// <returns>是否已存在</returns>
        public bool ContainsHash(TorrentInfo info)
        {
            return InfoHashHashSet.Contains(info.V1orV2);
        }

        /// <summary>
        /// 更新引擎和种子设置，并应用到所有相关任务。
        /// </summary>
        public void UpdateSettings()
        {
            bool engineNeedUpdate = !EngineSettingsModel!.IsSettingsEqual(currentEngineSettings);
            if (engineNeedUpdate)
            {
                currentEngineSettings = EngineSettingsModel.ToSettings();
            }

            bool torrentNeedUpdate = !TorrentSettingsModel!.IsSettingsEqual(currentTorrentSettngs);
            if (torrentNeedUpdate)
            {
                currentTorrentSettngs = TorrentSettingsModel.ToSettings();
            }

            Task.Run(async () =>
            {
                if (engineNeedUpdate)
                {
                    await Engine.UpdateSettingsAsync(currentEngineSettings);
                }
                if (torrentNeedUpdate && DowloadTorrentCollection != null)
                {
                    foreach (var torrent in DowloadTorrentCollection)
                    {
                        if (torrent.Manager != null)
                        {
                            await torrent.Manager.UpdateSettingsAsync(currentTorrentSettngs);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 将 Tracker 列表中的所有 Tracker 添加到指定的 TorrentManager 中。
        /// </summary>
        /// <param name="manager">目标 TorrentManager</param>
        /// <returns>异步任务</returns>
        private async Task SetTrackersToManager(TorrentManager manager)
        {
            if (Trackers == null || Trackers.Count == 0)
                return;

            for (int i = 0; i < Trackers.Count; i++)
            {
                var uri = Trackers[i];
                await manager.TrackerManager.AddTrackerAsync(uri);
            }
        }
    }
}
