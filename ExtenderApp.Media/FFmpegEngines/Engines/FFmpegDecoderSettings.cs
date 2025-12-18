namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码器参数设置类。
    /// 用于配置视频/音频解码相关参数，并提供帧调度事件。
    /// </summary>
    public class FFmpegDecoderSettings
    {
        /// <summary>
        /// 视频像素格式，默认 BGR24。
        /// </summary>
        public FFmpegPixelFormat PixelFormat { get; } = FFmpegPixelFormat.PIX_FMT_BGR24;

        /// <summary>
        /// 视频帧最大缓存数量。
        /// </summary>
        public int VideoMaxCacheLength { get; } = 3;

        /// <summary>
        /// 音频帧最大缓存数量。
        /// </summary>
        public int AudioMaxCacheLength { get; } = 6;

        /// <summary>
        /// 音频通道布局（如立体声=2）。
        /// </summary>
        public int Channels { get; } = 2;

        /// <summary>
        /// 音频采样格式（如 S16）。
        /// </summary>
        public FFmpegSampleFormat SampleFormat { get; } = FFmpegSampleFormat.SAMPLE_FMT_S16;

        /// <summary>
        /// 音频采样率（Hz），默认 44100。
        /// </summary>
        public int SampleRate { get; } = 44100;

        /// <summary>
        /// 获取每个音频样本的字节数。
        /// </summary>
        /// <returns>返回每个音频样本的字节数。</returns>
        public int GetBytesPerSample()
        {
            return FFmpegEngine.GetBytesPerSample(SampleFormat);
        }
    }
}