
using ExtenderApp.Common;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 解码器集合类。
    /// 封装视频和音频解码器的统一管理，便于解码流程的调度与资源释放。
    /// 支持根据流索引获取对应解码器，并自动管理解码器的生命周期。
    /// </summary>
    public class FFmpegDecoderCollection : DisposableObject
    {
        /// <summary>
        /// 视频解码器实例。
        /// </summary>
        public FFmpegVideoDecoder VideoDecoder { get; }

        /// <summary>
        /// 音频解码器实例。
        /// </summary>
        public FFmpegAudioDecoder AudioDecoder { get; }

        /// <summary>
        /// 初始化 FFmpegDecoderCollection 实例，创建视频和音频解码器。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="contexts">视频和音频解码器上下文集合。</param>
        /// <param name="info">媒体基础信息。</param>
        /// <param name="token">取消令牌。</param>
        /// <param name="settings">解码器设置参数。</param>
        public FFmpegDecoderCollection(FFmpegEngine engine, FFmpegDecoderContextCollection contexts, FFmpegInfo info, CancellationToken token, FFmpegDecoderSettings settings)
        {
            VideoDecoder = new FFmpegVideoDecoder(engine, contexts.VideoContext, info, token, settings);
            AudioDecoder = new FFmpegAudioDecoder(engine, contexts.AudioContext, info, token, settings);
        }

        /// <summary>
        /// 释放视频和音频解码器相关资源。
        /// </summary>
        /// <param name="disposing">指示是否由 Dispose 方法调用。</param>
        protected override void Dispose(bool disposing)
        {
            VideoDecoder.Dispose();
            AudioDecoder.Dispose();
        }
    }
}
