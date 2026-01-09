using ExtenderApp.FFmpegEngines.Decoders;

namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 基于播放位置从解码器队列中选择可输出的帧，并将其写入到指定输出。
    /// </summary>
    /// <remarks>
    /// 处理规则（以 <c>frame.Pts - position</c> 的差值为准）：
    /// <list type="bullet">
    /// <item><description><c>timeDiff &gt; 0</c>：下一帧尚未到输出时机，停止处理并返回该差值，供外层决定等待时长。</description></item>
    /// <item><description><c>|timeDiff| &lt;= outTime</c>：认为在允许误差内，写出该帧。</description></item>
    /// <item><description><c>timeDiff &lt; -outTime</c>：帧已明显过期，丢弃。</description></item>
    /// </list>
    /// 另外，当帧的 <c>Generation</c> 与当前解码器世代不一致时，将直接出队并丢弃，避免旧世代数据污染当前播放。
    /// </remarks>
    internal class FrameProcessing
    {
        /// <summary>
        /// 帧写出目标。
        /// </summary>
        private readonly IMediaOutput _output;

        /// <summary>
        /// 帧来源解码器（内部维护待输出帧队列）。
        /// </summary>
        private readonly FFmpegDecoder _decoder;

        /// <summary>
        /// 获取一个值，该值指示此实例是否为空（未正确初始化）。
        /// </summary>
        /// <remarks>
        /// 当输出或解码器任一为 <see langword="null"/> 时为 <see langword="true"/>。
        /// </remarks>
        public bool IsEmpty => _output == null || _decoder == null;

        /// <summary>
        /// 初始化 <see cref="FrameProcessing"/> 的新实例。
        /// </summary>
        /// <param name="output">用于写出帧的媒体输出。</param>
        /// <param name="decoder">提供帧队列的 FFmpeg 解码器。</param>
        public FrameProcessing(IMediaOutput output, FFmpegDecoder decoder)
        {
            _output = output;
            _decoder = decoder;
        }

        /// <summary>
        /// 处理解码器队列中的帧：写出可输出帧并丢弃过期/旧世代帧。
        /// </summary>
        /// <param name="outTime">允许的最大输出时间差（毫秒）。</param>
        /// <param name="position">当前播放位置（毫秒）。</param>
        /// <param name="generation">当前解码器世代标识，用于过滤旧世代帧。</param>
        /// <returns>
        /// 下一帧与当前播放位置的时间差（毫秒）。
        /// 返回值为正表示“还没到输出时间”；返回 <c>-1</c> 通常表示已写出/丢弃了一些帧且队列中暂无可用于计算的下一帧。
        /// </returns>
        public int Process(int outTime, long position, int generation)
        {
            if (IsEmpty)
                return 0;

            FFmpegFrame frame = default;
            long timeDiff = -1;

            while (_decoder.TryPeekFrame(out frame))
            {
                // 丢弃旧世代帧，避免 seek/重建解码器后输出历史数据。
                if (frame.Generation != generation)
                {
                    _decoder.TryDequeueFrame(out frame);
                    frame.Dispose();
                    continue;
                }

                // 计算帧相对播放位置的时间差：>0 表示帧在未来；<0 表示帧已过期。
                timeDiff = frame.Pts - position;

                // 队首帧在未来：停止，交给外层按 timeDiff 等待。
                if (timeDiff > 0)
                    break;

                // 队首帧不在未来：尝试出队处理；出队失败则重试。
                if (!_decoder.TryDequeueFrame(out frame))
                    continue;

                // 在允许误差内写出；否则视为过期帧直接丢弃。
                if (Math.Abs(timeDiff) <= outTime)
                {
                    _output.WriteFrame(frame);
                }

                frame.Dispose();
                timeDiff = -1;
            }

            return (int)timeDiff;
        }
    }
}