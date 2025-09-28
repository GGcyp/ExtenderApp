using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    /// <summary>
    /// FFmpeg 音频解码器。
    /// 负责将音频帧解码为指定格式的 PCM 数据，并通过重采样（SwrContext）输出标准音频帧。
    /// 支持音频格式转换、采样率变换和声道布局调整，适用于多媒体播放和处理场景。
    /// </summary>
    public class FFmpegAudioDecoder : FFmpegDecoder
    {
        /// <summary>
        /// 音频重采样上下文（SwrContext），用于音频格式和采样率转换。
        /// </summary>
        private NativeIntPtr<SwrContext> swrContext;

        /// <summary>
        /// PCM 输出帧（AVFrame），用于存放重采样后的音频数据。
        /// </summary>
        private NativeIntPtr<AVFrame> pcmFrame;

        /// <summary>
        /// 初始化 FFmpegAudioDecoder 实例，配置音频帧和重采样上下文。
        /// </summary>
        /// <param name="engine">FFmpeg 引擎实例。</param>
        /// <param name="context">音频解码器上下文。</param>
        /// <param name="info">媒体基础信息。</param>
        /// <param name="allToken">全局取消令牌。</param>
        /// <param name="settings">解码器设置参数。</param>
        public FFmpegAudioDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, CancellationToken allToken, FFmpegDecoderSettings settings)
            : base(engine, context, info, allToken, settings, settings.AudioMaxCacheLength)
        {
            // 创建 PCM 输出帧并设置参数
            pcmFrame = engine.CreateFrame();
            engine.SettingsAudioFrame(pcmFrame, context, settings);

            // 创建并初始化音频重采样上下文
            swrContext = engine.CreateSwrContext();
            engine.SetSwrContextOptionsAndInit(swrContext, context, settings);
        }

        /// <summary>
        /// 解码并重采样音频帧，将其转换为标准 PCM 数据并调度到上层。
        /// </summary>
        /// <param name="frame">输入的原始音频帧。</param>
        /// <param name="framePts">帧的时间戳。</param>
        protected override void ProtectedDecoding(NativeIntPtr<AVFrame> frame, long framePts)
        {
            // 使用 SwrContext 进行音频重采样，输出到 pcmFrame
            Engine.SwrConvert(swrContext, pcmFrame, frame);

            // 拷贝 PCM 数据到托管缓冲区
            var buffer = Engine.CopyFrameToBuffer(pcmFrame, (long)Settings.ChannelLayout, out int length);

            // 获取帧持续时间
            long duration = Engine.GetFrameDuration(frame, Context);

            // 构造音频帧并调度到上层
            AudioFrame audioFrame = new AudioFrame(buffer, length, Settings.SampleRate, (int)Settings.ChannelLayout, 16, framePts, duration);
            Settings.OnAudioScheduling(audioFrame);
        }

        /// <summary>
        /// 释放音频解码器相关资源，包括重采样上下文和 PCM 帧。
        /// </summary>
        /// <param name="disposing">指示是否由 Dispose 方法调用。</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Engine.Free(ref swrContext);
            Engine.Free(ref pcmFrame);
        }
    }
}



