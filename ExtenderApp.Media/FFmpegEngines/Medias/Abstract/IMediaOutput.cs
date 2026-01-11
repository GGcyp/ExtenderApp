namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 定义媒体输出的通用接口。
    /// </summary>
    public interface IMediaOutput : IDisposable
    {
        /// <summary>
        /// 获取当前媒体输出支持的媒体类型。
        /// </summary>
        FFmpegMediaType MediaType { get; }

        /// <summary>
        /// 将一个媒体帧写入到输出。
        /// </summary>
        /// <param name="frame">要写入的媒体帧。</param>
        void WriteFrame(FFmpegFrame frame);

        /// <summary>
        /// 当播放器状态发生变化时调用此方法。
        /// </summary>
        /// <param name="state">新的播放器状态。</param>
        void PlayerStateChange(PlayerState state);
    }
}