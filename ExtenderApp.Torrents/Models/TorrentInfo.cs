using System.ComponentModel;
using ExtenderApp.Data;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// 表示一个Torrent的信息
    /// </summary>
    public class TorrentInfo : INotifyPropertyChanged
    {
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

        public int Seeds { get; set; }

        public int Leechs { get; set; }

        public int Available { get; set; }

        #region Files

        public BitFieldData? BitData { get; set; }

        public ValueOrList<TorrentFileInfoNode> Files { get; set; }

        public int FileCount { get; set; }

        public bool SelecrAll { get; set; }

        #endregion

        #region MonoTorrent

        public MagnetLink? MagnetLink { get; set; }

        public Torrent? Torrent { get; set; }

        public TorrentManager? Manager { get; private set; }

        public TorrentState? State
        {
            get
            {
                if (Manager == null)
                    return TorrentState.Stopped;

                return Manager.State;
            }
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        public TorrentInfo(Torrent torrent)
        {
            Torrent = torrent;
            Name = torrent.Name;
            Size = torrent.Size;
            PieceLength = torrent.PieceLength;
            PieceCount = torrent.PieceCount;
            BitData = new BitFieldData(PieceCount);
            IsDownloading = false;
            SelecrAll = false;

            Files = new();
            var list = torrent.Files;
            FileCount = list.Count;
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

            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                file.UpdateLengthForFolder();
            }
        }

        public void Set(TorrentManager manager)
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
            Task.Run(async () =>
            {
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

        #region Update

        public void UpdateInfo()
        {
            if (Manager == null)
                return;

            Progress = Manager.Progress;
            DownloadSpeed = Manager.Monitor.DownloadRate;
            UploadSpeed = Manager.Monitor.UploadRate;

            Seeds = Manager.Peers.Seeds;
            Leechs = Manager.Peers.Leechs;
            Available = Manager.Peers.Available;

            foreach (var info in Files)
            {
                info.UpdetaProgress();
            }
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

            for (int i = 0; i < Files.Count; i++)
            {
                var node = Files[i];
                allCount += node.GetSelectedFileCount();
                allLength += node.GetSelectedFileLength();
                completeCount += node.GetSelectedFileCompleteCount();
                completeLength += node.GetSelectedFileCompleteLength();
            }
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

        #endregion
    }
}
