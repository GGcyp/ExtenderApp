using System.Runtime.CompilerServices;
using System.Threading.Channels;
using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器抽象基类。
    /// <para>本类负责：
    /// <list type="bullet">
    /// <item>
    /// <description>通过 <see cref="Channel{T}"/> 维护“待解码包(packet)”与“已解码帧(frame)”的生产/消费队列。</description>
    /// </item>
    /// <item>
    /// <description>在 Seek 场景下基于“代际（generation）”丢弃旧数据，保证跳转后不会继续解码/输出跳转前的包与帧。</description>
    /// </item>
    /// <item>
    /// <description>封装 FFmpeg 的 send/receive 解码工作流： <c>SendPacket</c> + <c>ReceiveFrame</c>。</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>设计说明：
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="_packetChannel"/> 为无界队列，用于容纳“包不连续/突发”的输入；避免 demux 因短期拥塞频繁阻塞。</description>
    /// </item>
    /// <item>
    /// <description><see cref="_frameChannel"/> 为有界队列，用于对输出帧施加背压，防止 UI/音频消费变慢时内存无限增长。</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>线程模型（约定）：
    /// <list type="bullet">
    /// <item>
    /// <description>上游（解复用/控制器）可多线程写入 <see cref="_packetChannel"/>（ <c>SingleWriter=false</c>）。</description>
    /// </item>
    /// <item>
    /// <description>解码线程为单读者（ <c>SingleReader=true</c>），在 <see cref="DecodeLoopAsync"/> 里串行执行 send/receive。</description>
    /// </item>
    /// <item>
    /// <description>输出帧同样为单读者，通常由渲染/播放线程消费。</description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    public abstract class FFmpegDecoder : DisposableObject, IFFmpegDecoder
    {
        /// <summary>
        /// 待解码包通道。
        /// <para>上游（解复用/控制器）写入 <see cref="FFmpegPacket"/>；解码线程从该通道读取并送入 FFmpeg 解码器。</para>
        /// <para>该通道允许“突发写入”，用于适应音视频包可能不均匀/不连续到达的情况。</para>
        /// <para>注意：本类约定“包所有权”在写入成功后转移给解码线程；若写入失败，写入方必须负责归还/释放底层资源。</para>
        /// </summary>
        private readonly Channel<FFmpegPacket> _packetChannel;

        /// <summary>
        /// 已解码帧通道。
        /// <para>解码线程写入 <see cref="FFmpegFrame"/>；下游（播放/渲染）读取并消费帧数据。</para>
        /// <para>该通道为有界队列：用于对输出做背压，避免消费端变慢时无限堆积帧导致内存上涨。</para>
        /// <para>注意： <see cref="FFmpegFrame"/> 持有非托管缓冲（ <see cref="ByteBlock"/>），一旦写入失败必须及时释放。</para>
        /// </summary>
        private readonly Channel<FFmpegFrame> _frameChannel;

        /// <summary>
        /// 控制器上下文：提供代际（generation）与共享的 <see cref="FFmpegEngine"/>。
        /// </summary>
        private readonly FFmpegDecoderControllerContext _controllerContext;

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

        /// <summary>
        /// 当前解码器对应的流索引。
        /// </summary>
        public int StreamIndex => DecoderContext.StreamIndex;

        /// <summary>
        /// 当前解码器对应的媒体类型（音频/视频）。
        /// </summary>
        public FFmpegMediaType MediaType => DecoderContext.MediaType;

        /// <summary>
        /// 初始化 <see cref="FFmpegDecoder"/>。
        /// </summary>
        /// <param name="decoderContext">解码器上下文（codec/stream 指针等）。</param>
        /// <param name="controllerContext">控制器上下文（Engine + generation 提供者）。</param>
        /// <param name="packetCacheCount">输入包通道（ <see cref="_packetChannel"/>）的最大缓存数量。</param>
        /// <param name="frameCacheCount">输出帧通道（ <see cref="_frameChannel"/>）的最大缓存数量。</param>
        public FFmpegDecoder(FFmpegDecoderContext decoderContext, FFmpegDecoderControllerContext controllerContext, int packetCacheCount, int frameCacheCount)
        {
            _controllerContext = controllerContext;
            DecoderContext = decoderContext;

            _packetChannel = CreateChannel<FFmpegPacket>(packetCacheCount);
            _frameChannel = CreateChannel<FFmpegFrame>(frameCacheCount);
        }

        /// <summary>
        /// 创建一个指定容量的有界通道。
        /// <para>选项说明：
        /// <list type="bullet">
        /// <item>
        /// <description><see cref="BoundedChannelFullMode.Wait"/>：满时等待，提供背压。</description>
        /// </item>
        /// <item>
        /// <description><c>SingleReader=true</c>：解码线程单读（提高 channel 内部性能）。</description>
        /// </item>
        /// <item>
        /// <description><c>SingleWriter=false</c>：允许上游多线程投递。</description>
        /// </item>
        /// </list>
        /// </para>
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
        /// <para>优先走快速路径 <see cref="ChannelWriter{T}.TryWrite(T)"/>，失败后进入慢速路径等待可写。</para>
        /// <para>异常/关闭处理说明：
        /// <list type="bullet">
        /// <item>
        /// <description>取消或通道关闭时：吞掉异常作为正常退出路径；资源释放由慢速路径负责。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="packet">待解码数据包（含 <see cref="FFmpegPacket.Generation"/> 与 <see cref="FFmpegPacket.PacketPtr"/>）。</param>
        /// <param name="token">取消令牌。</param>
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
        /// 低速路径：当 <see cref="_packetChannel"/> 暂时不可写（队列已满）时，等待通道变为可写后再写入。
        /// </summary>
        /// <remarks>
        /// 资源管理约定：
        /// <list type="bullet">
        /// <item>
        /// <description>写入成功：包的所有权移交给解码线程（由 <see cref="ProcessOnePacket"/> 的 finally 归还）。</description>
        /// </item>
        /// <item>
        /// <description>写入失败或等待过程中发生异常：本方法负责归还包并重新抛出异常。</description>
        /// </item>
        /// </list>
        /// </remarks>
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
                // 写入失败：归还底层 AVPacket（避免泄漏/池占用）
                Engine.Return(packet);
                throw;
            }
        }

        /// <summary>
        /// 解码线程主循环：持续从 <see cref="_packetChannel"/> 读取 packet，并解码输出到 <see cref="_frameChannel"/>。
        /// </summary>
        /// <remarks>
        /// Seek/跳转处理（代际）：
        /// <list type="bullet">
        /// <item>
        /// <description>每次循环读取当前代际号（由控制器维护）。</description>
        /// </item>
        /// <item>
        /// <description>当代际变化：调用 <see cref="FlushPrivate"/> 丢弃堆积队列并 flush codec。</description>
        /// </item>
        /// <item>
        /// <description>当 packet.Generation 与当前代际不一致：视为旧代数据，直接丢弃并归还包。</description>
        /// </item>
        /// </list>
        /// </remarks>
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
                catch (ChannelClosedException)
                {
                    return;
                }

                // 代际检查：Seek 后旧代数据直接丢弃
                int currentGeneration = _controllerContext.GetCurrentGeneration();
                if (localGeneration != currentGeneration)
                {
                    localGeneration = currentGeneration;
                    FlushPrivate();
                }

                // 若包属于旧代，直接丢弃（并归还底层 AVPacket）
                if (packet.Generation != localGeneration)
                {
                    Engine.Return(packet);
                    continue;
                }

                ProcessOnePacket(packet, token);
            }
        }

        /// <summary>
        /// 将一个 <see cref="AVPacket"/> 送入 FFmpeg 解码器（send），并尽可能接收输出帧（receive）。
        /// </summary>
        /// <remarks>
        /// 资源回收：
        /// <list type="bullet">
        /// <item>
        /// <description>无论 send 成功与否，最终都会在 finally 中归还 packet。</description>
        /// </item>
        /// </list>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessOnePacket(FFmpegPacket packet, CancellationToken token)
        {
            try
            {
                NativeIntPtr<AVPacket> pktPtr = packet.PacketPtr;

                // 注意：SendPacket 可能会根据 FFmpeg 约定修改/消费 pktPtr（因此使用 ref）
                int result = Engine.SendPacket(DecoderContext, ref pktPtr);
                if (result < 0 || token.IsCancellationRequested)
                {
                    return;
                }

                ProcessFrames(packet.Generation, token);
            }
            finally
            {
                // 归还 packet（无论是否被成功 send）
                Engine.Return(packet);
            }
        }

        /// <summary>
        /// 从 FFmpeg 解码器中循环接收已解码帧，并转换为输出 <see cref="FFmpegFrame"/> 写入帧通道。
        /// </summary>
        /// <remarks>
        /// 退出条件（任一满足则退出）：
        /// <list type="bullet">
        /// <item>
        /// <description>取消请求。</description>
        /// </item>
        /// <item>
        /// <description>EAGAIN：需要更多输入包。</description>
        /// </item>
        /// <item>
        /// <description>EOF：解码结束。</description>
        /// </item>
        /// <item>
        /// <description>其他错误：记录并退出。</description>
        /// </item>
        /// </list>
        /// </remarks>
        private void ProcessFrames(int packetGeneration, CancellationToken token)
        {
            var framePtr = Engine.GetFrame();
            int currentGeneration = _controllerContext.GetCurrentGeneration();

            try
            {
                // 注意：这里的循环逻辑意图是“取消后尽快退出”以及“若代际变化则终止旧代输出”
                while (!token.IsCancellationRequested ||
                    currentGeneration != packetGeneration)
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

                    long framePts = GetFrameTimestampMs(framePtr);
                    // 若帧时间戳无效，跳过该帧
                    if (framePts == FFmpegEngine.InvalidTimestamp)
                        continue;

                    ProcessFrame(framePtr, out var block);

                    FFmpegFrame frame = new(currentGeneration, block, framePts);
                    EnqueueFrame(frame, token);

                    currentGeneration = _controllerContext.GetCurrentGeneration();
                }
            }
            finally
            {
                // 归还 frame（池化/复用），避免泄漏
                Engine.Return(framePtr);
            }
        }

        /// <summary>
        /// 将已解码帧写入输出通道。
        /// <para>快速路径 TryWrite，失败则进入慢速路径等待可写。</para>
        /// <para>注意：若最终写入失败必须释放 <see cref="FFmpegFrame"/>（以释放 <see cref="ByteBlock"/>）。</para>
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
        /// 低速路径：等待帧通道可写后再写入。
        /// </summary>
        /// <remarks>
        /// 资源管理：
        /// <list type="bullet">
        /// <item>
        /// <description>写入成功：帧所有权移交给消费端（由消费端在取出后 Dispose）。</description>
        /// </item>
        /// <item>
        /// <description>写入失败/取消/异常：释放帧（避免 <see cref="ByteBlock"/> 泄漏）。</description>
        /// </item>
        /// </list>
        /// </remarks>
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
        /// 尝试预览下一个已解码的帧，但不从队列中移除它。
        /// </summary>
        /// <param name="frame">预览的已解码帧</param>
        /// <returns>取出成功返回 true；否则 false。</returns>
        public bool TryPeekFrame(out FFmpegFrame frame)
        {
            return _frameChannel.Reader.TryPeek(out frame);
        }

        /// <summary>
        /// 清空解码器内部缓存（packet/frame）并 flush FFmpeg codec 状态。
        /// <para>典型用途：Seek/Stop 时快速清理堆积数据，并重置解码器内部缓冲区。</para>
        /// </summary>
        internal void FlushInternal()
        {
            FlushPrivate();
        }

        /// <summary>
        /// <see cref="FlushInternal"/> 的内部实现。
        /// <para>顺序说明：
        /// <list type="number">
        /// <item>
        /// <description><see cref="Clear"/>：清空队列并归还/释放底层对象。</description>
        /// </item>
        /// <item>
        /// <description>Flush codec：调用 <see cref="FFmpegEngine.Flush"/> 重置解码器内部缓冲。</description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>重要：codec flush 必须在“确定不会再并发使用该 <see cref="AVCodecContext"/>”的前提下执行，否则可能触发 native 崩溃。</para>
        /// </summary>
        private void FlushPrivate()
        {
            Clear();

            // 清空 codec 内部缓冲（Seek 后必须做） 说明：这里使用局部变量 + ref，依赖 Engine.Flush 的签名（若 Engine.Flush 不是 ref，则不会影响原始句柄）
            var codecContext = DecoderContext.CodecContext;
            Engine.Flush(ref codecContext);
        }

        /// <summary>
        /// 清空 packet 与 frame 通道中的所有数据，并释放/归还底层资源。
        /// </summary>
        /// <remarks>
        /// 清理策略：
        /// <list type="bullet">
        /// <item>
        /// <description>packet：归还到底层池（ <see cref="FFmpegEngine.Return(FFmpegPacket)"/>）。</description>
        /// </item>
        /// <item>
        /// <description>frame：调用 <see cref="FFmpegFrame.Dispose"/> 释放托管/非托管帧数据。</description>
        /// </item>
        /// </list>
        /// </remarks>
        private void Clear()
        {
            // 清空 packet（并归还底层 AVPacket）
            while (_packetChannel.Reader.TryRead(out var packet))
            {
                Engine.Return(packet);
            }

            // 清空 frame（并释放托管帧数据）
            while (_frameChannel.Reader.TryRead(out var frame))
            {
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
        /// 获取帧的时间戳（毫秒）。
        /// </summary>
        /// <param name="frame">需要获取时间戳的帧。</param>
        /// <returns>获取到的时间戳。</returns>
        private long GetFrameTimestampMs(NativeIntPtr<AVFrame> frame)
        {
            return Engine.GetFrameTimestampMs(frame, DecoderContext);
        }

        /// <summary>
        /// 将解码得到的原始 <see cref="AVFrame"/> 转换为输出帧数据。
        /// </summary>
        /// <param name="frame">原生帧指针。</param>
        /// <param name="block">输出帧数据（视频像素/音频 PCM）。</param>
        protected abstract void ProcessFrame(NativeIntPtr<AVFrame> frame, out ByteBlock block);

        /// <summary>
        /// 释放托管资源：清空缓存并完成通道写入端。
        /// <para>完成通道后：
        /// <list type="bullet">
        /// <item>
        /// <description>后续写入会失败/抛出，从而触发上游归还资源逻辑。</description>
        /// </item>
        /// <item>
        /// <description>读端在队列耗尽后会收到 <see cref="ChannelClosedException"/>。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        protected override void DisposeManagedResources()
        {
            Clear();
            _packetChannel.Writer.TryComplete();
            _frameChannel.Writer.TryComplete();
        }
    }
}