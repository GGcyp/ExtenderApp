namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码器参数设置类。
    /// <para>提供视频/音频解码的基础配置项，以及 Seek 场景下用于丢弃旧数据的“代际（Generation）”标记。</para>
    /// <para>线程安全说明：本类会被多个后台解码任务读取；其中 <see cref="generation"/> 会在 Seek 时递增，供生产/消费双方判定数据是否过期。</para>
    /// </summary>
    public class FFmpegDecoderSettings
    {
        /// <summary>
        /// 视频像素格式（解码输出像素格式），默认 BGR24。
        /// </summary>
        public FFmpegPixelFormat PixelFormat { get; } = FFmpegPixelFormat.PIX_FMT_BGR24;

        #region PacketCacheCount

        /// <summary>
        /// 视频包最大缓存数量（通常对应输入包通道的容量）。
        /// <para>用于吸收 demux 端的“突发读包”；过小会导致 demux 更频繁阻塞，过大会增加停播/Seek 时需要清理的积压。</para>
        /// </summary>
        public int VideoPacketCacheCount { get; } = 8;

        /// <summary>
        /// 音频包最大缓存数量（通常对应输入包通道的容量）。
        /// </summary>
        public int AudioPacketCacheCount { get; } = 16;

        #endregion PacketCacheCount

        #region FrameCacheCount

        /// <summary>
        /// 视频帧最大缓存数量（通常对应输出帧通道的容量）。
        /// <para>值越小背压越强（内存更稳，但更容易阻塞解码）；值越大可吸收消费端抖动（但占用更多内存）。</para>
        /// </summary>
        public int VideoFrameCacheCount { get; } = 3;

        /// <summary>
        /// 音频帧最大缓存数量（通常对应输出帧通道的容量）。
        /// </summary>
        public int AudioFrameCacheCount { get; } = 8;

        #endregion FrameCacheCount

        /// <summary>
        /// 音频通道数（例如：立体声=2）。
        /// </summary>
        public int Channels { get; } = 2;

        /// <summary>
        /// 音频采样格式（例如 S16）。
        /// </summary>
        public FFmpegSampleFormat SampleFormat { get; } = FFmpegSampleFormat.SAMPLE_FMT_S16;

        /// <summary>
        /// 音频采样率（Hz），默认 44100。
        /// </summary>
        public int SampleRate { get; } = 44100;

        /// <summary>
        /// 获取每个音频样本的字节数。
        /// <para>该值与 <see cref="SampleFormat"/> 对应，例如 S16=2 字节。</para>
        /// </summary>
        public int GetBytesPerSample()
        {
            return FFmpegEngine.GetBytesPerSample(SampleFormat);
        }
    }
}