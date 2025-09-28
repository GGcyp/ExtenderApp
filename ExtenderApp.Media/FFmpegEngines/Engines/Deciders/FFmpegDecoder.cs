using ExtenderApp.Common;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码器抽象基类。
    /// 封装解码器的通用属性和生命周期管理，具体解码逻辑由子类实现。
    /// </summary>
    public abstract class FFmpegDecoder : DisposableObject
    {
        private const int WaitCacheTimeout = 10;

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
        /// 获取用于取消解码操作的取消令牌。
        /// </summary>
        protected CancellationToken AllToken { get; }

        /// <summary>
        /// 缓存状态控制器，用于管理解码缓存和节奏。
        /// </summary>
        public CacheStateController CacheStateController { get; }

        /// <summary>
        /// 当前解码操作的取消令牌源。
        /// </summary>
        public CancellationTokenSource Source;

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
        public FFmpegDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, CancellationToken allToken, FFmpegDecoderSettings settings, int maxCacheLength)
        {
            Settings = settings;
            Engine = engine;
            Context = context;
            CacheStateController = new(maxCacheLength);
            Info = info;
            AllToken = allToken;
            Source = CancellationTokenSource.CreateLinkedTokenSource(AllToken);
        }

        /// <summary>
        /// 解码指定的数据包（AVPacket），具体解码逻辑由子类实现。
        /// </summary>
        /// <param name="packet">待解码的数据包指针。</param>
        public void Decoding(NativeIntPtr<AVPacket> packet)
        {
            Source = Source ?? CancellationTokenSource.CreateLinkedTokenSource(AllToken);
            int result = Engine.SendPacket(Context.CodecContext, ref packet);
            if (result < 0)
            {
                return;
            }

            while (!AllToken.IsCancellationRequested && !Source.IsCancellationRequested)
            {
                if (!CacheStateController.WaitForCacheSpace(AllToken, WaitCacheTimeout))
                {
                    continue;
                }

                NativeIntPtr<AVFrame> frame = Engine.GetFrame();
                int ret = Engine.ReceiveFrame(Context.CodecContext, ref frame);
                if (Engine.IsTryAgain(ret) || !Engine.CheckResult(ret))
                {
                    Engine.ReturnFrame(ref frame);
                    break;
                }
                long framePts = Engine.GetFrameTimestampMs(frame, Context);
                ProtectedDecoding(frame, framePts);
                Engine.ReturnFrame(ref frame);
            }
        }

        /// <summary>
        /// 根据解码器设置和媒体信息，计算视频帧的行跨度（Stride，单位：字节）。
        /// 行跨度用于表示一行像素在内存中的实际字节数，常用于图像处理和视频帧数据读取。
        /// </summary>
        /// <returns>视频帧的行跨度（字节数）。</returns>
        protected int GetStride()
        {
            return FFmpegEngine.GetStride(Settings, Info);
        }

        protected abstract void ProtectedDecoding(NativeIntPtr<AVFrame> frame, long framePts);

        public void UpdateSettings(FFmpegDecoderSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// 释放解码器相关资源。
        /// </summary>
        /// <param name="disposing">指示是否由 Dispose 方法调用。</param>
        protected override void Dispose(bool disposing)
        {
            CacheStateController.Dispose();
        }
    }
}