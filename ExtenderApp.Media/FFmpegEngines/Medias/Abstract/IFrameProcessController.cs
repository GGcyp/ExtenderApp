namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 帧处理控制器接口。
    /// <para>用于在播放循环中，按指定播放位置从解码器队列挑选合适的音视频帧，并将其写入对应的 <see cref="IMediaOutput"/>。</para>
    /// <para>同时负责向调用方返回“距离下一帧还有多久”的等待时间，以便上层决定等待策略（Sleep/自旋等）。</para>
    /// </summary>
    public interface IFrameProcessController
    {
        /// <summary>
        /// 获取帧处理集合，用于管理解码器与输出的映射关系（例如按 <see cref="FFmpegMediaType"/> 绑定音频/视频输出）。
        /// </summary>
        IFrameProcessCollection FrameProcessCollection { get; }

        /// <summary>
        /// 执行一次帧处理调度。
        /// </summary>
        /// <param name="outTime">允许输出的最大时间误差（毫秒）。</param>
        /// <param name="position">当前播放位置（毫秒）。</param>
        /// <param name="generation">当前播放世代号，用于过滤 Seek 前后的旧帧。</param>
        /// <returns>
        /// 距离下一帧到达的等待时间（毫秒）。
        /// <para>返回值含义通常为：</para>
        /// <list type="bullet">
        /// <item>
        /// <description>&gt; 0：下一帧尚未到达，应等待该毫秒数。</description>
        /// </item>
        /// <item>
        /// <description>0：下一帧可立即输出或无需等待。</description>
        /// </item>
        /// <item>
        /// <description>&lt; 0：当前无可用于计算等待的未来帧（例如队列为空或刚输出完），由上层决定退避策略。</description>
        /// </item>
        /// </list>
        /// </returns>
        int Processing(int outTime, long position, int generation);


        public void WaitFirstFrameAligned(int generation, int timeoutMs, out long position);
    }
}