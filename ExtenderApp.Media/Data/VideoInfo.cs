
namespace ExtenderApp.Media
{
    /// <summary>
    /// 视频信息类，用于存储视频相关的各种信息。
    /// </summary>
    public class VideoInfo
    {
        /// <summary>
        /// 视频文件的本地存储路径或网络路径（如果支持在线播放等情况）。
        /// </summary>
        public string VideoPath { get; set; }

        /// <summary>
        /// 视频已观看时长。
        /// </summary>
        public TimeSpan VideoWatchedDuration { get; set; }

        /// <summary>
        /// 视频的标题，通常可以从视频文件元数据或者相关播放平台获取（如果有对应接口）。
        /// </summary>
        public string VideoTitle { get; set; }

        /// <summary>
        /// 视频的总时长。
        /// </summary>
        public TimeSpan TotalVideoDuration { get; set; }

        /// <summary>
        /// 视频的分辨率，用宽度和高度表示，可以是从视频文件属性中解析得到。
        /// </summary>
        public int VideoWidth { get; set; }

        /// <summary>
        /// 视频的高度，与 <see cref="VideoWidth"/> 共同表示视频的分辨率。
        /// </summary>
        public int VideoHeight { get; set; }

        /// <summary>
        /// 视频文件的大小，单位可以是字节（Byte），便于了解存储占用情况等。
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 视频的创建时间，从文件属性或者相关元数据中获取，有助于管理视频的历史信息等。
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 视频的播放次数统计，可用于应用程序内分析用户观看习惯等。
        /// </summary>
        public int PlayCount { get; set; }

        /// <summary>
        /// 视频的收藏状态，布尔值表示是否被用户收藏，方便实现个性化功能，如收藏列表展示等。
        /// </summary>
        public bool IsFavorite { get; set; }

        /// <summary>
        /// 视频所属的分类或类别，例如电影、纪录片、教学视频等，有助于构建分类浏览功能。
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 视频的标签列表，用于对视频进行分类、标注等操作，方便用户查找和筛选视频。
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// 视频的评分（如果有相关评分机制，比如用户打分或者平台综合评分等情况）。
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// 获取或设置该配置是否被启用。
        /// </summary>
        /// <value>
        /// 如果配置被启用，则为true；否则为false。
        /// </value>
        public bool IsConfiguration => string.IsNullOrEmpty(VideoPath) && TotalVideoDuration == TimeSpan.Zero;

        public VideoInfo() : this(string.Empty)
        {
        }

        public VideoInfo(string videoPath)
        {
            VideoPath = videoPath;
        }

        public static bool operator ==(VideoInfo left, VideoInfo right)
        {
            if(left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(VideoInfo left, VideoInfo right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is VideoInfo info && VideoPath == info.VideoPath;
        }

        public override int GetHashCode()
        {
            return VideoPath.GetHashCode();
        }
    }
}
