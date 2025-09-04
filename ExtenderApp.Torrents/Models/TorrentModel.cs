using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Models;
using ExtenderApp.Torrents.Models;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents
{
    public class TorrentModel : ExtenderAppModel
    {
        private readonly List<TorrentInfo> _tempList;
        internal HashSet<HashValue> InfoHashHashSet { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; } = new();

        public ClientEngine Engine { get; private set; }

        public TorrentSettingsBuilderModel TorrentSettingsModel { get; set; }

        public TorrentSettingsBuilderModel DisplayTorrentSettingsModel { get; set; }

        public EngineSettingsBuilderModel EngineSettingsModel { get; set; }
        public EngineSettingsBuilderModel DisplayEngineSettingsModel { get; set; }

        public string? SaveDirectory { get; set; }

        private TorrentSettings currentTorrentSettngs;

        private EngineSettings currentEngineSettings;

        public TorrentInfo? SelectedTorrent { get; set; }

        public IView? TorrentListView { get; set; }

        public IView? TorrentDetailsView { get; set; }

        public ObservableCollection<TorrentInfo>? DowloadTorrentCollection { get; set; }

        public ObservableCollection<TorrentInfo>? DowloadCompletedTorrentCollection { get; set; }

        public ObservableCollection<TorrentInfo>? SeedrTorrentCollection { get; set; }

        public ObservableCollection<TorrentInfo>? RecycleBinCollection { get; set; }

        public ObservableCollection<Uri>? Trackers { get; set; }

        public TorrentModel()
        {
            Engine = new();
            _tempList = new();
            InfoHashHashSet = new();
            DisplayEngineSettingsModel = new();
            DisplayTorrentSettingsModel = new();
        }

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

            SaveDirectory = string.IsNullOrEmpty(SaveDirectory) ? store.PathService.CreateFolderPathForAppRootFolder("TorrentSave") : SaveDirectory;

            if (Trackers == null || Trackers.Count == 0)
            {
                Trackers = new ObservableCollection<Uri>();

                string path = store.PuginDetails.PluginDirectoryPath;
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

        public bool ContainsHash(TorrentInfo info)
        {
            return InfoHashHashSet.Contains(info.V1orV2);
        }

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
        /// 将Trackers列表中的所有Tracker添加到指定的TorrentManager中。
        /// </summary>
        /// <param name="manager">被添加的TorrentManager</param>
        /// <returns>返回Task</returns>
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
