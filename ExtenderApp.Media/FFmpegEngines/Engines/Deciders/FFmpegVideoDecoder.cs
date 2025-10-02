using System.Buffers;
using System.Runtime.InteropServices;
using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 视频解码器。
    /// 负责将解码后的视频帧转换为指定像素格式（如RGB），并输出标准化的视频帧数据。
    /// 支持像素格式转换、缓冲区管理和帧调度，适用于多媒体播放和图像处理场景。
    /// </summary>
    public class FFmpegVideoDecoder : FFmpegDecoder
    {
        /// <summary>
        /// 图像像素格式转换上下文（SwsContext），用于视频帧格式转换和缩放。
        /// </summary>
        private NativeIntPtr<SwsContext> swsContext;

        /// <summary>
        /// 用于存放转换后像素数据的 RGB 帧（AVFrame）。
        /// </summary>
        private NativeIntPtr<AVFrame> rgbFrame;

        /// <summary>
        /// RGB 图像缓冲区指针，存储转换后的视频帧字节数据。
        /// </summary>
        private NativeIntPtr<byte> rgbBuffer;

        /// <summary>
        /// RGB 图像缓冲区长度（字节）。
        /// </summary>
        private int rgbBufferLength;

        /// <summary>
        /// 初始化 FFmpegVideoDecoder 实例，分配帧、缓冲区并创建像素格式转换上下文。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="context">视频解码器上下文。</param>
        /// <param name="info">媒体基础信息。</param>
        /// <param name="allToken">全局取消令牌。</param>
        /// <param name="settings">解码器设置参数。</param>
        public FFmpegVideoDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, FFmpegDecoderSettings settings)
            : base(engine, context, info, settings, settings.VideoMaxCacheLength)
        {
            // 分配 RGB 帧和缓冲区
            rgbFrame = engine.CreateFrame();
            rgbBufferLength = engine.GetBufferSizeForImage(settings.PixelFormat.Convert(), info.Width, info.Height);
            rgbBuffer = engine.CreateRGBBuffer(ref rgbFrame, rgbBufferLength, settings.PixelFormat.Convert(), info.Width, info.Height);
            // 创建像素格式转换上下文
            swsContext = engine.CreateSwsContext(info.Width, info.Height, info.PixelFormat.Convert(), info.Width, info.Height, settings.PixelFormat.Convert());
        }

        /// <summary>
        /// 解码并转换视频帧，将其转换为目标像素格式并调度到上层。
        /// </summary>
        /// <param name="frame">输入的原始视频帧。</param>
        /// <param name="framePts">帧的时间戳。</param>
        protected override void ProtectedDecoding(NativeIntPtr<AVFrame> frame, long framePts)
        {
            // 使用 SwsContext 进行像素格式转换，输出到 rgbFrame
            Engine.Scale(swsContext, frame, rgbFrame, Info);
            // 从 native 缓冲区拷贝数据到托管缓冲区
            var buffer = ArrayPool<byte>.Shared.Rent(rgbBufferLength);
            Marshal.Copy(rgbBuffer, buffer, 0, rgbBufferLength);
            // 构造视频帧并调度到上层
            VideoFrame videoFrame = new(buffer, framePts, Info.Width, Info.Height, GetStride());
            Settings.OnVideoScheduling(videoFrame);
        }

        /// <summary>
        /// 释放视频解码器相关资源，包括像素格式转换上下文、帧和缓冲区。
        /// </summary>
        /// <param name="disposing">指示是否由 Dispose 方法调用。</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Engine.Free(ref swsContext);
            Engine.ReturnFrame(ref rgbFrame);
            Marshal.FreeHGlobal(rgbBuffer);
        }
    }
}
