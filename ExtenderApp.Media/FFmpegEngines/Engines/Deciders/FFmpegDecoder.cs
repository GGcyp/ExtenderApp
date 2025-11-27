using System.Collections.Concurrent;
using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 解码器抽象基类。
    /// 封装解码器的通用属性和生命周期管理，具体解码逻辑由子类实现。
    /// </summary>
    public abstract class FFmpegDecoder : DisposableObject
    {
        /// <summary>
        /// 当前解码器的数据包队列。
        /// </summary>
        private readonly ConcurrentQueue<NativeIntPtr<AVPacket>> _packets;

        /// <summary>
        /// FFmpeg 引擎实例，用于底层解码操作和资源管理。
        /// </summary>
        protected FFmpegEngine Engine { get; }

        /// <summary>
        /// 解码器上下文，包含解码器指针、流参数等信息。
        /// </summary>
        protected FFmpegDecoderContext Context { get; }

        /// <summary>
        /// 媒体基础信息（如时长、格式等），便于界面展示和业务逻辑处理。
        /// </summary>
        protected FFmpegInfo Info { get; }

        /// <summary>
        /// 解析器设置，用于配置解码器行为和参数。
        /// </summary>
        protected FFmpegDecoderSettings Settings { get; private set; }

        /// <summary>
        /// 缓存状态控制器，用于管理解码缓存和节奏。
        /// </summary>
        public CacheStateController CacheStateController { get; }

        /// <summary>
        /// 当前解码器对应的流索引。
        /// </summary>
        public int StreamIndex => Context.StreamIndex;

        /// <summary>
        /// 初始化 FFmpegDecoder 实例。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="context">解码器上下文。</param>
        /// <param name="info">媒体基础信息。</param>
        /// <param name="maxCacheLength">最大缓存长度。</param>
        /// <param name="settings">解码器设置。</param>
        public FFmpegDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, FFmpegDecoderSettings settings, int maxCacheLength)
        {
            Settings = settings;
            Engine = engine;
            Context = context;
            Info = info;
            CacheStateController = new(maxCacheLength);
            _packets = new();
        }

        /// <summary>
        /// 从队列中取出一个数据包并进行解码。
        /// 如果缓存已满，此方法将异步等待；否则将同步处理并立即返回。
        /// </summary>
        /// <param name="token">用于取消操作的取消令牌。</param>
        public async ValueTask ProcessPacket(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            if (!_packets.TryDequeue(out NativeIntPtr<AVPacket> packet))
                return;

            try
            {
                int result = Engine.SendPacket(Context.CodecContext, ref packet);
                if (result < 0)
                {
                    // 如果发送失败，直接返回，因为不太可能恢复
                    return;
                }

                // 如果缓存已满，则异步等待空间
                if (!CacheStateController.HasCacheSpace)
                {
                    await CacheStateController.WaitForCacheSpaceAsync(cancellationToken: token); // 先尝试非阻塞检查
                }

                // 同步处理所有可用的帧
                ProcessFrames(token);
            }
            finally
            {
                Engine.ReturnPacket(ref packet);
            }
        }

        /// <summary>
        /// 同步循环，从解码器接收并处理所有可用的帧。
        /// </summary>
        private void ProcessFrames(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 检查是否有缓存空间，但这次不阻塞等待
                if (!CacheStateController.HasCacheSpace)
                {
                    break;
                }

                NativeIntPtr<AVFrame> frame = Engine.GetFrame();
                try
                {
                    int ret = Engine.ReceiveFrame(Context.CodecContext, ref frame);

                    // 如果需要重试或没有更多帧，则退出循环
                    if (Engine.IsTryAgain(ret))
                    {
                        break;
                    }
                    // 检查其他错误
                    if (!Engine.IsSuccess(ret))
                    {
                        Engine.ShowException("接收帧失败", ret);
                        break;
                    }

                    long framePts = Engine.GetFrameTimestampMs(frame, Context);
                    ProcessFrame(frame, framePts);
                }
                finally
                {
                    Engine.ReturnFrame(ref frame);
                }
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
        /// 刷新解码器，清空所有未处理的数据包。
        /// </summary>
        public void Flush()
        {
            ClearPackets();
            CacheStateController.Reset();
        }

        private void ClearPackets()
        {
            while (_packets.TryDequeue(out var packet))
            {
                Engine.ReturnPacket(ref packet);
            }
        }

        /// <summary>
        /// 更新解码器设置。
        /// 允许在运行时动态更改解码参数，例如输出格式。
        /// </summary>
        /// <param name="settings">新的解码器设置。</param>
        public void UpdateSettings(FFmpegDecoderSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// 通知缓存控制器已添加一帧。
        /// 调用此方法会增加缓存计数，并可能唤醒等待缓存空间的解码线程。
        /// </summary>
        public void OnFrameAdded()
        {
            CacheStateController.OnFrameAdded();
        }

        /// <summary>
        /// 通知缓存控制器已移除一帧。
        /// 调用此方法会减少缓存计数，并可能在缓存已满时唤醒解码线程以继续解码。
        /// </summary>
        public void OnFrameRemoved()
        {
            CacheStateController.OnFrameRemoved();
        }

        /// <summary>
        /// 处理解码后的帧。子类应实现此方法以处理具体的帧数据，例如进行格式转换、渲染或缓存。
        /// </summary>
        /// <param name="frame">解码后的 AVFrame 指针。</param>
        /// <param name="framePts">帧的显示时间戳（毫秒）。</param>
        protected abstract void ProcessFrame(NativeIntPtr<AVFrame> frame, long framePts);

        protected override void DisposeManagedResources()
        {
            ClearPackets();
            CacheStateController.Dispose();
        }
    }
}