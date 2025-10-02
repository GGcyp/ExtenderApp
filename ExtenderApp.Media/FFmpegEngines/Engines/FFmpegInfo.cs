namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// 媒体基础信息封装类，包含视频或音频流的关键参数。 用于描述媒体文件的格式、时长、编解码器等元数据，便于界面展示和业务逻辑处理。
    /// </summary>
    public struct FFmpegInfo
    {
        /// <summary>
        /// 默认值，表示未设置或未知的参数值。
        /// </summary>
        public const int DefaultValue = -1;

        /// <summary>
        /// 空的媒体信息实例，所有字段均为默认值。
        /// </summary>
        public static readonly FFmpegInfo Empty = new FFmpegInfo(string.Empty, FFmpegPixelFormat.PIX_FMT_NONE, FFmpegSampleFormat.SAMPLE_FMT_NONE, string.Empty, string.Empty);

        /// <summary>
        /// 视频宽度（像素）。
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// 视频高度（像素）。
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// 音频采样率（Hz）。
        /// </summary>
        public int SampleRate { get; internal set; }

        /// <summary>
        /// 音频声道数。
        /// </summary>
        public int Channels { get; internal set; }

        /// <summary>
        /// 媒体时长（微秒）。
        /// </summary>
        public long Duration { get; internal set; }

        /// <summary>
        /// 生成 Duration 对应的 TimeSpan 对象，便于时间计算和显示。
        /// </summary>
        public TimeSpan DurationTimeSpan => TimeSpan.FromMilliseconds(Duration);

        /// <summary>
        /// 视频帧率（FPS）。
        /// </summary>
        public double Rate { get; internal set; }

        /// <summary>
        /// 媒体码率（比特率，单位：bps）。
        /// </summary>
        public long BitRate { get; internal set; }

        /// <summary>
        /// 视频编解码器名称。
        /// </summary>
        public string VideoCodecName { get; }

        /// <summary>
        /// 音频编码器名称。
        /// </summary>
        public string AudioCodecName { get; }

        /// <summary>
        /// 视频的像素格式。
        /// </summary>
        public FFmpegPixelFormat PixelFormat { get; }

        /// <summary>
        /// 媒体源 URI 或文件路径。
        /// </summary>
        public Uri MediaUri { get; }

        /// <summary>
        /// 声音采样格式。
        /// </summary>
        public FFmpegSampleFormat SampleFormat { get; }

        /// <summary>
        /// 是否为流媒体（时长小于等于0视为流媒体）。
        /// </summary>
        public bool IsStreamMedia => Duration <= 0;

        /// <summary>
        /// 构造函数，初始化媒体信息各字段。
        /// </summary>
        /// <param name="uri">媒体源 URI 或文件路径。</param>
        /// <param name="pixelFormat">视频像素格式。</param>
        /// <param name="sampleFormat">声音采样格式。</param>
        /// <param name="videoCodecName">视频编解码器名称。</param>
        /// <param name="audioCodecName">音频编解码器名称。</param>
        public FFmpegInfo(string uri, FFmpegPixelFormat pixelFormat, FFmpegSampleFormat sampleFormat, string videoCodecName, string audioCodecName)
        {
            MediaUri = new Uri(uri);
            PixelFormat = pixelFormat;
            SampleFormat = sampleFormat;
            VideoCodecName = videoCodecName;
            AudioCodecName = audioCodecName;
            Width = DefaultValue;
            Height = DefaultValue;
            SampleRate = DefaultValue;
            Channels = DefaultValue;
            Duration = DefaultValue;
            Rate = DefaultValue;
            BitRate = DefaultValue;
        }
    }
}