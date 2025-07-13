

namespace ExtenderApp.Torrent
{
    /// <summary>
    /// 表示一个种子文件的片段
    /// </summary>
    public struct TorrentFileSegment
    {
        /// <summary>
        /// 获取或设置种子文件片段的起始信息
        /// </summary>
        public TorrentFileSegmentInfo Fist { get; set; }

        /// <summary>
        /// 获取或设置种子文件片段的结束信息
        /// </summary>
        public TorrentFileSegmentInfo? End { get; set; }
    }
}
