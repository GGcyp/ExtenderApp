namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 编码器设置。
    /// <para>目前用于抽象层契约，具体字段可按“写文件/推流/转码”的目标逐步补齐。</para>
    /// </summary>
    public sealed class FFmpegEncoderSettings
    {
        /// <summary>
        /// 目标码率（bit/s）。
        /// </summary>
        public long BitRate { get; set; }

        /// <summary>
        /// GOP 长度（视频关键帧间隔）。仅对视频编码器有意义。
        /// </summary>
        public int GopSize { get; set; }

        /// <summary>
        /// 视频帧率（fps）。仅对视频编码器有意义。
        /// </summary>
        public double FrameRate { get; set; }

        /// <summary>
        /// 音频采样率（Hz）。仅对音频编码器有意义。
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// 音频声道数。仅对音频编码器有意义。
        /// </summary>
        public int Channels { get; set; }
    }
}