
namespace ExtenderApp.Torrent
{
    /// <summary>
    /// Tracker响应类
    /// </summary>
    public class TrackerResponse
    {
        /// <summary>
        /// 下次请求间隔(秒)
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 种子数
        /// </summary>
        public int Complete { get; set; }

        /// <summary>
        /// 下载者数
        /// </summary>
        public int Incomplete { get; set; }
    }
}
