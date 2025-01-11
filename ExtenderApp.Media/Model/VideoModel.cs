namespace ExtenderApp.Media
{
    /// <summary>
    /// 视频模型类，用于表示视频操作相关的属性和方法。
    /// </summary>
    public class VideoModel
    {
        /// <summary>
        /// 获取或设置当前视频信息。
        /// </summary>
        public VideoInfo CurrentVideoInfo { get; set; }

        /// <summary>
        /// 播放视频。
        /// </summary>
        public Action PlayAction { get; set; }

        /// <summary>
        /// 停止视频。
        /// </summary>
        public Action StopAction { get; set; }

        /// <summary>
        /// 暂停视频。
        /// </summary>
        public Action PauseAction { get; set; }

        /// <summary>
        /// 设置快进视频，参数表示跳过的时间。
        /// </summary>
        public Action<double> FastForwardAction { get; set; }

        /// <summary>
        /// 打开视频。
        /// </summary>
        public Action OpenVideoAction { get; set; }

        /// <summary>
        /// 获取或设置用于设置速度比例的动作。
        /// </summary>
        public Action<double> SpeedRatioAction { get; set; }

        /// <summary>
        /// 获取或设置用于获取自然时间间隔的函数。
        /// </summary>
        public Func<TimeSpan> NaturalTimeSpanFunc { get; set; }

        /// <summary>
        /// 获取或设置用于设置位置的动作。
        /// </summary>
        public Action<TimeSpan> SetPosition { get; set; }

        /// <summary>
        /// 获取或设置用于获取当前位置的时间间隔的函数。
        /// </summary>
        public Func<TimeSpan> GetPosition { get; set; }
    }
}
