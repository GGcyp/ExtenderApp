namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码模式枚举。
    /// </summary>
    public enum FFmpegDecodeMode
    {
        /// <summary>
        /// 默认：音视频正常播放
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 仅音频（不处理视频帧）
        /// </summary>
        AudioOnly = 1,

        /// <summary>
        /// 仅视频（不处理音频帧）
        /// </summary>
        VideoOnly = 2,
    }
}