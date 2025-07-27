

namespace ExtenderApp.Torrents
{
    /// <summary>
    /// 表示种子下载状态的枚举
    /// </summary>
    public enum TorrentStatus : byte
    {
        /// <summary>
        /// 下载中
        /// </summary>
        Downloading,
        /// <summary>
        /// 做种中
        /// </summary>
        Seeding,
        /// <summary>
        /// 已暂停
        /// </summary>
        Paused,
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        /// <summary>
        /// 发生错误
        /// </summary>
        Error
    }
}
