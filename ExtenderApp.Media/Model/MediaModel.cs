using System.Collections.ObjectModel;

namespace ExtenderApp.Media
{
    /// <summary>
    /// 媒体数据类
    /// </summary>
    public class MediaModel
    {
        /// <summary>
        /// 视频信息集合
        /// </summary>
        public ObservableCollection<VideoInfo> VideoInfos { get; set; }

        /// <summary>
        /// 音量
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// 是否记录观看时间
        /// </summary>
        public bool RecordWatchingTime { get; set; }
    }
}
