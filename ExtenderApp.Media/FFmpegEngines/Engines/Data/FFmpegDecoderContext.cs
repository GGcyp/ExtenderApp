using ExtenderApp.Contracts;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines.Decoders
{
    /// <summary>
    /// 解码器上下文封装，包含解码器、流参数和解码器实例指针。
    /// 用于统一管理 FFmpeg 解码相关的原生指针，便于托管代码安全访问和传递。
    /// </summary>
    public struct FFmpegDecoderContext
    {
        /// <summary>
        /// 空实例，所有指针均为 None。
        /// </summary>
        public static FFmpegDecoderContext Empty => new(NativeIntPtr<AVCodec>.Empty, NativeIntPtr<AVCodecParameters>.Empty, NativeIntPtr<AVCodecContext>.Empty, NativeIntPtr<AVStream>.Empty, -1, FFmpegMediaType.UNKNOWN);

        /// <summary>
        /// 解码器指针（AVCodec），用于描述解码器类型和能力。
        /// </summary>
        public NativeIntPtr<AVCodec> Codec;

        /// <summary>
        /// 编解码参数指针（AVCodecParameters），包含流的基础参数信息（如分辨率、采样率、格式等）。
        /// </summary>
        public NativeIntPtr<AVCodecParameters> CodecParameters;

        /// <summary>
        /// 解码器上下文指针（AVCodecContext），用于实际解码操作和状态管理。
        /// </summary>
        public NativeIntPtr<AVCodecContext> CodecContext;

        /// <summary>
        /// 解码器对应的流指针（AVStream），包含流的元信息和时间基准。
        /// </summary>
        public NativeIntPtr<AVStream> CodecStream;

        /// <summary>
        /// 解析包在对应媒体文件中的流索引。
        /// </summary>
        public int StreamIndex { get; }

        /// <summary>
        /// 当前解码器上下文对应的媒体类型（音频或视频）。
        /// </summary>
        public FFmpegMediaType MediaType { get; }

        /// <summary>
        /// 是否为空（任一指针为空则视为无效上下文）。
        /// </summary>
        public bool IsEmpty => Codec.IsEmpty || CodecParameters.IsEmpty || CodecContext.IsEmpty;

        /// <summary>
        /// 构造函数，初始化解码器上下文各指针。
        /// </summary>
        /// <param name="codec">解码器指针。</param>
        /// <param name="codecParameters">编解码参数指针。</param>
        /// <param name="codecContext">解码器上下文指针。</param>
        /// <param name="stream">解码器对应的流指针。</param>
        /// <param name="streamIndex">流索引。</param>
        /// <param name="type">媒体类型。</param>
        public FFmpegDecoderContext(NativeIntPtr<AVCodec> codec, NativeIntPtr<AVCodecParameters> codecParameters, NativeIntPtr<AVCodecContext> codecContext, NativeIntPtr<AVStream> stream, int streamIndex, FFmpegMediaType type)
        {
            Codec = codec;
            CodecParameters = codecParameters;
            CodecContext = codecContext;
            CodecStream = stream;
            StreamIndex = streamIndex;
            MediaType = type;
        }
    }
}