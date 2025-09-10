namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// 媒体基础信息封装类，包含视频或音频流的关键参数。
    /// 用于描述媒体文件的格式、时长、编解码器等元数据，便于界面展示和业务逻辑处理。
    /// </summary>
    public struct FFmpegInfo
    {
        /// <summary>
        /// 默认值，表示未设置或未知的参数值。
        /// </summary>
        public const int DefaultValue = -1;

        /// <summary>
        /// 视频宽度（像素）。
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// 视频高度（像素）。
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// 音频采样率（Hz）。
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// 音频声道数。
        /// </summary>
        public int Channels { get; private set; }

        /// <summary>
        /// 媒体时长（秒）。
        /// </summary>
        public double Duration { get; private set; }

        /// <summary>
        /// 视频帧率（FPS）。
        /// </summary>
        public double FrameRate { get; private set; }

        /// <summary>
        /// 媒体时长（TimeSpan 格式）。
        /// </summary>
        public TimeSpan DurationTimeSpan { get; private set; }

        /// <summary>
        /// 媒体码率（比特率，单位：bps）。
        /// </summary>
        public long BitRate { get; private set; }

        /// <summary>
        /// 视频编解码器名称。
        /// </summary>
        public string VideoCodecName { get; }

        /// <summary>
        /// 音频编码器名称。
        /// </summary>
        public string AudioCodecName { get; }

        /// <summary>
        /// 媒体源 URI 或文件路径。
        /// </summary>
        public string Uri { get; }

        /// <summary>
        /// 是否为流媒体（时长小于等于0视为流媒体）。
        /// </summary>
        public bool IsStreamVideo => Duration <= 0;

        /// <summary>
        /// 构造函数，初始化媒体信息各字段。
        /// </summary>
        /// <param name="uri">媒体源 URI 或文件路径。</param>
        /// <param name="videoCodecName">视频编解码器名称。</param>
        /// <param name="audioCodecName">音频编解码器名称。</param>
        public FFmpegInfo(string uri, string videoCodecName, string audioCodecName)
        {
            Uri = uri;
            VideoCodecName = videoCodecName;
            AudioCodecName = audioCodecName;
            Width = DefaultValue;
            Height = DefaultValue;
            SampleRate = DefaultValue;
            Channels = DefaultValue;
            Duration = DefaultValue;
            FrameRate = DefaultValue;
            DurationTimeSpan = TimeSpan.Zero;
            BitRate = DefaultValue;
        }

        /// <summary>
        /// 写入或更新媒体信息各字段，仅在当前字段为默认值时更新。
        /// </summary>
        /// <param name="width">视频宽度（像素）。</param>
        /// <param name="height">视频高度（像素）。</param>
        /// <param name="sampleRate">音频采样率（Hz）。</param>
        /// <param name="channels">音频声道数。</param>
        /// <param name="duration">媒体时长（秒）。</param>
        /// <param name="frameRate">视频帧率（FPS）。</param>
        /// <param name="bitRate">媒体码率（比特率，单位：bps）。</param>
        public void SetInfo(int width, int height, int sampleRate, int channels, double duration, double frameRate, long bitRate)
        {
            Width = Width == DefaultValue ? width : Width;
            Height = Height == DefaultValue ? height : Height;
            SampleRate = SampleRate == DefaultValue ? sampleRate : SampleRate;
            Channels = Channels == DefaultValue ? channels : Channels;
            Duration = Duration == DefaultValue ? duration : Duration;
            FrameRate = FrameRate == DefaultValue ? frameRate : FrameRate;
            BitRate = BitRate == DefaultValue ? bitRate : BitRate;
            DurationTimeSpan = DurationTimeSpan == TimeSpan.Zero ? Duration == DefaultValue ? TimeSpan.Zero : TimeSpan.FromSeconds(duration) : DurationTimeSpan;
        }
    }
}
