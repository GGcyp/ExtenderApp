using System.Runtime.CompilerServices;
using System.Threading.Channels;
using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器抽象基类。
    /// <para>本类负责：</para>
    /// <list type="bullet">
    /// <item>
    /// <description>通过 <see cref="Channel{T}"/> 维护“待解码包(packet)”与“已解码帧(frame)”的生产/消费队列。</description>
    /// </item>
    /// <item>
    /// <description>在 Seek 场景下基于“代际（generation）”丢弃旧数据，保证跳转后不会继续解码/输出跳转前的包与帧。</description>
    /// </item>
    /// <item>
    /// <description>封装 FFmpeg 的 send/receive 解码工作流： <c>avcodec_send_packet</c> + <c>avcodec_receive_frame</c>。</description>
    /// </item>
    /// <item>
    /// <description>对输出帧的时间戳做“自修复（Fixup）”：补齐无效 PTS、并保证单调递增，以降低播放调度抖动。</description>
    /// </item>
    /// </list>
    /// </summary>
    public abstract class FFmpegDecoder : DisposableObject, IFFmpegDecoder
    {
        /// <summary>
        /// 待解码包通道。
        /// <para>上游（解复用/控制器）写入 <see cref="FFmpegPacket"/>；解码线程从该通道读取并送入 FFmpeg 解码器。</para>
        /// <para>注意：包所有权在写入成功后转移给解码线程；写入失败时需归还底层 <see cref="AVPacket"/>（本类已处理）。</para>
        /// </summary>
        private readonly Channel<FFmpegPacket> _packetChannel;

        /// <summary>
        /// 已解码帧通道。
        /// <para>解码线程写入 <see cref="FFmpegFrame"/>；下游（播放/渲染）读取并消费帧数据。</para>
        /// <para>该通道为有界队列：用于对输出施加背压，避免消费端变慢时内存无限增长。</para>
        /// </summary>
        private readonly Channel<FFmpegFrame> _frameChannel;

        /// <summary>
        /// 控制器上下文：提供代际（generation）与共享的 <see cref="FFmpegEngine"/>。
        /// </summary>
        private readonly FFmpegDecoderControllerContext _controllerContext;

        /// <summary>
        /// 上一帧的时间戳（毫秒）。
        /// <para>用于时间戳自修复（Fixup）：当帧 PTS 无效或回退时，使用该值 + duration 推导出一个单调的 PTS。</para>
        /// <para>注意：该字段按“单解码线程”访问设计，无额外并发保护。</para>
        /// </summary>
        private long lastFramePtsMs = FFmpegEngine.InvalidTimestamp;

        /// <summary>
        /// FFmpeg 引擎实例，用于调用底层 FFmpeg API（send/receive、时间戳换算、资源池回收等）。
        /// </summary>
        protected FFmpegEngine Engine => _controllerContext.Engine;

        /// <summary>
        /// 解码器上下文，包含流索引、媒体类型以及底层 <see cref="AVCodecContext"/> / <see cref="AVStream"/> 等指针。
        /// </summary>
        protected FFmpegDecoderContext DecoderContext { get; }

        /// <summary>
        /// 媒体基础信息（时长、分辨率、帧率等）。
        /// </summary>
        protected FFmpegInfo Info => _controllerContext.Info;

        /// <inheritdoc/>
        public int StreamIndex => DecoderContext.StreamIndex;

        /// <inheritdoc/>
        public FFmpegMediaType MediaType => DecoderContext.MediaType;

        /// <summary>
        /// 初始化 <see cref="FFmpegDecoder"/>。
        /// </summary>
        /// <param name="decoderContext">解码器上下文（codec/stream 指针等）。</param>
        /// <param name="controllerContext">控制器上下文（Engine + generation 提供者）。</param>
        /// <param name="packetCacheCount">输入包通道最大缓存数量（有界）。</param>
        /// <param name="frameCacheCount">输出帧通道最大缓存数量（有界）。</param>
        public FFmpegDecoder(FFmpegDecoderContext decoderContext, FFmpegDecoderControllerContext controllerContext, int packetCacheCount, int frameCacheCount)
        {
            _controllerContext = controllerContext;
            DecoderContext = decoderContext;

            _packetChannel = CreateChannel<FFmpegPacket>(packetCacheCount);
            _frameChannel = CreateChannel<FFmpegFrame>(frameCacheCount);
        }

        /// <summary>
        /// 创建一个指定容量的有界通道（Bounded Channel）。
        /// <para>配置为单读（解码线程）+ 多写（控制器多线程投递）。</para>
        /// </summary>
        private Channel<T> CreateChannel<T>(int capacity)
        {
            return Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
            });
        }

        /// <summary>
        /// 将一个带代际信息的 packet 投递到解码器队列。
        /// <para>优先走快速路径 <see cref="ChannelWriter{T}.TryWrite(T)"/>，失败则走慢路径等待可写。</para>
        /// </summary>
        public void EnqueuePacket(FFmpegPacket packet, CancellationToken token)
        {
            if (_packetChannel.Writer.TryWrite(packet))
            {
                return;
            }

            try
            {
                SlowEnqueuePacket(packet, token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Stop/Dispose 的正常退出路径：吞掉取消异常（SlowEnqueuePacket 已负责归还 packet）
            }
            catch (ChannelClosedException)
            {
                // 通道已完成/关闭：吞掉（SlowEnqueuePacket 已负责归还 packet）
            }
        }

        /// <summary>
        /// 慢路径：当通道暂时不可写时等待恢复可写，再写入。
        /// <para>若等待/写入过程中出现异常，本方法负责归还 packet 以避免泄漏。</para>
        /// </summary>
        private async ValueTask SlowEnqueuePacket(FFmpegPacket packet, CancellationToken token)
        {
            try
            {
                while (await _packetChannel.Writer.WaitToWriteAsync(token).ConfigureAwait(false))
                {
                    if (_packetChannel.Writer.TryWrite(packet))
                    {
                        return;
                    }
                }

                // WaitToWriteAsync 返回 false：writer 已完成（TryComplete）或被关闭
                return;
            }
            catch (ChannelClosedException)
            {
                // 通道已关闭：释放/归还资源，按正常结束处理
                Engine.Return(ref packet);
            }
            catch
            {
                Engine.Return(ref packet);
                throw;
            }
        }

        /// <summary>
        /// 解码线程主循环：持续从 <see cref="_packetChannel"/> 读取 packet，并解码输出到 <see cref="_frameChannel"/>。
        /// <para>代际（generation）变化时会 flush：清空队列 + flush codec。</para>
        /// </summary>
        public async Task DecodeLoopAsync(CancellationToken token)
        {
            int localGeneration = 0;

            while (!token.IsCancellationRequested)
            {
                FFmpegPacket packet;
                try
                {
                    packet = await _packetChannel.Reader.ReadAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    return;
                }
                catch (ChannelClosedException)
                {
                    // 通道完成：正常结束
                    return;
                }

                int currentGeneration = _controllerContext.GetCurrentGeneration();
                if (localGeneration != currentGeneration)
                {
                    localGeneration = currentGeneration;
                    FlushPrivate(currentGeneration);
                }

                if (packet.Generation != localGeneration)
                {
                    Engine.Return(ref packet);
                    continue;
                }

                ProcessOnePacket(packet, token);
            }
        }

        /// <summary>
        /// 处理单个 packet：send 进 codec，然后 receive 尽可能多的 frame。
        /// <para>注意：不在此处循环读取 packet；循环由 <see cref="DecodeLoopAsync"/> 驱动。</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessOnePacket(FFmpegPacket packet, CancellationToken token)
        {
            try
            {
                NativeIntPtr<AVPacket> pktPtr = packet.PacketPtr;

                int result = Engine.SendPacket(DecoderContext, ref pktPtr);
                if (result < 0 || token.IsCancellationRequested)
                {
                    return;
                }

                ProcessFrames(packet.Generation, token);
            }
            finally
            {
                // 归还 packet（无论是否成功 send）
                Engine.Return(ref packet);
            }
        }

        /// <summary>
        /// receive 循环：从 codec 中持续取出解码帧并推送到输出队列。
        /// <para>循环退出条件：</para>
        /// <list type="bullet">
        /// <item><description>取消请求（token cancel）。</description></item>
        /// <item><description>EAGAIN：codec 需要更多输入包。</description></item>
        /// <item><description>EOF：codec 内部已结束（通常在 drain/flush 场景出现）。</description></item>
        /// <item><description>代际变化：当 <paramref name="packetGeneration"/> 与当前 generation 不一致时主动停止旧代输出。</description></item>
        /// </list>
        /// </summary>
        private void ProcessFrames(int packetGeneration, CancellationToken token)
        {
            var framePtr = Engine.GetFrame();
            int currentGeneration = _controllerContext.GetCurrentGeneration();

            try
            {
                while (!token.IsCancellationRequested &&
                    currentGeneration == packetGeneration)
                {
                    int ret = Engine.ReceiveFrame(DecoderContext, ref framePtr);

                    if (token.IsCancellationRequested ||
                        Engine.IsTryAgain(ret) ||
                        Engine.IsEndOfFile(ret))
                    {
                        break;
                    }
                    else if (!Engine.IsSuccess(ret))
                    {
                        Engine.ShowException("接收帧失败", ret);
                        break;
                    }

                    long rawPts = GetFrameTimestampMs(framePtr);

                    // duration 由子类按媒体类型估算：视频按帧率，音频按 nb_samples/sample_rate。
                    long durationMs = GetFrameDurationMs(framePtr);

                    // PTS 自修复：补齐无效时间戳、并确保单调递增，避免播放调度抖动。
                    long framePts = FixupFrameTimestampMs(rawPts, durationMs);

                    ProcessFrame(framePtr, out var block);

                    FFmpegFrame frame = new(currentGeneration, block, framePts);
                    EnqueueFrame(frame, token);

                    currentGeneration = _controllerContext.GetCurrentGeneration();
                }
            }
            finally
            {
                Engine.Return(ref framePtr);
            }
        }

        /// <summary>
        /// 将已解码帧写入输出通道（快速路径 TryWrite，失败则等待可写）。
        /// <para>注意：若最终写入失败必须释放 <see cref="FFmpegFrame"/>（以释放持有的 <see cref="ByteBlock"/>）。</para>
        /// </summary>
        private void EnqueueFrame(FFmpegFrame frame, CancellationToken token)
        {
            if (_frameChannel.Writer.TryWrite(frame))
            {
                return;
            }

            try
            {
                SlowEnqueueFrame(frame, token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // Stop/Dispose 的正常退出路径：吞掉取消异常（SlowEnqueueFrame 已负责释放 frame）
            }
            catch (ChannelClosedException)
            {
                // 通道已完成/关闭：吞掉（SlowEnqueueFrame 已负责释放 frame）
            }
        }

        /// <summary>
        /// 慢路径写帧：等待通道可写后写入。
        /// <para>失败时释放帧以避免 <see cref="ByteBlock"/> 泄漏。</para>
        /// </summary>
        private async ValueTask SlowEnqueueFrame(FFmpegFrame frame, CancellationToken token)
        {
            try
            {
                while (await _frameChannel.Writer.WaitToWriteAsync(token).ConfigureAwait(false))
                {
                    if (_frameChannel.Writer.TryWrite(frame))
                    {
                        return;
                    }
                }

                // 通道已完成：不能再写，释放 frame
                frame.Dispose();
            }
            catch (ChannelClosedException)
            {
                // 通道已关闭：释放 frame，按正常结束处理
                frame.Dispose();
            }
            catch
            {
                frame.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public bool TryDequeueFrame(out FFmpegFrame frame)
        {
            return _frameChannel.Reader.TryRead(out frame);
        }

        /// <inheritdoc/>
        public bool TryPeekFrame(out FFmpegFrame frame)
        {
            return _frameChannel.Reader.TryPeek(out frame);
        }

        /// <summary>
        /// Flush 内部实现：
        /// <list type="number">
        /// <item><description>先 <see cref="Clear"/> 清空通道并归还/释放底层对象。</description></item>
        /// <item><description>再 flush codec（ <see cref="FFmpegEngine.Flush"/> ）以重置 FFmpeg 解码器内部状态。</description></item>
        /// </list>
        /// </summary>
        private void FlushPrivate(int generation)
        {
            Clear(generation);

            // Flush 后重新开始时间戳序列（后续 PTS fixup 从头计算）
            lastFramePtsMs = FFmpegEngine.InvalidTimestamp;

            var codecContext = DecoderContext.CodecContext;
            Engine.Flush(ref codecContext);
        }

        /// <summary>
        /// 清空 packet 与 frame 通道中的所有数据，并释放/归还底层资源。
        /// </summary>
        /// <param name="generation">目标代际。</param>
        private void Clear(int generation = -1)
        {
            while (_packetChannel.Reader.TryPeek(out var packet))
            {
                if (generation == packet.Generation ||
                    !_packetChannel.Reader.TryRead(out packet))
                {
                    break;
                }
                Engine.Return(ref packet);
            }

            while (_frameChannel.Reader.TryRead(out var frame))
            {
                if (generation == frame.Generation ||
                    !_frameChannel.Reader.TryRead(out frame))
                {
                    break;
                }
                frame.Dispose();
            }
        }

        /// <summary>
        /// 更新解码器设置（由具体解码器实现，可能涉及重采样/缩放/滤镜等）。
        /// </summary>
        public virtual void UpdateSettings(FFmpegDecoderSettings settings)
        {
        }

        /// <summary>
        /// 获取当前帧的持续时间（毫秒）。
        /// <para>
        /// 获取策略：
        /// <list type="number">
        /// <item>
        /// <description>
        /// 优先使用 FFmpeg 在帧上提供的持续时间（通常来自 <see cref="AVFrame.duration"/>），
        /// 并通过 <see cref="FFmpegEngine.GetFrameDuration(NativeIntPtr{AVFrame}, NativeIntPtr{AVStream})"/> 按流的 <c>time_base</c> 换算为毫秒。
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// 当 FFmpeg 未提供（或换算后结果 ≤ 0）时，回退到 <see cref="GetFrameDurationMsProtected(NativeIntPtr{AVFrame})"/>，
        /// 由派生解码器按媒体类型进行估算（例如：视频按帧率，音频按 nb_samples/sample_rate）。
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// 该值主要用于为 PTS 修复逻辑（<see cref="FixupFrameTimestampMs(long, long)"/>）提供步进（step），以保证输出时间戳单调递增。
        /// </para>
        /// </summary>
        /// <param name="framePtr">当前解码得到的帧指针。</param>
        /// <returns>帧持续时间（毫秒）。若无法从 FFmpeg 获取，则使用派生类估算结果。</returns>
        private long GetFrameDurationMs(NativeIntPtr<AVFrame> framePtr)
        {
            long durationMs = Engine.GetFrameDuration(framePtr, DecoderContext.CodecStream);
            if (durationMs > 0)
            {
                return durationMs;
            }

            return GetFrameDurationMsProtected(framePtr);
        }

        /// <summary>
        /// 回退路径：当 FFmpeg 未能提供有效的帧持续时间时，由派生类估算持续时间（毫秒）。
        /// <para>
        /// 常见实现建议：
        /// <list type="bullet">
        /// <item><description>视频：根据帧率估算，例如 <c>1000 / fps</c>（fps 可来自流信息或 <see cref="FFmpegInfo.Rate"/>）。</description></item>
        /// <item><description>音频：根据样本数和采样率估算，例如 <c>nb_samples * 1000 / sample_rate</c>。</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// 返回值应尽量保证为正数；若返回 ≤ 0，上层 PTS 修复逻辑会退化为以 1ms 作为最小步进。
        /// </para>
        /// </summary>
        /// <param name="framePtr">当前解码得到的帧指针。</param>
        /// <returns>估算得到的帧持续时间（毫秒）。</returns>
        protected abstract long GetFrameDurationMsProtected(NativeIntPtr<AVFrame> framePtr);

        /// <summary>
        /// 获取帧的时间戳（毫秒）。
        /// <para>优先使用 best-effort timestamp，最终统一换算为毫秒。</para>
        /// </summary>
        private long GetFrameTimestampMs(NativeIntPtr<AVFrame> frame)
        {
            return Engine.GetFrameTimestampMs(frame, DecoderContext);
        }

        /// <summary>
        /// PTS 自修复逻辑（基类默认实现）：
        /// <list type="bullet">
        /// <item><description>若 PTS 无效：使用 <c>last + duration</c> 推导；若尚无 last，则从 0 开始。</description></item>
        /// <item><description>若 PTS 回退/重复（pts ≤ last）：强制修正为 <c>last + duration</c>，保证单调递增。</description></item>
        /// </list>
        /// </summary>
        /// <param name="ptsMs">原始 PTS（毫秒）。</param>
        /// <param name="durationMs">估算的帧时长（毫秒）。</param>
        /// <returns>修复后的 PTS（毫秒）。</returns>
        protected virtual long FixupFrameTimestampMs(long ptsMs, long durationMs)
        {
            long step = durationMs > 0 ? durationMs : 1;

            if (ptsMs == FFmpegEngine.InvalidTimestamp)
            {
                if (lastFramePtsMs != FFmpegEngine.InvalidTimestamp)
                {
                    ptsMs = lastFramePtsMs + step;
                }
                else
                {
                    ptsMs = 0;
                }
            }
            else if (lastFramePtsMs != FFmpegEngine.InvalidTimestamp && ptsMs <= lastFramePtsMs)
            {
                // 修正：禁止时间戳回退/重复
                ptsMs = lastFramePtsMs + step;
            }

            lastFramePtsMs = ptsMs;
            return ptsMs;
        }

        /// <summary>
        /// 将解码得到的原始 <see cref="AVFrame"/> 转换为输出帧数据（视频像素/音频 PCM）。
        /// </summary>
        /// <param name="frame">解码得到的 <see cref="AVFrame"/>。</param>
        /// <param name="block">输出的帧数据块。</param>
        protected abstract void ProcessFrame(NativeIntPtr<AVFrame> frame, out ByteBlock block);

        /// <summary>
        /// 释放托管资源：清空缓存并完成通道写入端。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            Clear();
            _packetChannel.Writer.TryComplete();
            _frameChannel.Writer.TryComplete();
        }
    }
}