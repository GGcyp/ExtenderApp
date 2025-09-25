using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpegEngine 扩展方法类，提供便捷的 Seek 操作。
    /// 支持对 FFmpegContext、FFmpegDecoderContextCollection、FFmpegDecoderContext 进行统一跳转和缓冲区刷新。
    /// 适用于多流同步跳转、单流跳转及解码器状态重置等场景。
    /// </summary>
    public static class FFmpegEnginesExpansions
    {
        /// <summary>
        /// 对整个 FFmpegContext 进行跳转操作。
        /// 实际会对其包含的所有解码器上下文集合（视频/音频）执行 Seek。
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
        /// 对解码器上下文集合（视频/音频）分别进行跳转操作。
        /// 可同时跳转音频流和视频流，适合多流同步定位场景。
        /// </summary>
        /// <param name="engine">FFmpegEngine 实例。</param>
        /// <param name="context">格式上下文指针。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="dContext">解码器上下文集合。</param>
        public static void Seek(this FFmpegEngine engine, NativeIntPtr<AVFormatContext> context, long targetTime, FFmpegDecoderContextCollection dContext)
        {
            engine.Seek(context, targetTime, dContext.AudioContext);
            engine.Seek(context, targetTime, dContext.VideoContext);
        }

        /// <summary>
        /// 对单个解码器上下文进行跳转，并刷新解码器缓冲区。
        /// 跳转后自动调用 Flush，确保解码器状态同步，避免脏数据影响后续解码。
        /// </summary>
        /// <param name="engine">FFmpegEngine 实例。</param>
        /// <param name="context">格式上下文指针。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="dContext">解码器上下文。</param>
        public static void Seek(this FFmpegEngine engine, NativeIntPtr<AVFormatContext> context, long targetTime, FFmpegDecoderContext dContext)
        {
            engine.Seek(context, targetTime, dContext.CodecStream);
            engine.Flush(ref dContext.CodecContext);
        }
    }
}
