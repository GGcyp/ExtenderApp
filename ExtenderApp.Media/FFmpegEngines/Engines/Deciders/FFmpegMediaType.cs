

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg媒体类型枚举
    /// </summary>
    public enum FFmpegMediaType : int
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        UNKNOWN = -1,
        /// <summary>
        /// 视频类型
        /// </summary>
        VIDEO = 0,
        /// <summary>
        /// 音频类型
        /// </summary>
        AUDIO = 1,
        /// <summary>
        /// 数据类型
        /// </summary>
        DATA = 2,
        /// <summary>
        /// 字幕类型
        /// </summary>
        SUBTITLE = 3,
        /// <summary>
        /// 附件类型
        /// </summary>
        ATTACHMENT = 4,
        /// <summary>
        /// 类型数量
        /// </summary>
        NB = 5,
    }
}
