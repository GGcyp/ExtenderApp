using ExtenderApp.Contracts;

namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 帧处理控制器：负责从解码器队列中按时间戳（PTS）挑选可输出帧，并写入对应的 <see cref="IMediaOutput"/>。
    /// <para>本类位于“解码”与“渲染/播放”之间，承担“帧调度（Frame Scheduling）”职责：</para>
    /// <list type="bullet">
    /// <item>
    /// <description>从每路 <see cref="IFFmpegDecoder"/> 的输出队列中窥视（peek）队首帧，判断是否应当输出/丢弃/等待。</description>
    /// </item>
    /// <item>
    /// <description>在 Seek 或重建播放链路时，基于 <c>generation</c> 丢弃旧代帧，保证不会输出跳转前的缓存帧。</description>
    /// </item>
    /// <item>
    /// <description>以音频作为主时钟（若存在音频解码器），否则退化使用视频作为主时钟；并在开始播放时对齐首帧时间线。</description>
    /// </item>
    /// <item>
    /// <description>向上层返回“距离下一帧还有多久”的建议等待时间（毫秒），用于控制播放循环的 sleep/spin 策略。</description>
    /// </item>
    /// </list>
    /// <para>资源管理：所有从解码器队列中出队的 <see cref="FFmpegFrame"/> 都必须调用 <see cref="FFmpegFrame.Dispose"/> 释放底层缓冲。</para>
    /// </summary>
    internal class FrameProcessController : DisposableObject, IFrameProcessController
    {
        /// <summary>
        /// 内部帧处理集合：维护解码器与媒体输出（ <see cref="IMediaOutput"/>）的映射关系。
        /// </summary>
        private readonly FrameProcessCollection _frameProcesses;

        /// <summary>
        /// 对外暴露的帧处理集合，用于注册/查询音视频输出。
        /// </summary>
        public IFrameProcessCollection FrameProcessCollection => _frameProcesses;

        /// <summary>
        /// 创建帧处理控制器。
        /// </summary>
        /// <param name="controller">解码控制器，用于访问解码器集合。</param>
        public FrameProcessController(IFFmpegDecoderController controller)
        {
            _frameProcesses = new(controller.DecoderCollection);
        }

        /// <summary>
        /// 执行一次帧调度处理。
        /// <para>会分别处理音频与视频两路队列，并返回“下一次应该等待多久”的建议值（毫秒）。</para>
        /// </summary>
        /// <param name="outTimeMs">允许输出的最大时间误差（毫秒）。</param>
        /// <param name="position">当前播放位置（毫秒）。</param>
        /// <param name="generation">当前世代号，用于过滤 Seek 前后的旧帧。</param>
        /// <returns>
        /// 距离下一帧到达的等待时间（毫秒）。
        /// <list type="bullet">
        /// <item>
        /// <description>&gt; 0：存在未来帧（Pts &gt; position），应等待该差值。</description>
        /// </item>
        /// <item>
        /// <description>-1：暂无可用于计算等待的未来帧（队列为空或仅有过期帧）。</description>
        /// </item>
        /// </list>
        /// 若存在音频帧等待时间则优先使用更小的等待值作为调度依据。
        /// </returns>
        public int Processing(int outTimeMs, long position, int generation)
        {
            var audioTupe = _frameProcesses.GetDecoderAndOutput(FFmpegMediaType.AUDIO);
            var videoTupe = _frameProcesses.GetDecoderAndOutput(FFmpegMediaType.VIDEO);

            int audioNext = ProcessOne(audioTupe.decoder, audioTupe.output, outTimeMs, position, generation);
            int videoNext = ProcessOne(videoTupe.decoder, videoTupe.output, outTimeMs, position, generation);

            if (audioNext < 0)
                return videoNext;
            if (videoNext < 0)
                return audioNext;

            return Math.Min(audioNext, videoNext);
        }

        /// <summary>
        /// 处理单路（单媒体类型）解码器的帧队列。
        /// <para>逻辑为：
        /// <list type="bullet">
        /// <item>
        /// <description>peek 队首帧：若 generation 不匹配则出队并释放（旧代丢弃）。</description>
        /// </item>
        /// <item>
        /// <description>若 <c>Pts &gt; position</c>：返回差值作为建议等待时间。</description>
        /// </item>
        /// <item>
        /// <description>若 <c>Pts ≤ position</c>：尝试出队；若时间误差在阈值内则输出，否则丢弃。</description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>资源约束：所有成功出队的 <see cref="FFmpegFrame"/> 必须调用 <see cref="FFmpegFrame.Dispose"/> 释放底层缓冲。</para>
        /// </summary>
        /// <param name="decoder">目标解码器；为 <see langword="null"/> 时返回 -1。</param>
        /// <param name="output">对应输出；为 <see langword="null"/> 时仅丢弃帧不写入。</param>
        /// <param name="outTimeMs">允许输出的最大时间误差（毫秒）。</param>
        /// <param name="position">当前播放位置（毫秒）。</param>
        /// <param name="generation">当前世代号。</param>
        /// <returns>距离下一帧到达的等待时间（毫秒）；若无未来帧可等待则返回 -1。</returns>
        private int ProcessOne(IFFmpegDecoder? decoder, IMediaOutput? output, int outTimeMs, long position, int generation)
        {
            if (decoder == null)
            {
                return -1;
            }

            FFmpegFrame frame = default;

            while (decoder.TryPeekFrame(out frame))
            {
                if (frame.Generation != generation)
                {
                    decoder.TryDequeueFrame(out frame);
                    frame.Dispose();
                    continue;
                }

                long timeDiff = frame.Pts - position;
                if (timeDiff > 0)
                {
                    return (int)timeDiff;
                }

                if (!decoder.TryDequeueFrame(out frame))
                {
                    continue;
                }

                if (Math.Abs(timeDiff) <= outTimeMs)
                {
                    output?.WriteFrame(frame);
                }

                frame.Dispose();
            }

            return -1;
        }

        /// <summary>
        /// 释放由控制器持有的托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _frameProcesses.Dispose();
        }
    }
}