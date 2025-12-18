namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 定义音频输出的接口，继承自 <see cref="IMediaOutput"/>。
    /// </summary>
    public interface IAudioOutput : IMediaOutput
    {
        /// <summary>
        /// 播放速率，1表示正常速度，2表示两倍速，0.5表示半速。
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// 获取或设置音频的播放速度（节奏）。
        /// 值为 1.0 表示正常速度。
        /// </summary>
        double Tempo { get; set; }

        /// <summary>
        /// 获取或设置音频的音量。
        /// 值的范围通常在 0.0 (静音) 到 1.0 (最大音量) 之间。
        /// </summary>
        float Volume { get; set; }
    }
}