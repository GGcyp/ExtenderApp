using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.Torrent
{
    /// <summary>
    /// Tracker响应类
    /// </summary>
    public class TrackerResponse
    {
        private static ObjectPool<TrackerResponse> pool = ObjectPool.CreateDefaultPool<TrackerResponse>();
        public TrackerResponse Get() => pool.Get();
        public void Release(TrackerResponse response) => pool.Release(response);

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

        /// <summary>
        /// 上次公告时间
        /// </summary>
        public DateTime LastAnnounceTime { get; set; }

        public void Release()
        {
            Release(this);
        }
    }
}
