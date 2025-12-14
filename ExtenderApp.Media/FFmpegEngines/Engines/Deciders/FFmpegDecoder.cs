using System.Collections.Concurrent;
using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器抽象基类。 封装解码器的通用属性和生命周期管理，具体解码逻辑由子类实现。
    /// </summary>
    public abstract class FFmpegDecoder : DisposableObject
    {
        /// <summary>
        /// 存储待解码数据包的队列。
        /// </summary>
        private readonly ConcurrentQueue<NativeIntPtr<AVPacket>> _packets;

        /// <summary>
        /// 存储已解码并处理完成的帧的队列。
        /// </summary>
        private readonly ConcurrentQueue<FFmpegFrame> _frames;

        /// <summary>
        /// 缓存状态控制器，用于管理解码缓存和控制生产/消费节奏。
        /// </summary>
        private readonly CacheStateController _cacheStateController;

        private readonly int _maxCacheLength;

        /// <summary>
        /// 获取 FFmpeg 引擎实例，用于底层解码操作和资源管理。
        /// </summary>
        protected FFmpegEngine Engine { get; }

        /// <summary>
        /// 获取解码器上下文，包含解码器指针、流参数等核心信息。
        /// </summary>
        protected FFmpegDecoderContext Context { get; }

        /// <summary>
        /// 获取媒体基础信息（如时长、格式等），便于界面展示和业务逻辑处理。
        /// </summary>
        protected FFmpegInfo Info { get; }

        /// <summary>
        /// 获取或设置解码器设置，用于配置解码器行为和参数。
        /// </summary>
        protected FFmpegDecoderSettings Settings { get; private set; }

        /// <summary>
        /// 获取一个值，该值指示缓存队列是否还有空间。
        /// </summary>
        public bool HasPacketCacheSpace => _packets.Count == 0;

        /// <summary>
        /// 获取一个值，该值指示帧缓存队列是否还有空间。
        /// </summary>
        public bool HasFrameCacheSpace => _cacheStateController.HasCacheSpace;

        /// <summary>
        /// 获取当前已缓存的帧数。
        /// </summary>
        public int CachedFrameCount => _frames.Count;

        /// <summary>
        /// 获取当前解码器对应的流索引。
        /// </summary>
        public int StreamIndex => Context.StreamIndex;

        /// <summary>
        /// 获取一个值，该值指示是否有已解码的帧可供消费。
        /// </summary>
        public bool HasFrames => !_frames.IsEmpty;

        /// <summary>
        /// 获取下一帧的时间戳（以毫秒为单位）。
        /// </summary>
        public long NextFramePts => _frames.TryPeek(out var frame) ? frame.Pts : long.MinValue;

        /// <summary>
        /// 获取当前解码器对应的媒体类型（音频或视频）。
        /// </summary>
        public FFmpegMediaType MediaType => Context.MediaType;

        /// <summary>
        /// 初始化 <see cref="FFmpegDecoder"/> 类的新实例。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="context">解码器上下文。</param>
        /// <param name="info">媒体基础信息。</param>
        /// <param name="settings">解码器设置。</param>
        /// <param name="maxCacheLength">已解码帧的最大缓存数量。</param>
        public FFmpegDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, FFmpegDecoderSettings settings, int maxCacheLength)
        {
            Settings = settings;
            Engine = engine;
            Context = context;
            Info = info;
            _cacheStateController = new(maxCacheLength);
            _packets = new();
            _frames = new();
            _maxCacheLength = maxCacheLength;
        }

        /// <summary>
        /// 从队列中取出一个数据包并进行解码。 如果缓存已满，此方法将异步等待；否则将同步处理并立即返回。
        /// </summary>
        /// <param name="canWait">是否需要等待缓存</param>
        /// <param name="token">用于取消操作的取消令牌。</param>
        public void ProcessPacket(bool canWait, CancellationToken token)
        {
            if (token.IsCancellationRequested || _packets.IsEmpty)
                return;

            while (true)
            {
                // 如果缓存已满，则先阻塞检查
                if (!_cacheStateController.HasCacheSpace)
                {
                    if (!canWait)
                        return;
                    _cacheStateController.WaitForCacheSpace(cancellationToken: token);
                    if (token.IsCancellationRequested)
                        return;
                }

                if (!_packets.TryDequeue(out var packet))
                    return;

                int result = Engine.SendPacket(Context.CodecContext, ref packet);
                if (result < 0 || token.IsCancellationRequested)
                {
                    Engine.Return(ref packet);
                    return;
                }

                ProcessFrames(token);
                Engine.Return(ref packet);
            }
        }

        /// <summary>
        /// 同步循环，从解码器接收并处理所有可用的帧，直到没有更多帧或缓存已满。
        /// </summary>
        /// <param name="token">用于取消操作的取消令牌。</param>
        private void ProcessFrames(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            var frame = Engine.GetFrame();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int ret = Engine.ReceiveFrame(Context.CodecContext, ref frame);

                    // 如果需要重试或没有更多帧，则退出循环
                    if (token.IsCancellationRequested || Engine.IsTryAgain(ret) || Engine.IsEndOfFile(ret))
                    {
                        break;
                    }
                    else if (!Engine.IsSuccess(ret))
                    {
                        // 检查是否已完成
                        Engine.ShowException("接收帧失败", ret);
                        break;
                    }

                    ProcessFrame(frame, out var block);

                    long framePts = Engine.GetFrameTimestampMs(frame, Context);
                    EnqueueFrame(new FFmpegFrame(block, framePts));
                }
            }
            finally
            {
                Engine.Return(ref frame);
            }
        }

        /// <summary>
        /// 将一个待解码的数据包加入队列。
        /// </summary>
        /// <param name="packet">待解码的数据包指针。</param>
        public void EnqueuePacket(NativeIntPtr<AVPacket> packet)
        {
            _packets.Enqueue(packet);
        }

        /// <summary>
        /// 将一个已处理的帧加入到输出队列，并更新缓存计数。
        /// </summary>
        /// <param name="frame">要入队的帧。</param>
        private void EnqueueFrame(FFmpegFrame frame)
        {
            _frames.Enqueue(frame);
            _cacheStateController.OnFrameAdded();
        }

        /// <summary>
        /// 尝试从输出队列中取出一个已解码的帧。
        /// </summary>
        /// <param name="frame">当此方法返回时，如果成功，则包含取出的帧；否则为默认值。</param>
        /// <returns>如果成功取出一个帧，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool TryDequeueFrame(out FFmpegFrame frame)
        {
            if (_frames.TryDequeue(out frame))
            {
                _cacheStateController.OnFrameRemoved();
                return true;
            }
            frame = default;
            return false;
        }

        /// <summary>
        /// 刷新解码器，清空所有内部队列和缓存状态。
        /// </summary>
        public void Flush()
        {
            Clear();
            _cacheStateController.Reset();
        }

        /// <summary>
        /// 清空内部的数据包和帧队列，并释放相关资源。
        /// </summary>
        private void Clear()
        {
            while (_packets.TryDequeue(out var packet))
            {
                Engine.Return(ref packet);
            }
            while (_frames.TryDequeue(out var frame))
            {
                frame.Dispose();
            }
        }

        /// <summary>
        /// 更新解码器设置。 允许在运行时动态更改解码参数，例如输出格式。
        /// </summary>
        /// <param name="settings">新的解码器设置。</param>
        public void UpdateSettings(FFmpegDecoderSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// 处理解码后的帧。子类应实现此方法以处理具体的帧数据，例如进行格式转换、渲染或缓存。
        /// </summary>
        /// <param name="frame">解码后的 AVFrame 指针。</param>
        /// <param name="block">输出参数，包含处理后的帧数据。</param>
        protected abstract void ProcessFrame(NativeIntPtr<AVFrame> frame, out ByteBlock block);

        /// <summary>
        /// 释放由 <see cref="FFmpegDecoder"/> 占用的托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            Clear();
            _cacheStateController.Dispose();
        }
    }
}