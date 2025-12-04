using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 音频解码器。 负责将音频帧解码为指定格式的 PCM
    /// 数据，并通过重采样（SwrContext）输出标准音频帧。 支持音频格式转换、采样率变换和声道布局调整，适用于多媒体播放和处理场景。
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
        public FFmpegAudioDecoder(FFmpegEngine engine, FFmpegDecoderContext context, FFmpegInfo info, FFmpegDecoderSettings settings)
            : base(engine, context, info, settings, settings.AudioMaxCacheLength)
        {
            // 创建 PCM 输出帧并设置参数
            pcmFrame = engine.CreateFrame();
            engine.SettingsAudioFrame(pcmFrame, context, settings);

            // 创建并初始化音频重采样上下文
            swrContext = engine.CreateSwrContext();
            engine.SetSwrContextOptionsAndInit(swrContext, context, settings);
        }

        protected override unsafe void ProcessFrame(NativeIntPtr<AVFrame> frame, out ByteBlock block)
        {
            // 使用 SwrContext 进行音频重采样，输出到 pcmFrame
            Engine.SwrConvert(swrContext, pcmFrame, frame);

            var dataLength = Engine.GetBufferSizeForSamples(pcmFrame);
            block = new(dataLength);

            Span<byte> span = new(pcmFrame.Value->data[0], dataLength);
            block.Write(span);
        }

        protected override void DisposeUnmanagedResources()
        {
            base.DisposeUnmanagedResources();
            Engine.Free(ref swrContext);
            Engine.Free(ref pcmFrame);
        }
    }
}