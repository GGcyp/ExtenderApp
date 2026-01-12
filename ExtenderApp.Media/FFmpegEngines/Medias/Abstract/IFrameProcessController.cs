namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 帧处理控制器接口。
    /// <para>
    /// 职责：在播放循环中按“当前播放位置（position）”从各路解码器队列中挑选合适的音/视频帧，写入对应的 <see cref="IMediaOutput"/>，
    /// 并向调用方返回建议等待时间，用于驱动播放节拍（sleep / 自旋 / 事件等待等）。
    /// </para>
    /// <para>
    /// 时间基准：position 与帧的 PTS 均以毫秒计（ms）。
    /// </para>
    /// <para>
    /// 世代号（generation）：用于 Seek/切源等场景的数据隔离；当 generation 变化时，旧 generation 的帧应被丢弃以避免“播放回流”。
    /// </para>
    /// </summary>
    public interface IFrameProcessController : IDisposable
    {
        /// <summary>
        /// 获取帧处理集合，用于管理“解码器”与“输出端”的映射关系。
        /// <para>
        /// 常见用途：按 <see cref="FFmpegMediaType"/> 绑定音频/视频的 <see cref="IMediaOutput"/>，
        /// 以及查询某一路是否存在对应解码器输出。
        /// </para>
        /// </summary>
        IFrameProcessCollection FrameProcessCollection { get; }

        /// <summary>
        /// 执行一次帧处理调度。
        /// <para>
        /// 典型调用方式：由上层播放循环在每个 tick 调用一次，根据返回的等待时间决定下一次循环的等待策略。
        /// </para>
        /// <para>
        /// 调度语义（通常实现）：
        /// <list type="bullet">
        /// <item><description>丢弃 <paramref name="generation"/> 不匹配的旧帧。</description></item>
        /// <item><description>当队首帧 PTS 已到达或略滞后（|Pts - position| ≤ outTime）时，输出该帧。</description></item>
        /// <item><description>当队首帧 PTS 还未到达（Pts &gt; position）时，返回差值作为“建议等待时间”。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="outTime">允许输出的最大时间误差（毫秒）。</param>
        /// <param name="position">当前播放位置（毫秒）。</param>
        /// <param name="generation">当前播放世代号，用于过滤 Seek 前后的旧帧。</param>
        /// <returns>
        /// 距离下一帧到达的等待时间（毫秒）。
        /// <para>返回值含义通常为：</para>
        /// <list type="bullet">
        /// <item><description>&gt; 0：下一帧尚未到达，应等待该毫秒数。</description></item>
        /// <item><description>0：下一帧可立即输出或无需等待。</description></item>
        /// <item><description>&lt; 0：当前无可用于计算等待的未来帧（例如队列为空或刚输出完），由上层决定退避策略。</description></item>
        /// </list>
        /// </returns>
        int Processing(int outTime, long position, int generation);
    }
}