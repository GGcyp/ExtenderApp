using System.Collections.ObjectModel;
using System.IO;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Models;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// 表示一个Torrent的信息
    /// </summary>
    public class TorrentInfo : DataModel
    {
        private readonly IDispatcherService _dispatcherService;

        #region Properties

        private bool isVerifyFileIntegrity;

        /// <summary>
        /// Torrent的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Torrent的大小
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Torrent是否正在下载
        /// </summary>
        public bool IsDownloading { get; set; }

        /// <summary>
        /// 每个数据块的大小
        /// </summary>
        public int PieceLength { get; set; }

        /// <summary>
        /// 数据块的数量
        /// </summary>
        public int PieceCount { get; set; }

        /// <summary>
        /// 下载进度
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// 是否完成下载
        /// </summary>
        public bool IsComplete => Progress >= 100;

        /// <summary>
        /// 下载速度
        /// </summary>
        public long DownloadSpeed { get; set; }

        /// <summary>
        /// 上传速度
        /// </summary>
        public long UploadSpeed { get; set; }

        /// <summary>
        /// 已完成的选中文件数量
        /// </summary>
        public int SelectedFileCompleteCount { get; set; }

        /// <summary>
        /// 已完成的选中文件总大小
        /// </summary>
        public long SelectedFileCompleteLength { get; set; }

        /// <summary>
        /// 选中的文件数量
        /// </summary>
        public int SelectedFileCount { get; set; }

        /// <summary>
        /// 选中的文件总大小
        /// </summary>
        public long SelectedFileLength { get; set; }

        /// <summary>
        /// 种子文件的保存路径
        /// </summary>
        public string TorrentPath { get; set; }

        /// <summary>
        /// 下载保存路径
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 磁力链接
        /// </summary>
        public string TorrentMagnetLink { get; set; }

        /// <summary>
        /// 下载任务创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 种子创建时间
        /// </summary>
        public DateTime TorrentCreateTime { get; set; }

        /// <summary>
        /// 获取或设置创建该种子的客户端名称/版本
        /// 示例值: "uTorrent/3.5.5"
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 获取或设置种子附加的评论信息
        /// 可能是制作者注释、文件说明等内容
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 获取或设置种子文件的编码格式
        /// 示例值: "UTF-8"
        /// </summary>
        public string Encoding { get; set; }

        /// <summary>
        /// 剩余下载时间
        /// </summary>
        public TimeSpan RemainingTime { get; set; }

        /// <summary>
        /// 获取或设置V1哈希值，用于标识 torrent 的版本1信息。
        /// </summary>
        public HashValue V1 { get; set; }

        /// <summary>
        /// 获取或设置V2哈希值，用于标识 torrent 的版本2信息。
        /// </summary>
        public HashValue V2 { get; set; }

        /// <summary>
        /// 获取当前 Torrent 的有效哈希值，优先返回 V1（如果存在），否则返回 V2。
        /// </summary>
        /// <remarks>
        /// 用于兼容 Hybrid Torrent（同时支持 V1 和 V2 哈希）的场景。
        /// </remarks>
        public HashValue V1orV2 => V1.IsEmpty ? V2 : V1;

        #endregion Properties

        #region TorrentFiles

        public ValueOrList<TorrentFileInfoNode> Files { get; set; }

        public int FileCount { get; set; }

        public bool SelecrAll { get; set; }

        #endregion TorrentFiles

        #region MonoTorrent

        public MagnetLink? MagnetLink { get; set; }

        public Torrent? Torrent { get; set; }

        public TorrentManager? Manager { get; set; }

        public InfoHashes? InfoHashes => Torrent == null ? MagnetLink == null ? null : MagnetLink.InfoHashes : Torrent.InfoHashes;

        public TorrentState? State
        {
            get
            {
                if (Manager == null)
                    return TorrentState.Stopped;

                return Manager.State;
            }
        }

        #endregion MonoTorrent

        #region Peers

        /// <summary>
        /// 做种者数量
        /// </summary>
        public int Seeds { get; set; }

        /// <summary>
        /// 下载者数量
        /// </summary>
        public int Leechs { get; set; }

        /// <summary>
        /// 可用连接数（做种者+下载者）
        /// </summary>
        public int Available { get; set; }

        /// <summary>
        /// 已连接的对等体数量
        /// </summary>
        public int PeerCount { get; set; }

        public ObservableCollection<TorrentPeer> ConnectPeers { get; set; }

        #endregion Peers

        #region Tracker

        public ObservableCollection<TorrentTracker> Trackers { get; set; }

        #endregion Tracker

        #region Piece

        /// <summary>
        /// 获取或设置表示种子文件位域（Bitfield）的可观察集合。
        /// </summary>
        public List<TorrentPiece> Pieces { get; set; }

        public int TrueCount { get; set; }

        public int SelectedBitfieldCount { get; set; }

        #endregion Piece

        public TorrentInfo(Torrent torrent, string torrentPath, string savePath, IDispatcherService service) : this(service)
        {
            Torrent = torrent;
            Name = torrent.Name;
            Size = torrent.Size;
            PieceLength = torrent.PieceLength;
            PieceCount = torrent.PieceCount;
            IsDownloading = false;
            SelecrAll = false;
            TorrentCreateTime = torrent.CreationDate;
            CreateTime = DateTime.Now;
            CreatedBy = !string.IsNullOrEmpty(torrent.CreatedBy) ? torrent.CreatedBy : "未找到";
            Comment = torrent.Comment;
            Encoding = !string.IsNullOrEmpty(torrent.Encoding) ? torrent.Encoding : "未找到";
            V1 = torrent.InfoHashes.V1 == null ? HashValue.SHA1Empty : new(torrent.InfoHashes.V1.Span);
            V2 = torrent.InfoHashes.V2 == null ? HashValue.SHA256Empty : new(torrent.InfoHashes.V2.Span);
            TorrentMagnetLink = torrent.GetMagnetLink();
            TorrentPath = torrentPath;
            SavePath = savePath;

            Files = new();
            Pieces = new(PieceCount);

            var list = Torrent.Files;
            FileCount = list.Count;
            WriteFileInfo(list);
            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                file.UpdateLengthForFolder();
            }
        }

        public TorrentInfo(IDispatcherService service)
        {
            _dispatcherService = service;
            ConnectPeers = new();
            Trackers = new();
        }

        public void StartTorrent(TorrentManager manager)
        {
            Manager = manager;
            var list = Manager.Files;
            for (int i = 0; i < list.Count; i++)
            {
                var file = list[i];
                var node = FindNodeForTorrentFilePath(file.Path);
                if (node == null)
                    continue;

                node.TorrentManagerFile = file;
                node.TorrentFileInfoChanged();
            }

            InitPeerEvent(manager);
            InitTrackerEvent(manager);

            Task.Run(async () =>
            {
                if (!Directory.Exists(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                }

                for (int i = 0; i < Files.Count; i++)
                {
                    var info = Files[i];
                    foreach (var node in info)
                    {
                        if (node.TorrentManagerFile == null)
                            continue;
                        await Manager.SetFilePriorityAsync(node.TorrentManagerFile, node.Priority);
                    }
                }
            });
        }

        private void InitPeerEvent(TorrentManager manager)
        {
            manager.PeerConnected += (o, e) =>
            {
                TorrentPeer? peer = new TorrentPeer(e.Peer);

                _dispatcherService.Invoke(() =>
                {
                    ConnectPeers.Add(new TorrentPeer(e.Peer));
                });
            };

            manager.PeerDisconnected += (o, e) =>
            {
                int index = -1;
                TorrentPeer? peer = null;
                for (int i = 0; i < ConnectPeers.Count; i++)
                {
                    if (ConnectPeers[i].Id == e.Peer)
                    {
                        index = i;
                        peer = ConnectPeers[i];
                        break;
                    }
                }
                if (index != -1)
                {
                    _dispatcherService.Invoke(() =>
                    {
                        ConnectPeers.RemoveAt(index);
                    });
                }
            };
        }

        private void InitTrackerEvent(TorrentManager manager)
        {
            manager.TrackerManager.AnnounceComplete += (o, e) =>
            {
                int index = -1;
                TorrentTracker? tracker = null;
                for (int i = 0; i < Trackers.Count; i++)
                {
                    tracker = Trackers[i];
                    if (e.Tracker.Uri == tracker.TrackerUri)
                    {
                        tracker.Update();
                        index = i;
                        break;
                    }
                }

                if (index != -1 && !e.Successful)
                {
                    _dispatcherService.Invoke(() =>
                    {
                        Trackers.RemoveAt(index);
                    });
                    return;
                }

                //if (tracker != null && tracker.Tracker == e.Tracker &&
                //tracker.TrackerUri.Host == e.Tracker.Uri.Host &&
                //tracker.TrackerUri.Port == e.Tracker.Uri.Port)
                //{
                //    manager.TrackerManager.RemoveTrackerAsync(e.Tracker);
                //    return;
                //}

                _dispatcherService.Invoke(() =>
                {
                    Trackers.Add(new TorrentTracker(e.Tracker));
                });
            };
            manager.TrackerManager.ScrapeComplete += (o, e) =>
            {
            };
        }

        private TorrentFileInfoNode? FindNodeForTorrentFilePath(string path)
        {
            var span = path.AsSpan();
            var index = span.IndexOf(System.IO.Path.DirectorySeparatorChar);
            TorrentFileInfoNode? node = null;
            string nodeName = string.Empty;

            if (index != -1)
            {
                nodeName = new(span.Slice(0, index));
                node = FindNodeForFiles(nodeName);
                if (node == null) return null;
            }
            else
            {
                //直接是文件，没有父文件夹
                return FindNodeForFiles(path);
            }

            while (index != -1)
            {
                span = span.Slice(index + 1);
                index = span.IndexOf(System.IO.Path.DirectorySeparatorChar);
                if (index == -1)
                    break;

                nodeName = new(span.Slice(0, index));
                node = node?.Find(n => n.Name == nodeName);

                if (node == null || node.Name == nodeName)
                    break;
            }
            return node;
        }

        /// <summary>
        /// 根据文件名查找对应的 TorrentFileInfoNode 节点
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>找到对应的 TorrentFileInfoNode 节点，如果未找到则返回 null</returns>
        private TorrentFileInfoNode? FindNodeForFiles(string name)
        {
            if (Files == null) return null;

            foreach (var fileNode in Files)
            {
                if (fileNode.Name == name)
                    return fileNode;
            }
            return null;
        }

        private void WriteFileInfo(IList<ITorrentFile> list)
        {
            foreach (var file in list)
            {
                var span = file.Path.AsSpan();
                var index = span.IndexOf(System.IO.Path.DirectorySeparatorChar);
                TorrentFileInfoNode? node = null;
                TorrentFileInfoNode? parentNode = null;
                string parentNodeName = string.Empty;

                if (index != -1)
                {
                    parentNodeName = new(span.Slice(0, index));
                    parentNode = FindNodeForFiles(parentNodeName);
                }
                while (index != -1)
                {
                    var lastParentNode = parentNode;
                    if (parentNode != null)
                    {
                        span = span.Slice(index + 1);
                        index = span.IndexOf(System.IO.Path.DirectorySeparatorChar);
                        if (index == -1)
                            break;

                        parentNodeName = new(span.Slice(0, index));
                        parentNode = parentNode?.Find(n => n.Name == parentNodeName);
                    }

                    if (parentNode == null)
                    {
                        node = new();
                        node.Name = parentNodeName;
                        node.IsFile = false;
                        node.TorrentInfo = this;
                        parentNode = lastParentNode;
                    }
                    else
                    {
                        continue;
                    }

                    if (parentNode == null)
                    {
                        Files.Add(node);
                        node.Depth = 0;
                    }
                    else
                    {
                        parentNode.Add(node);
                        node.Depth = parentNode.Depth + 1;
                    }

                    parentNode = node;

                    span = span.Slice(index + 1);
                    index = span.IndexOf(System.IO.Path.DirectorySeparatorChar);
                }

                node = new();
                node.Length = file.Length;
                node.Name = new string(span);
                node.IsFile = true;
                node.TorrentInfo = this;
                if (parentNode == null)
                {
                    Files.Add(node);
                    node.Depth = 0;
                }
                else
                {
                    parentNode.Add(node);
                    node.Depth = parentNode.Depth + 1;
                }
            }
        }

        /// <summary>
        /// 选择或取消选择所有文件
        /// </summary>
        public void SelecrAllFiles()
        {
            var list = Files;
            bool selecrAll = !SelecrAll;
            for (int i = 0; i < list.Count; i++)
            {
                var node = list[i];
                node.DisplayNeedDownload = selecrAll;
            }
            SelecrAll = selecrAll;
        }

        /// <summary>
        /// 验证文件的完整性
        /// </summary>
        /// <remarks>
        /// 该方法首先检查Manager是否存在以及是否包含元数据，
        /// 如果不满足条件则直接返回。否则，先停止Manager，
        /// 然后执行哈希校验以验证文件完整性。
        /// </remarks>
        public async Task VerifyFileIntegrity()
        {
            if (Manager == null || !Manager.HasMetadata)
                return;
            isVerifyFileIntegrity = true;

            await Manager.StopAsync();
            await Manager.HashCheckAsync(false);
            isVerifyFileIntegrity = false;
        }

        /// <summary>
        /// 异步添加一个 Peer 节点到当前 Torrent 任务。
        /// </summary>
        /// <param name="peerUri">Peer 节点的 URI 地址（例如：tcp://192.168.1.100:54321）</param>
        /// <remarks>
        /// 如果 Manager 未初始化（null），则直接返回不执行任何操作。<br/>
        /// 实际添加 Peer 的操作通过 <see cref="Task.Run"/> 异步执行，避免阻塞主线程。
        /// </remarks>
        public void AddPeer(Uri peerUri)
        {
            if (Manager == null) return;

            Task.Run(async () =>
            {
                await Manager.AddPeerAsync(new MonoTorrent.PeerInfo(peerUri));
            });
        }

        #region Update

        public void SimlpeUpdateInfo()
        {
            if (Manager == null || isVerifyFileIntegrity)
                return;

            Progress = Manager.Progress;
            DownloadSpeed = Manager.Monitor.DownloadRate;
            UploadSpeed = Manager.Monitor.UploadRate;

            long completeLength = 0;
            for (int i = 0; i < Files.Count; i++)
            {
                var node = Files[i];
                completeLength += node.GetSelectedFileCompleteLength();
            }
            SelectedFileCompleteLength = completeLength;

            long RemainingLength = SelectedFileLength - SelectedFileCompleteLength;

            double remainingTime = DownloadSpeed == 0 ? 0 : RemainingLength / DownloadSpeed;
            RemainingTime = TimeSpan.FromSeconds(remainingTime);
        }

        public void UpdateInfo()
        {
            if (Manager == null || isVerifyFileIntegrity)
                return;

            Seeds = Manager.Peers.Seeds;
            Leechs = Manager.Peers.Leechs;
            Available = Manager.Peers.Available;
            PeerCount = ConnectPeers.Count;
            Progress = Manager.Progress;

            int selectedBitfieldCount = 0;
            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                file.UpdateProgress();
                selectedBitfieldCount += file.UpdatePieceCount();
            }
            SelectedBitfieldCount = selectedBitfieldCount;
            for (int i = 0; i < ConnectPeers.Count; i++)
            {
                ConnectPeers[i].Update();
            }
            var bitfield = Manager.Bitfield;
            TrueCount = bitfield.TrueCount;
            _dispatcherService.Invoke(() =>
            {
                for (int i = 0; i < PieceCount; i++)
                {
                    var piece = Pieces[i];
                    bool bitBool = bitfield[i];
                    piece.State = bitBool ? TorrentPieceStateType.Complete : piece.State;
                    piece.UpdateMessageType();
                }
            });
        }

        /// <summary>
        /// 更新当前种子中所有文件的下载状态
        /// </summary>
        /// <param name="isUpdate">可选参数，默认为true，表示是否更新下载状态</param>
        public void UpdateDownloadState(bool isUpdate = true)
        {
            Task.Run(() =>
            {
                // 遍历当前种子中的所有文件
                for (int i = 0; i < Files.Count; i++)
                {
                    // 获取当前文件节点
                    var file = Files[i];

                    // 调用文件节点的更新下载状态方法
                    file.UpdateDownloadState(isUpdate);
                    file.UpdatePieces(Pieces);
                }
            });
        }

        /// <summary>
        /// 更新信息
        /// </summary>
        /// <remarks>
        /// 该方法用于更新文件信息，包括所有选中文件的数量、总大小、已完成的数量以及已完成的总大小。
        /// </remarks>
        public void UpdateSelectedFileInfo()
        {
            int allCount = 0;
            long allLength = 0;
            int completeCount = 0;
            long completeLength = 0;

            bool isAllSelected = true;
            for (int i = 0; i < Files.Count; i++)
            {
                var node = Files[i];
                allCount += node.GetSelectedFileCount();
                allLength += node.GetSelectedFileLength();
                completeCount += node.GetSelectedFileCompleteCount();
                completeLength += node.GetSelectedFileCompleteLength();
                if (!node.DisplayNeedDownload) isAllSelected = false;
            }
            SelecrAll = isAllSelected;
            SelectedFileCount = allCount;
            SelectedFileLength = allLength;
            SelectedFileCompleteCount = completeCount;
            SelectedFileCompleteLength = completeLength;
        }

        /// <summary>
        /// 更新选中文件的总长度。
        /// </summary>
        /// <returns>返回选中文件的总长度。</returns>
        public long UpdateSelectedFileLength()
        {
            long result = 0;
            for (int i = 0; i < Files.Count; i++)
            {
                result += Files[i].GetSelectedFileLength();
            }
            SelectedFileLength = result;
            return result;
        }

        /// <summary>
        /// 更新选中文件的总数。
        /// </summary>
        /// <returns>返回选中文件的总数。</returns>
        public int UpdateSelectedFileCount()
        {
            int result = 0;
            for (int i = 0; i < Files.Count; i++)
            {
                result += Files[i].GetSelectedFileCount();
            }
            SelectedFileCount = result;
            return result;
        }

        /// <summary>
        /// 更新所有文件中已选择文件的总长度，并返回该长度。
        /// </summary>
        /// <returns>返回所有文件中已选择文件的总长度。</returns>
        public long UpdateSelectedFileCompleteLength()
        {
            long result = 0;
            for (int i = 0; i < Files.Count; i++)
            {
                result += Files[i].GetSelectedFileCompleteLength();
            }
            SelectedFileCompleteLength = result;
            return result;
        }

        /// <summary>
        /// 更新所有文件中已选择文件的总数，并返回该数量。
        /// </summary>
        /// <returns>返回所有文件中已选择文件的总数。</returns>
        public int UpdateSelectedFileCompleteCount()
        {
            int result = 0;
            for (int i = 0; i < Files.Count; i++)
            {
                result += Files[i].GetSelectedFileCompleteCount();
            }
            SelectedFileCompleteCount = result;
            return result;
        }

        #endregion Update
    }
}