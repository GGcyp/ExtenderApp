using ExtenderApp.Contracts;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 视频解码器。
    /// <para>
    /// 职责：
    /// <list type="bullet">
    /// <item><description>从基类 <see cref="FFmpegDecoder"/> 获取解码后的原始 <see cref="AVFrame"/>（通常为源像素格式，如 YUV）。</description></item>
    /// <item><description>通过 <see cref="SwsContext"/> 将原始帧转换为目标像素格式（例如 RGB/BGR）。</description></item>
    /// <item><description>将转换后的像素数据写入 <see cref="ByteBlock"/>，形成标准化的视频帧输出。</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 资源管理说明：
    /// <list type="bullet">
    /// <item><description><see cref="swsContext"/>：由 <see cref="FFmpegEngine.CreateSwsContext"/> 创建，需显式释放。</description></item>
    /// <item><description><see cref="rgbFrame"/>：作为转换输出帧的复用缓存，从引擎帧池获取并在结束时归还。</description></item>
    /// <item><description><see cref="rgbBuffer"/>：承载转换后的像素数据（与 <see cref="rgbFrame"/> 绑定），需释放。</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class FFmpegVideoDecoder : FFmpegDecoder
    {
        /// <summary>
        /// 图像像素格式转换上下文（SwsContext）。
        /// <para>
        /// 用途：在 FFmpeg 的 swscale 模块中负责像素格式转换与缩放（例如 YUV420P -> RGB24）。
        /// </para>
        /// <para>
        /// 生命周期：构造时创建，析构/Dispose 时释放；同一解码器实例内复用以避免频繁创建带来的性能损耗。
        /// </para>
        /// </summary>
        private NativeIntPtr<SwsContext> swsContext;

        /// <summary>
        /// 用于存放转换后像素数据的 RGB 输出帧（<see cref="AVFrame"/>）。
        /// <para>
        /// 这是一个“复用帧”：每次处理输入帧时都会将输出写到该帧所指向的缓冲区，然后拷贝到 <see cref="ByteBlock"/>。
        /// </para>
        /// <para>
        /// 注意：该帧通常不直接下发给外部使用，而是仅作为 swscale 的输出容器。
        /// </para>
        /// </summary>
        private NativeIntPtr<AVFrame> rgbFrame;

        /// <summary>
        /// RGB 图像缓冲区（与 <see cref="rgbFrame"/> 绑定）。
        /// <para>
        /// 存储 swscale 转换后的像素字节数据；随后会被写入新的 <see cref="ByteBlock"/> 以供下游消费。
        /// </para>
        /// </summary>
        private NativeByteMemory rgbBuffer;

        /// <summary>
        /// <see cref="rgbBuffer"/> 的长度（字节）。
        /// <para>
        /// 由目标像素格式、宽高决定，通常通过 FFmpeg 的图像缓冲区大小计算函数得到。
        /// </para>
        /// </summary>
        private int rgbBufferLength;

        /// <summary>
        /// 默认帧持续时间（毫秒）。
        /// </summary>
        private long duration;

        /// <summary>
        /// 创建 <see cref="FFmpegVideoDecoder"/>。
        /// <para>
        /// 初始化一次性资源：
        /// <list type="bullet">
        /// <item><description>从引擎帧池获取 <see cref="rgbFrame"/>。</description></item>
        /// <item><description>根据目标像素格式与分辨率计算缓冲区大小，并创建/绑定 <see cref="rgbBuffer"/>。</description></item>
        /// <item><description>创建 <see cref="swsContext"/>，用于后续每帧转换。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="context">解码器上下文（包含 <see cref="AVCodecContext"/> / <see cref="AVStream"/> 等指针）。</param>
        /// <param name="info">媒体信息（宽高、源像素格式等）。</param>
        /// <param name="controllerContext">控制器上下文（Engine + generation）。</param>
        /// <param name="settings">视频解码设置（目标像素格式、缓存大小等）。</param>
        public FFmpegVideoDecoder(FFmpegDecoderContext context, FFmpegDecoderControllerContext controllerContext, FFmpegDecoderSettings settings)
            : base(context, controllerContext, settings.VideoPacketCacheCount, settings.VideoFrameCacheCount)
        {
            var info = Info;

            // 分配 RGB 输出帧和缓冲区：
            // - rgbFrame：用于 swscale 输出
            // - rgbBuffer：绑定到 rgbFrame 的 data/linesize，承载转换后的像素数据
            rgbFrame = Engine.GetFrame();
            rgbBufferLength = Engine.GetBufferSizeForImage(settings.PixelFormat.Convert(), info.Width, info.Height);
            rgbBuffer = Engine.CreateRGBBuffer(ref rgbFrame, rgbBufferLength, settings.PixelFormat.Convert(), info.Width, info.Height);

            // 创建像素格式转换上下文：
            // 源：info.PixelFormat / info.Width / info.Height
            // 目标：settings.PixelFormat / info.Width / info.Height
            swsContext = Engine.CreateSwsContext(info.Width, info.Height, info.PixelFormat.Convert(), info.Width, info.Height, settings.PixelFormat.Convert());

            double fps = Info.Rate;
            if (fps <= 0)
            {
                fps = FFmpegEngine.DefaultFrameRate;
            }

            long estimated = (long)System.Math.Round(1000d / fps);
            duration = estimated > 0 ? estimated : 1;
        }

        /// <summary>
        /// 将输入的原始帧转换为目标像素格式，并输出为 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="frame">解码得到的原始帧（通常为 YUV）。</param>
        /// <param name="block">输出的像素数据块（每帧分配一个新的块，便于下游异步消费）。</param>
        /// <remarks>
        /// 性能/内存权衡：
        /// <list type="bullet">
        /// <item><description>本实现每帧分配一个新的 <see cref="ByteBlock"/>，减少共享缓冲导致的并发/生命周期问题。</description></item>
        /// <item><description>若需要更高性能，可考虑引入 <see cref="ByteBlock"/> 池化，但必须严格保证下游消费完成后才能复用。</description></item>
        /// </list>
        /// </remarks>
        protected override unsafe void ProcessFrame(NativeIntPtr<AVFrame> frame, out ByteBlock block)
        {
            // 使用 swscale 将 frame 转换到 rgbFrame（输出写入 rgbBuffer 所在内存）
            Engine.Scale(swsContext, frame, rgbFrame, Info);

            // 拷贝输出像素数据到托管可控的块中，交给下游帧队列
            block = new(rgbBufferLength);
            block.Write(rgbBuffer);
        }

        /// <summary>
        /// 释放非托管资源。
        /// <para>
        /// 顺序说明：
        /// <list type="bullet">
        /// <item><description>先释放 swscale 上下文（<see cref="swsContext"/>）。</description></item>
        /// <item><description>归还 <see cref="rgbFrame"/> 到引擎帧池。</description></item>
        /// <item><description>释放 <see cref="rgbBuffer"/>。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            base.DisposeUnmanagedResources();
            Engine.Free(ref swsContext);
            Engine.Return(ref rgbFrame);
            rgbBuffer.Dispose();
        }

        protected override long GetFrameDurationMsProtected(NativeIntPtr<AVFrame> framePtr)
        {
            return duration;
        }
    }
}