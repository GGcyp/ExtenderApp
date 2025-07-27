using System.ComponentModel;
using ExtenderApp.Data;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents.Models
{
    public class TorrentInfo : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public bool IsDownload { get; set; }

        public int PieceLength { get; set; }

        public int PieceCount { get; set; }

        public int Progress { get; set; }

        #region Files

        public BitFieldData? BitData { get; set; }

        public ValueOrList<TorrentFileInfoNode> Files { get; set; }

        public int FileCount { get; set; }

        public bool SelecrAll { get; set; } = false;

        #endregion

        #region MonoTorrent

        public MagnetLink? MagnetLink { get; set; }

        public Torrent? Torrent { get; set; }

        public TorrentManager? Manager { get; private set; }

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
            IsDownload = false;

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
                node.TorrentManager = manager;
                node.TorrentFileInfoChanged();
            }
            IsDownload = true;
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
                if (fileNode.IsFile)
                    continue;
                if (fileNode.Name == name)
                    return fileNode;
            }
            return null;
        }
    }
}
