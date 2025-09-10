namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// 表示视频播放的状态。
    /// </summary>
    public enum FFmpegState
    {
        /// <summary>
        /// 未初始化，视频尚未加载或准备。
        /// </summary>
        Uninitialized,

        /// <summary>
        /// 正在播放视频。
        /// </summary>
        Playing,

        /// <summary>
        /// 视频已暂停。
        /// </summary>
        Paused,

        /// <summary>
        /// 视频已停止播放。
        /// </summary>
        Stopped,

        /// <summary>
        /// 视频已经播放完成。
        /// </summary>
        Completed
    }
}
