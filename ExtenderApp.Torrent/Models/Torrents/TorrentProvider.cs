

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
        private readonly TorrentFileForamtter _torrentFileForamtter;

        /// <summary>
        /// Tracker提供者。
        /// </summary>
        private readonly TrackerProvider _trackerProvider;

        /// <summary>
        /// 本地Torrent信息。
        /// </summary>
        private readonly LocalTorrentInfo _localTorrentInfo;

        /// <summary>
        /// 随机数生成器。
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// 初始化TorrentProvider类的新实例。
        /// </summary>
        /// <param name="torrentFileForamtter">Torrent文件格式化器。</param>
        /// <param name="trackerProvider">Tracker提供者。</param>
        /// <param name="info">本地Torrent信息。</param>
        public TorrentProvider(TorrentFileForamtter torrentFileForamtter, TrackerProvider trackerProvider, LocalTorrentInfo info)
        {
            _torrentFileForamtter = torrentFileForamtter;
            _trackerProvider = trackerProvider;
            _localTorrentInfo = info;
            _random = new Random();
        }

        /// <summary>
        /// 从指定的内存流中获取Torrent对象。
        /// </summary>
        /// <param name="memory">包含Torrent文件内容的内存流。</param>
        /// <returns>返回一个Torrent对象。</returns>
        public Torrent GetTorrent(Memory<byte> memory)
        {
            var torrentFile = _torrentFileForamtter.Decode(memory);

            Torrent result = new(torrentFile, _localTorrentInfo, _random, _trackerProvider);

            return result;
        }
    }
}
