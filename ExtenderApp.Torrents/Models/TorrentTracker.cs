using MonoTorrent.Trackers;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// 表示一个种子跟踪器，用于管理和更新种子跟踪器的状态和信息。
    /// </summary>
    public class TorrentTracker
    {
        /// <summary>
        /// 跟踪器实例
        /// </summary>
        public ITracker Tracker { get; }

        /// <summary>
        /// 获取或设置跟踪器的URI地址。
        /// </summary>
        public Uri TrackerUri { get; set; }

        /// <summary>
        /// 获取一个值，指示是否支持抓取跟踪器。
        /// </summary>
        public bool CanScrape { get; private set; }

        /// <summary>
        /// 获取最小更新间隔时间。
        /// </summary>
        public TimeSpan MinUpdateInterval { get; private set; }

        /// <summary>
        /// 获取更新间隔时间。
        /// </summary>
        public TimeSpan UpdateInterval { get; private set; }

        /// <summary>
        /// 获取自上次公告以来的时间。
        /// </summary>
        public TimeSpan TimeSinceLastAnnounce { get; private set; }

        /// <summary>
        /// 获取跟踪器的当前状态。
        /// </summary>
        public TrackerState Status { get; private set; }

        /// <summary>
        /// 获取跟踪器的警告消息（如果有）。
        /// </summary>
        public string? WarningMessage { get; private set; }

        /// <summary>
        /// 获取跟踪器的失败消息（如果有）。
        /// </summary>
        public string? FailureMessage { get; private set; }

        /// <summary>
        /// 初始化 <see cref="TorrentTracker"/> 类的新实例。
        /// </summary>
        /// <param name="tracker">用于初始化的跟踪器实例。</param>
        public TorrentTracker(ITracker tracker)
        {
            Tracker = tracker;
            TrackerUri = tracker.Uri;
            CanScrape = tracker.CanScrape;
        }

        /// <summary>
        /// 更新跟踪器的状态和信息。
        /// </summary>
        public void Update()
        {
            CanScrape = Tracker.CanScrape;
            MinUpdateInterval = Tracker.MinUpdateInterval;
            UpdateInterval = Tracker.UpdateInterval;
            TimeSinceLastAnnounce = Tracker.TimeSinceLastAnnounce;
            Status = Tracker.Status;
            WarningMessage = Tracker.WarningMessage;
            FailureMessage = Tracker.FailureMessage;
        }
    }
}
