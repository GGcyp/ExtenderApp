using ExtenderApp.Abstract;

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// TorrentProvider 类用于处理Torrent文件的提供。
    /// </summary>
    public class TorrentProvider
    {
        /// <summary>
        /// Torrent文件格式化器。
        /// </summary>
        private readonly TorrentFileFormatter _torrentFileForamtter;

        /// <summary>
        /// Tracker提供者。
        /// </summary>
        private readonly TrackerProvider _trackerProvider;

        /// <summary>
        /// 本地Torrent信息。
        /// </summary>
        private readonly LocalTorrentInfo _localTorrentInfo;

        private readonly TorrentPeerProvider _torrentPeerProvider;

        /// <summary>
        /// TorrentSender的只读引用
        /// </summary>
        private readonly TorrentSender _sender;

        private readonly IFileOperateProvider _fileOperateProvider;

        /// <summary>
        /// 初始化TorrentProvider类的新实例。
        /// </summary>
        /// <param name="torrentFileForamtter">Torrent文件格式化器。</param>
        /// <param name="trackerProvider">Tracker提供者。</param>
        /// <param name="info">本地Torrent信息。</param>
        public TorrentProvider(TorrentFileFormatter torrentFileForamtter,
            TrackerProvider trackerProvider,
            LocalTorrentInfo info,
            TorrentSender sender,
            IFileOperateProvider fileOperateProvider,
            TorrentPeerProvider torrentPeerProvider)
        {
            _torrentFileForamtter = torrentFileForamtter;
            _trackerProvider = trackerProvider;
            _localTorrentInfo = info;
            _sender = sender;
            _fileOperateProvider = fileOperateProvider;
            _torrentPeerProvider = torrentPeerProvider;
        }

        /// <summary>
        /// 获取种子对象
        /// </summary>
        /// <param name="parent">管理种子文件下载信息的节点</param>
        /// <returns>种子对象</returns>
        /// <exception cref="ArgumentNullException">如果parent为null，抛出此异常</exception>
        /// <exception cref="ArgumentException">如果parent的TorrentFileInfo为空或无法解析Torrent文件内容，抛出此异常</exception>
        public Torrent GetTorrent(TorrentFileInfoNodeParent parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent), "管理种子文件下载信息节点不能为空");

            Torrent torrent;
            var store = _torrentPeerProvider.CreateInfoHashPeerStore(parent.Hash, parent);
            if (parent.TorrentFileInfo.IsEmpty)
            {
                if (string.IsNullOrEmpty(parent.TorrentFileInfo))
                    throw new ArgumentException("种子文件地址不能为空", nameof(parent.TorrentFileInfo));

                var fileOperate = _fileOperateProvider.GetOperate(parent.TorrentFileInfo);
                var torrentFile = _torrentFileForamtter.Decode(fileOperate);
                parent.Set(torrentFile);
                torrent = new Torrent(torrentFile.Hash, parent, torrentFile, _localTorrentInfo, _sender, store);
            }
            else
            {
                torrent = new(parent.Hash, parent, null, _localTorrentInfo, _sender, store);
            }

            if (parent.AnnounceList != null)
                _trackerProvider.RegisterTracker(parent.AnnounceList);

            return torrent;
        }

        /// <summary>
        /// 根据提供的路径获取Torrent对象。
        /// </summary>
        /// <param name="path">Torrent文件路径。</param>
        /// <returns>返回一个Torrent对象。</returns>
        public Torrent GetTorrent(string path)
        {
            var fileOperate = _fileOperateProvider.GetOperate(path);
            return GetTorrent(fileOperate);
        }

        /// <summary>
        /// 根据提供的文件操作对象获取Torrent对象。
        /// </summary>
        /// <param name="fileOperate">文件操作对象。</param>
        /// <returns>返回一个Torrent对象。</returns>
        public Torrent GetTorrent(IFileOperate fileOperate)
        {
            var torrentFile = _torrentFileForamtter.Decode(fileOperate);
            if (torrentFile == null)
                throw new ArgumentException("无法解析Torrent文件内容。");

            Torrent torrent = GetTorrent(torrentFile);
            torrent.InfoNodeParent.TorrentFileInfo = fileOperate.Info.FilePath;
            return torrent;
        }

        /// <summary>
        /// 从指定的内存流中获取Torrent对象。
        /// </summary>
        /// <param name="memory">包含Torrent文件内容的内存流。</param>
        /// <returns>返回一个Torrent对象。</returns>
        public Torrent GetTorrent(Memory<byte> memory)
        {
            var torrentFile = _torrentFileForamtter.Decode(memory);
            if (torrentFile == null)
                throw new ArgumentException("无法解析Torrent内容。");

            return GetTorrent(torrentFile);
        }

        /// <summary>
        /// 初始化Torrent对象。
        /// </summary>
        /// <param name="torrentFile">Torrent文件对象。</param>
        /// <returns>返回一个Torrent对象。</returns>
        private Torrent GetTorrent(TorrentFile torrentFile)
        {
            if (torrentFile.AnnounceList != null)
                _trackerProvider.RegisterTracker(torrentFile.AnnounceList);

            TorrentFileInfoNodeParent parent = new();
            parent.Set(torrentFile);

            var store = _torrentPeerProvider.CreateInfoHashPeerStore(torrentFile.Hash, parent);
            Torrent torrent = new(torrentFile.Hash, parent, torrentFile, _localTorrentInfo, _sender, store);

            return torrent;
        }
    }
}
