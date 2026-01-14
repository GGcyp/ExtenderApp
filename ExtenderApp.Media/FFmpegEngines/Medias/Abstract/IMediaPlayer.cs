namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 媒体播放器抽象接口。
    /// <para>向上层提供统一的播放控制能力（播放/暂停/停止/跳转）与播放状态信息（位置、速度、音量、媒体信息等）， 并通过 <see cref="FrameProcessCollection"/> 暴露音视频输出的注册入口。</para>
    /// <para>该接口通常用于将 UI/业务逻辑与具体播放器实现（例如基于 FFmpeg 的实现）解耦，便于替换实现或进行单元测试。</para>
    /// </summary>
    public interface IMediaPlayer : IDisposable
    {
        /// <summary>
        /// 当前播放位置（毫秒）。
        /// <para>单位应与内部帧时间戳（PTS）换算后的单位保持一致，通常为毫秒。</para>
        /// </summary>
        long Position { get; }

        /// <summary>
        /// 播放速度倍率。
        /// <para>1 表示正常速度；实现通常会对过小的值设置下限以避免时钟推进过慢。</para>
        /// </summary>
        double SpeedRatio { get; set; }

        /// <summary>
        /// 媒体信息（时长、分辨率、帧率、采样率等）。
        /// </summary>
        FFmpegInfo Info { get; }

        /// <summary>
        /// 播放器当前状态。
        /// </summary>
        PlayerState State { get; }

        /// <summary>
        /// 帧处理集合，用于管理音视频输出（ <see cref="IMediaOutput"/>）的注册、替换与查询。
        /// <para>例如在 WPF 中注册视频输出以获得可绑定的图像源，或注册音频输出用于播放。</para>
        /// </summary>
        IFrameProcessCollection FrameProcessCollection { get; }

        /// <summary>
        /// 当前解码设置（从控制器读取）。
        /// </summary>
        FFmpegDecoderSettings Settings { get; }

        /// <summary>
        /// 播放进度回调事件。
        /// <para>通常由播放循环按一定频率触发，用于驱动 UI 的进度条、时间显示刷新等。</para>
        /// </summary>
        event Action? Playback;

        /// <summary>
        /// 当播放器状态发生变化时触发的事件。
        /// </summary>
        event Action<IMediaPlayer, PlayerState> PlayerStateChanged;

        /// <summary>
        /// 暂停播放。
        /// </summary>
        void Pause();

        /// <summary>
        /// 开始或恢复播放。
        /// </summary>
        void Play();

        /// <summary>
        /// 跳转到指定时间位置。
        /// </summary>
        /// <param name="timeSpan">目标时间。</param>
        void Seek(TimeSpan timeSpan);

        /// <summary>
        /// 跳转到指定播放位置（毫秒）。
        /// </summary>
        /// <param name="position">目标位置（毫秒）。</param>
        void Seek(long position);

        /// <summary>
        /// 停止播放并停止解码流程。
        /// </summary>
        /// <returns>用于等待停止完成的异步结果。</returns>
        ValueTask StopAsync();
    }
}