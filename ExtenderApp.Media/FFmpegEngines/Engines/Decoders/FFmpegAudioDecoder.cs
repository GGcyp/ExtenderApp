using ExtenderApp.Data;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// FFmpeg 音频解码器。
    /// <para>职责：
    /// <list type="bullet">
    /// <item>
    /// <description>从基类 <see cref="FFmpegDecoder"/> 获取解码后的原始音频 <see cref="AVFrame"/>。</description>
    /// </item>
    /// <item>
    /// <description>使用 <see cref="SwrContext"/> 做重采样/重排：采样率、采样格式、声道布局（layout）统一为目标输出格式。</description>
    /// </item>
    /// <item>
    /// <description>将重采样后的 PCM 数据拷贝到 <see cref="ByteBlock"/>，形成可安全跨线程消费的标准音频帧输出。</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>资源管理说明：
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="swrContext"/>：由 swresample 创建并初始化，必须显式释放。</description>
    /// </item>
    /// <item>
    /// <description><see cref="pcmFrame"/>：作为重采样输出帧的复用缓存，从引擎帧池获取；销毁时交由引擎释放/回收。</description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    public class FFmpegAudioDecoder : FFmpegDecoder
    {
        /// <summary>
        /// 音频重采样上下文（SwrContext）。
        /// <para>用途：将输入音频帧转换为指定输出格式，例如：
        /// <list type="bullet">
        /// <item>
        /// <description>采样率变换（44.1k -&gt; 48k）</description>
        /// </item>
        /// <item>
        /// <description>采样格式变换（FLTP -&gt; S16）</description>
        /// </item>
        /// <item>
        /// <description>声道布局变换（5.1 -&gt; stereo）</description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>生命周期：构造时创建并 init；Dispose 时释放。</para>
        /// </summary>
        private NativeIntPtr<SwrContext> swrContext;

        /// <summary>
        /// PCM 输出帧（ <see cref="AVFrame"/>）。
        /// <para>这是一个“复用帧”：每次处理输入帧时， <see cref="Engine.SwrConvert"/> 都会把重采样后的结果写到该帧中， 然后再把帧中的 PCM 数据拷贝到 <see cref="ByteBlock"/>。</para>
        /// <para>注意：该帧通常不直接下发给外部消费者，仅作为 swr 的输出容器。</para>
        /// </summary>
        private NativeIntPtr<AVFrame> pcmFrame;

        /// <summary>
        /// 创建 <see cref="FFmpegAudioDecoder"/>。
        /// <para>初始化一次性资源：
        /// <list type="bullet">
        /// <item>
        /// <description>从引擎帧池获取 <see cref="pcmFrame"/> 并设置输出帧参数（采样率/格式/声道布局等）。</description>
        /// </item>
        /// <item>
        /// <description>创建并初始化 <see cref="swrContext"/>，配置输入/输出音频参数。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="context">解码器上下文（包含 <see cref="AVCodecContext"/> / <see cref="AVStream"/> 等指针与流信息）。</param>
        /// <param name="controllerContext">控制器上下文（Engine + generation）。</param>
        /// <param name="settings">音频解码设置（目标采样率/采样格式/声道布局等）。</param>
        public FFmpegAudioDecoder(FFmpegDecoderContext context, FFmpegDecoderControllerContext controllerContext, FFmpegDecoderSettings settings)
            : base(context, controllerContext, settings.AudioPacketCacheCount, settings.AudioFrameCacheCount)
        {
            // 创建 PCM 输出帧并设置参数： 输出帧的 format/ch_layout/sample_rate/nb_samples 等由 settings 决定
            pcmFrame = Engine.GetFrame();
            Engine.SettingsAudioFrame(pcmFrame, context, settings);

            // 创建并初始化音频重采样上下文： 将输入（由 codec 输出决定）转换为 settings 指定的统一输出格式
            swrContext = Engine.CreateSwrContext();
            Engine.SetSwrContextOptionsAndInit(swrContext, context, settings);
        }

        /// <summary>
        /// 将输入的音频帧重采样为目标 PCM 格式，并输出为 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="frame">解码得到的原始音频帧（格式/采样率/布局可能与目标不同）。</param>
        /// <param name="block">输出 PCM 数据块（每次生成新的块，便于下游异步消费）。</param>
        /// <remarks>
        /// 注意事项：
        /// <list type="bullet">
        /// <item>
        /// <description><see cref="pcmFrame"/> 内部数据指针由 FFmpeg 管理；因此必须在本方法中把数据拷贝出来。</description>
        /// </item>
        /// <item>
        /// <description>此实现假设输出为“打包格式（packed）”且数据在 <c>data[0]</c> 连续；若输出为 planar，需要按通道分别拷贝。</description>
        /// </item>
        /// </list>
        /// </remarks>
        protected override unsafe void ProcessFrame(NativeIntPtr<AVFrame> frame, out ByteBlock block)
        {
            // 使用 SwrContext 进行音频重采样，输出到 pcmFrame
            Engine.SwrConvert(swrContext, pcmFrame, frame);

            int dataLength = Engine.GetBufferSizeForSamples(pcmFrame);
            int channelCount = Engine.GetChannelCount(pcmFrame);
            if (dataLength <= 0 || channelCount <= 0)
            {
                block = default;
                return;
            }

            block = new(dataLength);
            if (!Engine.IsPlanar(pcmFrame))
            {
                // packed：只有 data[0] 有效，且总长度应使用 GetBufferSizeForSamples
                Span<byte> span = new(pcmFrame.Value->data[0], dataLength);
                block.Write(span);
            }
            else
            {
                // planar：每声道一个 plane，逐 plane 写入
                for (uint i = 0; i < channelCount; i++)
                {
                    Span<byte> span = Engine.GetFrameData(pcmFrame, i);
                    block.Write(span);
                }
            }
        }

        protected override unsafe long GetFrameDurationMsProtected(NativeIntPtr<AVFrame> framePtr)
        {
            int sampleCount = Engine.GetSampleCount(framePtr);
            if (sampleCount <= 0)
            {
                return 1;
            }

            int sampleRate = Engine.GetSampleRate(framePtr);
            if (sampleRate <= 0 && !DecoderContext.IsEmpty && !DecoderContext.CodecContext.IsEmpty)
            {
                sampleRate = DecoderContext.CodecContext.Value->sample_rate;
            }
            if (sampleRate <= 0)
            {
                sampleRate = Info.SampleRate;
            }
            if (sampleRate <= 0)
            {
                return 1;
            }

            long estimated = (long)System.Math.Round(sampleCount * 1000d / sampleRate);
            return estimated > 0 ? estimated : 1;
        }

        /// <summary>
        /// 释放非托管资源。
        /// <para>顺序说明：
        /// <list type="bullet">
        /// <item>
        /// <description>释放 <see cref="swrContext"/>（swresample 上下文）。</description>
        /// </item>
        /// <item>
        /// <description>释放/回收 <see cref="pcmFrame"/>（输出帧容器）。</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            base.DisposeUnmanagedResources();
            Engine.Free(ref swrContext);
            Engine.Return(pcmFrame);
        }
    }
}