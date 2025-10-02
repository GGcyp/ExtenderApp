

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg媒体类型枚举
    /// </summary>
    public enum FFmpegMediaType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        UNKNOWN = -1,
        /// <summary>
        /// 视频类型
        /// </summary>
        VIDEO,
        /// <summary>
        /// 音频类型
        /// </summary>
        AUDIO,
        /// <summary>
        /// 数据类型
        /// </summary>
        DATA,
        /// <summary>
        /// 字幕类型
        /// </summary>
        SUBTITLE,
        /// <summary>
        /// 附件类型
        /// </summary>
        ATTACHMENT,
        /// <summary>
        /// 类型数量
        /// </summary>
        NB
    }
}
