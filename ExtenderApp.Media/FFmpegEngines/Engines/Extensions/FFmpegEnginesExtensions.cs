using ExtenderApp.Contracts;
using ExtenderApp.FFmpegEngines.Decoders;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpegEngine 扩展方法类，提供便捷的 Seek 操作。 支持对 FFmpegContext、FFmpegDecoderContextCollection、FFmpegDecoderContext 进行统一跳转和缓冲区刷新。 适用于多流同步跳转、单流跳转及解码器状态重置等场景。
    /// </summary>
    public static class FFmpegEnginesExtensions
    {
        /// <summary>
        /// 对整个 FFmpegContext 进行跳转操作。
        /// </summary>
        /// <param name="engine">FFmpegEngine 实例。</param>
        /// <param name="context">格式上下文指针。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="fContext">FFmpegContext，包含解码器上下文集合。</param>
        public static void Seek(this FFmpegEngine engine, NativeIntPtr<AVFormatContext> context, long targetTime, FFmpegContext fContext)
        {
            engine.Seek(context, targetTime, fContext.ContextCollection);
        }

        /// <summary>
        /// 对解码器上下文集合进行跳转操作。
        /// </summary>
        /// <param name="engine">FFmpegEngine 实例。</param>
        /// <param name="context">格式上下文指针。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="dContext">解码器上下文集合。</param>
        public static void Seek(this FFmpegEngine engine, NativeIntPtr<AVFormatContext> context, long targetTime, FFmpegDecoderContextCollection dContext)
        {
            engine.Seek(context, targetTime, dContext[0]);
        }

        /// <summary>
        /// 对单个解码器上下文进行跳转，并刷新解码器缓冲区。
        /// </summary>
        /// <param name="engine">FFmpegEngine 实例。</param>
        /// <param name="context">格式上下文指针。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="dContext">解码器上下文。</param>
        public static void Seek(this FFmpegEngine engine, NativeIntPtr<AVFormatContext> context, long targetTime, FFmpegDecoderContext dContext)
        {
            engine.Seek(context, targetTime, dContext.CodecStream);
        }

        /// <summary>
        /// 将 FFmpegPixelFormat 枚举值转换为 FFmpeg.AutoGen.AVPixelFormat 枚举值。 用于托管与非托管像素格式类型的互转，便于与 FFmpeg API 交互。
        /// </summary>
        /// <param name="format">FFmpegPixelFormat 枚举值。</param>
        /// <returns>对应的 AVPixelFormat 枚举值。</returns>
        public static AVPixelFormat Convert(this FFmpegPixelFormat format)
        {
            return (AVPixelFormat)format;
        }

        /// <summary>
        /// 将 FFmpeg.AutoGen.AVPixelFormat 枚举值转换为 FFmpegPixelFormat 枚举值。 用于非托管与托管像素格式类型的互转，便于业务逻辑处理。
        /// </summary>
        /// <param name="format">AVPixelFormat 枚举值。</param>
        /// <returns>对应的 FFmpegPixelFormat 枚举值。</returns>
        public static FFmpegPixelFormat Convert(this AVPixelFormat format)
        {
            return (FFmpegPixelFormat)format;
        }

        /// <summary>
        /// 将 FFmpegSampleFormat 枚举值转换为 FFmpeg.AutoGen.AVSampleFormat 枚举值。 用于托管与非托管音频采样格式类型的互转，便于与 FFmpeg API 交互。
        /// </summary>
        /// <param name="format">FFmpegSampleFormat 枚举值。</param>
        /// <returns>对应的 AVSampleFormat 枚举值。</returns>
        public static AVSampleFormat Convert(this FFmpegSampleFormat format)
        {
            return (AVSampleFormat)format;
        }

        /// <summary>
        /// 将 FFmpeg.AutoGen.AVSampleFormat 枚举值转换为 FFmpegSampleFormat 枚举值。 用于非托管与托管音频采样格式类型的互转，便于业务逻辑处理。
        /// </summary>
        /// <param name="format">AVSampleFormat 枚举值。</param>
        /// <returns>对应的 FFmpegSampleFormat 枚举值。</returns>
        public static FFmpegSampleFormat Convert(this AVSampleFormat format)
        {
            return (FFmpegSampleFormat)format;
        }

        /// <summary>
        /// 将 FFmpeg.AutoGen.AVMediaType 枚举值转换为 FFmpegMediaType 枚举值。 用于非托管与托管媒体类型枚举的互转，便于业务逻辑处理。
        /// </summary>
        /// <param name="mediaType">AVMediaType 枚举值。</param>
        /// <returns>对应的 FFmpegMediaType 枚举值。</returns>
        public static FFmpegMediaType Convert(this AVMediaType mediaType)
        {
            return (FFmpegMediaType)mediaType;
        }

        /// <summary>
        /// 将 FFmpegMediaType 枚举值转换为 FFmpeg.AutoGen.AVMediaType 枚举值。 用于托管与非托管媒体类型枚举的互转，便于与 FFmpeg API 交互。
        /// </summary>
        /// <param name="mediaType">FFmpegMediaType 枚举值。</param>
        /// <returns>对应的 AVMediaType 枚举值。</returns>
        public static AVMediaType Convert(this FFmpegMediaType mediaType)
        {
            return (AVMediaType)mediaType;
        }

        #region CreateFFmpegDecoderController

        /// <summary>
        /// 创建解码控制器（FFmpegDecoderController）。 用于管理解码流程，包括解码启动、停止、跳转、资源释放等操作。 支持多线程解码、取消令牌控制和解码器集合的统一管理。
        /// </summary>
        /// <param name="engine">FFmpegEngine 实例。</param>
        /// <param name="context">FFmpeg上下文，包含媒体信息和解码器集合。</param>
        /// <param name="settings"></param>
        /// <returns>FFmpegDecoderController 实例。</returns>
        public static IFFmpegDecoderController CreateDecoderController(this FFmpegEngine engine, FFmpegContext context, FFmpegDecoderSettings settings)
        {
            FFmpegDecoderControllerContext controllerContext = new(engine, context);
            var collection = CreateDecoderCollection(context, controllerContext, settings);
            return new FFmpegDecoderController(collection, controllerContext, settings);
        }

        /// <summary>
        /// 创建解码器集合（FFmpegDecoderCollection）。 用于根据上下文和解码设置，自动初始化视频和音频解码器，并管理解码过程中的资源。 支持自定义取消令牌和解码参数，便于多线程解码和参数调整。
        /// </summary>
        /// <param name="context">FFmpeg上下文，包含解码器集合和媒体信息。</param>
        /// <param name="controllerContext">解码器控制器上下文。</param>
        /// <param name="settings">可选的解码器设置，若为空则使用默认设置。</param>
        /// <returns>FFmpegDecoderCollection 实例。</returns>
        internal static FFmpegDecoderCollection CreateDecoderCollection(FFmpegContext context, FFmpegDecoderControllerContext controllerContext, FFmpegDecoderSettings settings)
        {
            if (context.IsEmpty)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var contextCollection = context.ContextCollection;
            var decoders = new FFmpegDecoder[contextCollection.Length];

            for (int i = 0; i < contextCollection.Length; i++)
            {
                var decoderContext = contextCollection[i];
                decoders[i] = FFmpegDecoderFactory.CreateDecoder(decoderContext, controllerContext, settings);
            }

            return new FFmpegDecoderCollection(decoders);
        }

        #endregion CreateFFmpegDecoderController
    }
}