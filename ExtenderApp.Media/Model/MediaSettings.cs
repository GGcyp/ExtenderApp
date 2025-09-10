using ExtenderApp.Models;

namespace ExtenderApp.Media.Models
{
    public class MediaSettings : DataModel
    {
        /// <summary>
        /// 音量
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// 是否记录观看时间
        /// </summary>
        public bool RecordWatchingTime { get; set; }

        /// <summary>
        /// 视频文件不存在则删除
        /// </summary>
        public bool VideoNotExist { get; set; }

        public MediaSettings()
        {
            Volume = 0.5;
            RecordWatchingTime = true;
            VideoNotExist = true;
        }
    }
}
