using System.Diagnostics;
using ExtenderApp.Data;

namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 帧处理控制器：负责从解码器队列中按时间戳挑选可输出帧，并写入对应的 <see cref="IMediaOutput"/>。
    /// <para>同时向上层返回“距离下一帧还有多久”的等待时间，用于控制播放循环的等待策略。</para>
    /// <para>主时钟优先级：若存在音频解码器则以音频为主，否则以视频为主。</para>
    /// </summary>
    internal class FrameProcessController : DisposableObject, IFrameProcessController
    {
        /// <summary>
        /// 内部帧处理集合：维护解码器与媒体输出的映射关系。
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

        public void WaitFirstFrameAligned(int generation, int timeoutMs, out long position)
        {
            position = 0;

            var audioTuple = _frameProcesses.GetDecoderAndOutput(FFmpegMediaType.AUDIO);
            var videoTuple = _frameProcesses.GetDecoderAndOutput(FFmpegMediaType.VIDEO);

            var audioDecoder = audioTuple.decoder;
            var videoDecoder = videoTuple.decoder;

            // 超时降级策略：
            // 1) 若存在音频，则优先等待音频首帧（generation 匹配）
            // 2) 超时仍无音频，则降级改用视频首帧作为 position（视频为主时钟）
            // 3) 取到主时钟 position 后，对齐另一路（丢弃到 Pts >= position）
            if (audioDecoder != null)
            {
                if (TryWaitFirstPtsForGeneration(audioDecoder, generation, timeoutMs, out position))
                {
                    if (videoDecoder != null)
                    {
                        AlignDecoderToPts(videoDecoder, generation, position);
                    }
                    return;
                }

                // 音频超时：降级到视频
                if (videoDecoder == null)
                {
                    return;
                }

                if (TryWaitFirstPtsForGeneration(videoDecoder, generation, timeoutMs, out position))
                {
                    AlignDecoderToPts(audioDecoder, generation, position);
                }

                return;
            }

            // 无音频：直接使用视频
            if (videoDecoder != null && TryWaitFirstPtsForGeneration(videoDecoder, generation, timeoutMs, out position))
            {
                return;
            }
        }

        private static bool TryWaitFirstPtsForGeneration(IFFmpegDecoder decoder, int generation, int timeoutMs, out long pts)
        {
            pts = 0;

            if (timeoutMs <= 0)
            {
                return TryGetFirstPtsForGeneration(decoder, generation, out pts);
            }

            // 轮询等待：既能尽快拿到首帧，也不会像 Thread.Sleep(timeoutMs) 那样“睡死”
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if (TryGetFirstPtsForGeneration(decoder, generation, out pts))
                {
                    return true;
                }

                if (sw.ElapsedMilliseconds >= timeoutMs)
                {
                    return false;
                }

                Thread.Sleep(1);
            }
        }

        private static bool TryGetFirstPtsForGeneration(IFFmpegDecoder decoder, int generation, out long pts)
        {
            pts = 0;

            while (decoder.TryPeekFrame(out FFmpegFrame frame))
            {
                if (frame.Generation == generation)
                {
                    pts = frame.Pts;
                    return true;
                }

                decoder.TryDequeueFrame(out frame);
                frame.Dispose();
            }

            return false;
        }

        private static void AlignDecoderToPts(IFFmpegDecoder decoder, int generation, long targetPts)
        {
            while (decoder.TryPeekFrame(out FFmpegFrame frame))
            {
                if (frame.Generation != generation)
                {
                    decoder.TryDequeueFrame(out frame);
                    frame.Dispose();
                    continue;
                }

                // 丢到与主时钟对齐：保留第一帧 Pts >= targetPts
                if (frame.Pts >= targetPts)
                {
                    break;
                }

                decoder.TryDequeueFrame(out frame);
                frame.Dispose();
            }
        }

        /// <summary>
        /// 执行一次帧调度处理：
        /// <list type="bullet">
        /// <item>
        /// <description>丢弃 generation 不匹配的旧帧（通常由 Seek 导致）。</description>
        /// </item>
        /// <item>
        /// <description>输出时间误差在阈值内的帧（|Pts - position| ≤ outTimeMs）。</description>
        /// </item>
        /// <item>
        /// <description>当队首帧还未到达（Pts &gt; position）时，返回其与 position 的差值作为建议等待时间。</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="outTimeMs">允许输出的最大时间误差（毫秒）。</param>
        /// <param name="position">当前播放位置（毫秒）。</param>
        /// <param name="generation">当前世代号，用于过滤 Seek 前后的旧帧。</param>
        /// <returns>
        /// 距离下一帧到达的等待时间（毫秒）。
        /// <para>&gt;0：下一帧未到，应等待；&lt;0：暂无可用于计算等待的未来帧。</para>
        /// <para>若存在音频解码器则返回音频的等待时间，否则返回视频的等待时间。</para>
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

            return audioNext < videoNext ? audioNext : videoNext;
        }

        /// <summary>
        /// 处理单路（单媒体类型）解码器的帧队列：
        /// <list type="bullet">
        /// <item>
        /// <description>窥视队首帧，根据 PTS 与 position 关系决定输出、丢弃或等待。</description>
        /// </item>
        /// <item>
        /// <description>旧代帧直接出队并释放。</description>
        /// </item>
        /// <item>
        /// <description>对已出队帧始终调用 <see cref="FFmpegFrame.Dispose"/> 释放底层缓冲。</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="decoder">目标解码器，可能为 <see langword="null"/>。</param>
        /// <param name="output">对应输出，可能为 <see langword="null"/>。</param>
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

#if DEBUG
                Debug.Print($"[FrameProcessController] MediaType={decoder.MediaType}, Pts={frame.Pts}, Position={position}, TimeDiff={timeDiff}");
#endif
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