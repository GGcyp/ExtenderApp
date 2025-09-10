

using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// 视频输出设置类，包含视频处理相关的配置选项。
    /// </summary>
    public class VideoOutSettings
    {
        public AVPixelFormat TargetPixelFormat { get; set; } = AVPixelFormat.AV_PIX_FMT_BGR24;

        /// <summary>
        /// 指示源格式是否与目标格式相同。
        /// </summary>
        public bool IsSourceFormatEqualTargetFormat { get; set; }
    }
}
