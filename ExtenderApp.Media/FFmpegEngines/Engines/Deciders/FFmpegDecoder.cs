using System.Runtime.CompilerServices;
using System.Threading.Channels;
using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器抽象基类。
    /// <para>
    /// 本类负责：
    /// <list type="bullet">
    /// <item><description>通过 <see cref="Channel{T}"/> 维护“待解码包(packet)”与“已解码帧(frame)”的生产/消费队列。</description></item>
    /// <item><description>在 Seek 场景下基于“代际（Generation）”丢弃旧数据，保证跳转后不会继续解码/输出跳转前的包与帧。</description></item>
    /// <item><description>封装 FFmpeg 的 send/receive 解码工作流：<c>SendPacket</c> + <c>ReceiveFrame</c>。</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 设计说明：
    /// <list type="bullet">
    /// <item><description><see cref="_packetChannel"/> 为无界队列，用于容纳“包不连续/突发”的输入；避免 demux 因短期拥塞频繁阻塞。</description></item>
    /// <item><description><see cref="_frameChannel"/> 为有界队列，用于对输出帧施加背压，防止 UI/音频消费变慢时内存无限增长。</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public abstract class FFmpegDecoder : DisposableObject
    {
        /// <summary>
        /// 待解码包通道。
        /// <para>
        /// 上游（解复用/控制器）写入 <see cref="FFmpegPacket"/>；解码线程从该通道读取并送入 FFmpeg 解码器。
        /// </para>
        /// <para>
        /// 该通道通常允许“突发写入”，用于适应音视频包可能不均匀/不连续到达的情况。
        /// </para>
        /// </summary>
        private readonly Channel<FFmpegPacket> _packetChannel;

        /// <summary>
        /// 已解码帧通道。
        /// <para>
        /// 解码线程写入 <see cref="FFmpegFrame"/>；下游（播放/渲染）读取并消费帧数据。
        /// </para>
        /// <para>
        /// 该通道为有界队列，用于对输出做背压，避免消费端变慢时无限堆积帧导致内存上涨。
        /// </para>
        /// </summary>
        private readonly Channel<FFmpegFrame> _frameChannel;

        /// <summary>
        /// FFmpeg 引擎实例，用于调用底层 FFmpeg API（send/receive、时间戳换算、资源池回收等）。
        /// </summary>
        protected FFmpegEngine Engine { get; }

        /// <summary>
        /// 解码器上下文，包含流索引、媒体类型以及底层 <see cref="AVCodecContext"/> / <see cref="AVStream"/> 等指针。
        /// </summary>
        protected FFmpegDecoderContext Context { get; }

        /// <summary>
        /// 媒体基础信息（时长、分辨率、帧率等）。
        /// </summary>
        protected FFmpegInfo Info { get; }

        /// <summary>
        /// 解码器设置（输出格式、采样率、以及当前“代际”读取入口等）。
        /// </summary>
        protected FFmpegDecoderSettings Settings { get; private set; }

        /// <summary>
        /// 当前解码器对应的流索引。
        /// </summary>
        public int StreamIndex => Context.StreamIndex;

        /// <summary>
        /// 获取当前是否有已解码帧缓存。
        /// </summary>
        public bool HasFrameCached => _frameChannel.Reader.TryPeek(out _);

        /// <summary>
        /// 获取下一帧的显示时间戳（毫秒）。
        /// <para>
        /// 用于播放调度（按 PTS 进行音视频同步/丢帧/等待）。
        /// 若无可用帧则返回 <see cref="long.MinValue"/>。
        /// </para>
        /// </summary>
        public long NextFramePts => _frameChannel.Reader.TryPeek(out var frame) ? frame.Pts : long.MinValue;

        /// <summary>
        /// 当前解码器对应的媒体类型（音频/视频）。
        /// </summary>
        public FFmpegMediaType MediaType => Context.MediaType;

        /// <summary>
        /// 初始化 <see cref="FFmpegDecoder"/>。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="context">解码器上下文。</param>
        /// <param name="info">媒体基础信息。</param>
        /// <param name="settings">解码器设置。</param>
        /// <param name="maxFrameCacheCount">输出帧通道（<see cref="_frameChannel"/>）的最大缓存数量。</param>
        public FFmpegDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, FFmpegDecoderSettings settings, int maxFrameCacheCount)
        {
            Settings = settings;
            Engine = engine;
            Context = context;
            Info = info;

            _packetChannel = Channel.CreateBounded<FFmpegPacket>(new BoundedChannelOptions(2)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
            });

            _frameChannel = Channel.CreateBounded<FFmpegFrame>(new BoundedChannelOptions(maxFrameCacheCount)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
            });
        }

        /// <summary>
        /// 将一个带代际信息的 packet 投递到解码器队列。
        /// <para>
        /// 上游应在写入时附加当前代际号（通常由控制器在每次 Seek 时递增），以便解码线程在消费时进行校验。
        /// </para>
        /// </summary>
        /// <param name="packet">待解码数据包（包含 <see cref="FFmpegPacket.Generation"/> 与 <see cref="FFmpegPacket.PacketPtr"/>）。</param>
        public void EnqueuePacket(FFmpegPacket packet, CancellationToken token)
        {
            if (_packetChannel.Writer.TryWrite(packet))
            {
                return;
            }

            SlowEnqueuePacket(packet, token).GetAwaiter().GetResult();
        }

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
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                Engine.Return(ref packet);
                throw;
            }
        }

        /// <summary>
        /// 解码线程主循环：持续从 <see cref="_packetChannel"/> 读取 packet，并解码输出到 <see cref="_frameChannel"/>。
        /// <para>
        /// Seek/跳转处理方式：
        /// <list type="bullet">
        /// <item><description>每轮循环会读取当前代际号。</description></item>
        /// <item><description>当代际号变化时，调用 <see cref="FlushPrivate"/> 清空队列并 flush codec 缓冲。</description></item>
        /// <item><description>若 packet 的 <see cref="FFmpegPacket.Generation"/> 与当前代际不一致，则丢弃并归还包资源。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="token">取消令牌。</param>
        public async Task DecodeLoopAsync(CancellationToken token)
        {
            long localGeneration = -1;

            while (!token.IsCancellationRequested)
            {
                FFmpegPacket packet;
                try
                {
                    packet = await _packetChannel.Reader.ReadAsync(token).ConfigureAwait(false);
                }
                catch (ChannelClosedException)
                {
                    return;
                }

                // 代际检查：Seek 后旧代数据直接丢弃
                long currentGeneration = Settings.GetCurrentGeneration();
                if (localGeneration != currentGeneration)
                {
                    localGeneration = currentGeneration;
                    FlushPrivate();
                }

                // 若包属于旧代，直接丢弃（并归还底层 AVPacket）
                if (packet.Generation != localGeneration)
                {
                    Engine.Return(ref packet);
                    continue;
                }

                ProcessOnePacket(packet.PacketPtr, token);
            }
        }

        /// <summary>
        /// 将一个 <see cref="AVPacket"/> 送入 FFmpeg 解码器，并尽可能接收输出帧。
        /// <para>
        /// 包资源在本方法结束时归还（无论成功与否）。
        /// </para>
        /// </summary>
        /// <param name="packet">要送入解码器的原生包指针。</param>
        /// <param name="token">取消令牌。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessOnePacket(NativeIntPtr<AVPacket> packet, CancellationToken token)
        {
            try
            {
                int result = Engine.SendPacket(Context.CodecContext, ref packet);
                if (result < 0 || token.IsCancellationRequested)
                {
                    return;
                }

                ProcessFrames(token);
            }
            finally
            {
                // 归还 packet（无论是否被成功 send）
                Engine.Return(ref packet);
            }
        }

        /// <summary>
        /// 从 FFmpeg 解码器中循环接收已解码帧，并转换为输出 <see cref="FFmpegFrame"/> 写入帧通道。
        /// <para>
        /// 接收循环在遇到 EAGAIN（需要更多输入包）/ EOF / 取消请求 / 错误时退出。
        /// </para>
        /// </summary>
        private void ProcessFrames(CancellationToken token)
        {
            var framePtr = Engine.GetFrame();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int ret = Engine.ReceiveFrame(Context.CodecContext, ref framePtr);

                    if (token.IsCancellationRequested ||
                        Engine.IsTryAgain(ret) ||
                        Engine.IsEndOfFile(ret))
                    {
                        break;
                    }

                    if (!Engine.IsSuccess(ret))
                    {
                        Engine.ShowException("接收帧失败", ret);
                        break;
                    }

                    ProcessFrame(framePtr, out var block);
                    long framePts = Engine.GetFrameTimestampMs(framePtr, Context);

                    var frame = CreateFrame(block, framePts);
                    EnqueueFrame(frame, token);
                }
            }
            finally
            {
                Engine.Return(ref framePtr);
            }
        }

        /// <summary>
        /// 创建一个输出帧对象，并附带当前代际号。
        /// <para>
        /// 下游消费帧时可通过代际号判断帧是否过期（例如 Seek 后丢弃旧帧）。
        /// </para>
        /// </summary>
        private FFmpegFrame CreateFrame(ByteBlock block, long pts)
        {
            return new FFmpegFrame(Settings.GetCurrentGeneration(), block, pts);
        }

        /// <summary>
        /// 将已解码帧写入输出通道。
        /// <para>
        /// 若通道已满，将阻塞等待缓存空间（背压）；失败时确保释放帧资源避免泄漏。
        /// </para>
        /// </summary>
        private void EnqueueFrame(FFmpegFrame frame, CancellationToken token)
        {
            if (_frameChannel.Writer.TryWrite(frame))
            {
                return;
            }

            SlowEnqueueFrame(frame, token).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 低速路径：等待帧通道可写后再写入。
        /// <para>
        /// 注意：写入失败（例如取消/异常）时会释放 <paramref name="frame"/>，避免 <see cref="ByteBlock"/> 泄漏。
        /// </para>
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

                token.ThrowIfCancellationRequested();
            }
            catch
            {
                frame.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 尝试从输出队列中取出一个已解码的帧。
        /// </summary>
        /// <param name="frame">输出帧。</param>
        /// <returns>取出成功返回 true；否则 false。</returns>
        public bool TryDequeueFrame(out FFmpegFrame frame)
        {
            return _frameChannel.Reader.TryRead(out frame);
        }

        /// <summary>
        /// 清空解码器内部缓存（packet/frame）并 flush FFmpeg codec 状态。
        /// <para>
        /// 典型用途：Seek/Stop 时快速清理堆积数据，并重置解码器内部缓冲区。
        /// </para>
        /// </summary>
        internal void FlushInternal()
        {
            FlushPrivate();
        }

        /// <summary>
        /// <see cref="FlushInternal"/> 的内部实现。
        /// </summary>
        private void FlushPrivate()
        {
            // 清空 packet（并归还底层 AVPacket）
            while (_packetChannel.Reader.TryRead(out var packet))
            {
                Engine.Return(ref packet);
            }

            // 清空 frame（并释放托管帧数据）
            while (_frameChannel.Reader.TryRead(out var frame))
            {
                frame.Dispose();
            }

            // 清空 codec 内部缓冲（Seek 后必须做）
            var codecContext = Context.CodecContext;
            Engine.Flush(ref codecContext);
        }

        /// <summary>
        /// 更新解码器设置。
        /// </summary>
        public void UpdateSettings(FFmpegDecoderSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// 将解码得到的原始 <see cref="AVFrame"/> 转换为输出帧数据。
        /// </summary>
        /// <param name="frame">原生帧指针。</param>
        /// <param name="block">输出帧数据（视频像素/音频 PCM）。</param>
        protected abstract void ProcessFrame(NativeIntPtr<AVFrame> frame, out ByteBlock block);

        /// <summary>
        /// 释放托管资源：清空缓存并完成通道写入端。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            FlushInternal();
            _packetChannel.Writer.TryComplete();
            _frameChannel.Writer.TryComplete();
        }
    }
}