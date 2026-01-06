using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpegEngine 扩展方法类，提供便捷的 Seek 操作。 支持对
    /// FFmpegContext、FFmpegDecoderContextCollection、FFmpegDecoderContext
    /// 进行统一跳转和缓冲区刷新。 适用于多流同步跳转、单流跳转及解码器状态重置等场景。
    /// </summary>
    public static class FFmpegEnginesExpansions
    {
        /// <summary>
        /// 对整个 FFmpegContext 进行跳转操作。
        /// 实际会对其包含的所有解码器上下文集合（视频/音频）执行 Seek。
        /// </summary>
        /// <param name="engine">
        /// FFmpegEngine 实例。
        /// </param>
        /// <param name="context">格式上下文指针。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="fContext">FFmpegContext，包含解码器上下文集合。</param>
        public static void Seek(this FFmpegEngine engine, NativeIntPtr<AVFormatContext> context, long targetTime, FFmpegContext fContext)
        {
            engine.Seek(context, targetTime, fContext.ContextCollection);
        }

        /// <summary>
        /// 对解码器上下文集合（视频/音频）分别进行跳转操作。 可同时跳转音频流和视频流，适合多流同步定位场景。
        /// </summary>
        /// <param name="engine">
        /// FFmpegEngine 实例。
        /// </param>
        /// <param name="context">格式上下文指针。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="dContext">解码器上下文集合。</param>
        public static void Seek(this FFmpegEngine engine, NativeIntPtr<AVFormatContext> context, long targetTime, FFmpegDecoderContextCollection dContext)
        {
            foreach (var c in dContext)
            {
                engine.Seek(context, targetTime, c);
            }
        }

        /// <summary>
        /// 对单个解码器上下文进行跳转，并刷新解码器缓冲区。 跳转后自动调用 FlushAsync，确保解码器状态同步，避免脏数据影响后续解码。
        /// </summary>
        /// <param name="engine">
        /// FFmpegEngine 实例。
        /// </param>
        /// <param name="context">格式上下文指针。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="dContext">解码器上下文。</param>
        public static void Seek(this FFmpegEngine engine, NativeIntPtr<AVFormatContext> context, long targetTime, FFmpegDecoderContext dContext)
        {
            engine.Seek(context, targetTime, dContext.CodecStream);
            engine.Flush(ref dContext.CodecContext);
        }

        /// <summary>
        /// 将 FFmpegPixelFormat 枚举值转换为
        /// FFmpeg.AutoGen.AVPixelFormat 枚举值。
        /// 用于托管与非托管像素格式类型的互转，便于与 FFmpeg API 交互。
        /// </summary>
        /// <param name="format">
        /// FFmpegPixelFormat 枚举值。
        /// </param>
        /// <returns>对应的 AVPixelFormat 枚举值。</returns>
        public static AVPixelFormat Convert(this FFmpegPixelFormat format)
        {
            return (AVPixelFormat)format;
        }

        /// <summary>
        /// 将 FFmpeg.AutoGen.AVPixelFormat 枚举值转换为
        /// FFmpegPixelFormat 枚举值。 用于非托管与托管像素格式类型的互转，便于业务逻辑处理。
        /// </summary>
        /// <param name="format">
        /// AVPixelFormat 枚举值。
        /// </param>
        /// <returns>
        /// 对应的 FFmpegPixelFormat 枚举值。
        /// </returns>
        public static FFmpegPixelFormat Convert(this AVPixelFormat format)
        {
            return (FFmpegPixelFormat)format;
        }

        /// <summary>
        /// 将 FFmpegSampleFormat 枚举值转换为
        /// FFmpeg.AutoGen.AVSampleFormat 枚举值。
        /// 用于托管与非托管音频采样格式类型的互转，便于与 FFmpeg API 交互。
        /// </summary>
        /// <param name="format">
        /// FFmpegSampleFormat 枚举值。
        /// </param>
        /// <returns>对应的 AVSampleFormat 枚举值。</returns>
        public static AVSampleFormat Convert(this FFmpegSampleFormat format)
        {
            return (AVSampleFormat)format;
        }

        /// <summary>
        /// 将 FFmpeg.AutoGen.AVSampleFormat 枚举值转换为
        /// FFmpegSampleFormat 枚举值。 用于非托管与托管音频采样格式类型的互转，便于业务逻辑处理。
        /// </summary>
        /// <param name="format">
        /// AVSampleFormat 枚举值。
        /// </param>
        /// <returns>
        /// 对应的 FFmpegSampleFormat 枚举值。
        /// </returns>
        public static FFmpegSampleFormat Convert(this AVSampleFormat format)
        {
            return (FFmpegSampleFormat)format;
        }

        /// <summary>
        /// 将 FFmpeg.AutoGen.AVMediaType 枚举值转换为
        /// FFmpegMediaType 枚举值。 用于非托管与托管媒体类型枚举的互转，便于业务逻辑处理。
        /// </summary>
        /// <param name="mediaType">
        /// AVMediaType 枚举值。
        /// </param>
        /// <returns>
        /// 对应的 FFmpegMediaType 枚举值。
        /// </returns>
        public static FFmpegMediaType Convert(this AVMediaType mediaType)
        {
            return (FFmpegMediaType)mediaType;
        }

        /// <summary>
        /// 将 FFmpegMediaType 枚举值转换为
        /// FFmpeg.AutoGen.AVMediaType 枚举值。
        /// 用于托管与非托管媒体类型枚举的互转，便于与 FFmpeg API 交互。
        /// </summary>
        /// <param name="mediaType">
        /// FFmpegMediaType 枚举值。
        /// </param>
        /// <returns>对应的 AVMediaType 枚举值。</returns>
        public static AVMediaType Convert(this FFmpegMediaType mediaType)
        {
            return (AVMediaType)mediaType;
        }
    }
}