using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using ExtenderApp.Data;
using ExtenderApp.FFmpegEngines.Decoders;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    /// <summary>
    /// FFmpegEngine 类封装了FFmpeg的功能，用于处理音视频文件。
    /// </summary>
    public unsafe partial class FFmpegEngine : DisposableObject
    {
        #region SWS_Const

        /// <summary>
        /// FFmpeg 图像处理相关常量定义。 包含音视频重采样、像素格式转换、色彩空间、缩放算法等常用参数，便于调用 FFmpeg swscale/swr 等接口时使用。 常量值与
        /// FFmpeg C API 保持一致，适用于 sws_getContext、swr_alloc_set_opts 等场景。
        /// </summary>
        /// <remarks>
        /// 例如：SWS_FAST_BILINEAR 表示快速双线性缩放，SWS_BICUBIC 表示双三次缩放，SWS_CS_ITU709 表示 BT.709 色彩空间等。
        /// </remarks>
        public const int SWR_FLAG_RESAMPLE = 0x1;

        /// <summary>
        /// SWS_ACCURATE_RND = 0x40000，精确舍入算法
        /// </summary>
        public const int SWS_ACCURATE_RND = 0x40000;

        /// <summary>
        /// SWS_AREA = 0x20，面积采样算法
        /// </summary>
        public const int SWS_AREA = 0x20;

        /// <summary>
        /// SWS_BICUBIC = 0x4，双三次插值算法
        /// </summary>
        public const int SWS_BICUBIC = 0x4;

        /// <summary>
        /// SWS_BICUBLIN = 0x40，线性双三次插值
        /// </summary>
        public const int SWS_BICUBLIN = 0x40;

        /// <summary>
        /// SWS_BILINEAR = 0x2，双线性插值算法
        /// </summary>
        public const int SWS_BILINEAR = 0x2;

        /// <summary>
        /// SWS_BITEXACT = 0x80000，位精确处理
        /// </summary>
        public const int SWS_BITEXACT = 0x80000;

        /// <summary>
        /// SWS_CS_BT2020 = 0x9，BT.2020 色彩空间
        /// </summary>
        public const int SWS_CS_BT2020 = 0x9;

        /// <summary>
        /// SWS_CS_DEFAULT = 0x5，默认色彩空间
        /// </summary>
        public const int SWS_CS_DEFAULT = 0x5;

        /// <summary>
        /// SWS_CS_FCC = 0x4，FCC 色彩空间
        /// </summary>
        public const int SWS_CS_FCC = 0x4;

        /// <summary>
        /// SWS_CS_ITU601 = 0x5，ITU-R BT.601 色彩空间
        /// </summary>
        public const int SWS_CS_ITU601 = 0x5;

        /// <summary>
        /// SWS_CS_ITU624 = 0x5，ITU-R BT.624 色彩空间
        /// </summary>
        public const int SWS_CS_ITU624 = 0x5;

        /// <summary>
        /// SWS_CS_ITU709 = 0x1，ITU-R BT.709 色彩空间
        /// </summary>
        public const int SWS_CS_ITU709 = 0x1;

        /// <summary>
        /// SWS_CS_SMPTE170M = 0x5，SMPTE 170M 色彩空间
        /// </summary>
        public const int SWS_CS_SMPTE170M = 0x5;

        /// <summary>
        /// SWS_CS_SMPTE240M = 0x7，SMPTE 240M 色彩空间
        /// </summary>
        public const int SWS_CS_SMPTE240M = 0x7;

        /// <summary>
        /// SWS_DIRECT_BGR = 0x8000，直接 BGR 排布
        /// </summary>
        public const int SWS_DIRECT_BGR = 0x8000;

        /// <summary>
        /// SWS_ERROR_DIFFUSION = 0x800000，误差扩散算法
        /// </summary>
        public const int SWS_ERROR_DIFFUSION = 0x800000;

        /// <summary>
        /// SWS_FAST_BILINEAR = 0x1，快速双线性插值
        /// </summary>
        public const int SWS_FAST_BILINEAR = 0x1;

        /// <summary>
        /// SWS_FULL_CHR_H_INP = 0x4000，完整色度采样输入
        /// </summary>
        public const int SWS_FULL_CHR_H_INP = 0x4000;

        /// <summary>
        /// SWS_FULL_CHR_H_INT = 0x2000，完整色度采样内部处理
        /// </summary>
        public const int SWS_FULL_CHR_H_INT = 0x2000;

        /// <summary>
        /// SWS_GAUSS = 0x80，高斯滤波算法
        /// </summary>
        public const int SWS_GAUSS = 0x80;

        /// <summary>
        /// SWS_LANCZOS = 0x200，Lanczos 插值算法
        /// </summary>
        public const int SWS_LANCZOS = 0x200;

        /// <summary>
        /// SWS_MAX_REDUCE_CUTOFF = 0.002D，最大降采样截止频率
        /// </summary>
        public const double SWS_MAX_REDUCE_CUTOFF = 0.002D;

        /// <summary>
        /// SWS_PARAM_DEFAULT = 0x1e240，默认参数
        /// </summary>
        public const int SWS_PARAM_DEFAULT = 0x1e240;

        /// <summary>
        /// SWS_POINT = 0x10，点采样算法
        /// </summary>
        public const int SWS_POINT = 0x10;

        /// <summary>
        /// SWS_PRINT_INFO = 0x1000，打印信息标志
        /// </summary>
        public const int SWS_PRINT_INFO = 0x1000;

        /// <summary>
        /// SWS_SINC = 0x100，Sinc 插值算法
        /// </summary>
        public const int SWS_SINC = 0x100;

        /// <summary>
        /// SWS_SPLINE = 0x400，样条插值算法
        /// </summary>
        public const int SWS_SPLINE = 0x400;

        /// <summary>
        /// SWS_SRC_V_CHR_DROP_MASK = 0x30000，源垂直色度丢弃掩码
        /// </summary>
        public const int SWS_SRC_V_CHR_DROP_MASK = 0x30000;

        /// <summary>
        /// SWS_SRC_V_CHR_DROP_SHIFT = 0x10，源垂直色度丢弃位移
        /// </summary>
        public const int SWS_SRC_V_CHR_DROP_SHIFT = 0x10;

        /// <summary>
        /// SWS_X = 0x8，未知/扩展标志
        /// </summary>
        public const int SWS_X = 0x8;

        #endregion SWS_Const

        /// <summary>
        /// 最大缓存数据包数量（用于限制 _packetQueue 队列长度）。 
        /// 控制解码过程中可缓存的 AVPacket 数量，防止内存占用过高。
        /// 建议根据实际业务场景和内存压力调整，默认值为 10。
        /// </summary>
        public int MaxCachePacketCount = 10;

        /// <summary>
        /// 最大缓存帧数量（用于限制 _frameQueue 队列长度）。 
        /// 控制解码过程中可缓存的 AVFrame 数量，防止内存占用过高。 
        /// 建议根据实际业务场景和解码速率调整，默认值为 10。
        /// </summary>
        public int MaxCacheFrameCount = 10;

        /// <summary>
        /// 默认流索引。
        /// </summary>
        public const int DefaultStreamIndex = -1;

        /// <summary>
        /// 默认帧率。
        /// </summary>
        public const double DefaultFrameRate = 25.0;

        /// <summary>
        /// 存储指针的HashSet。
        /// </summary>
        private readonly HashSet<nint> _intPtrHashSet;

        /// <summary>
        /// 数据包队列。
        /// </summary>
        public readonly ConcurrentQueue<NativeIntPtr<AVPacket>> _packetQueue;

        /// <summary>
        /// 帧队列。
        /// </summary>
        private readonly ConcurrentQueue<NativeIntPtr<AVFrame>> _frameQueue;

        /// <summary>
        /// 获取FFmpeg的版本信息。
        /// </summary>
        public string FFmpegVersion => ffmpeg.av_version_info();

        /// <summary>
        /// 获取FFmpeg的安装路径。
        /// </summary>
        public string FFmpegPath => ffmpeg.RootPath;

        /// <summary>
        /// 初始化FFmpegEngine实例。
        /// </summary>
        /// <param name="ffmpegPath">FFmpeg的安装路径。</param>
        internal FFmpegEngine(string ffmpegPath)
        {
            _intPtrHashSet = new();
            _packetQueue = new();
            _frameQueue = new();
            ffmpeg.RootPath = ffmpegPath;
            ffmpeg.avformat_network_init();
        }

        #region Open

        /// <summary>
        /// 打开指定URI的音视频流，并返回一个FFmpegContext实例。
        /// </summary>
        /// <param name="uri">要打开的URI。</param>
        /// <returns>FFmpegContext实例。</returns>
        public FFmpegContext OpenUri(string uri)
        {
            return OpenUri(uri, FFmpegInputFormat.Empty, null);
        }

        /// <summary>
        /// 打开指定URI的音视频流，并返回一个FFmpegContext实例。
        /// </summary>
        /// <param name="uri">要打开的URI。</param>
        /// <param name="inputFormat">输入格式。</param>
        /// <param name="options">选项。</param>
        /// <returns>FFmpegContext实例。</returns>
        public FFmpegContext OpenUri(string uri, FFmpegInputFormat inputFormat, Dictionary<string, string>? options)
        {
            var formatContext = CreateFormatContext().Value;
            AVFormatContext** formatContextPtr = &formatContext;
            var nOptions = CreateOptions(options);
            var inputOptions = nOptions.Value;
            AVDictionary** inputOptionsIntPtr = &inputOptions;
            int result = ffmpeg.avformat_open_input(formatContextPtr, uri, inputFormat, inputOptionsIntPtr);

            if (result < 0)
            {
                ShowException($"未找到指定uri：{uri}", result);
            }

            result = ffmpeg.avformat_find_stream_info(formatContext, inputOptionsIntPtr);
            if (result < 0)
            {
                throw new FFmpegException($"无法获取流信息:{uri}");
            }

            FFmpegDecoderContextCollection collection = CreateDecoderContextCollection(formatContext, nOptions, uri);

            var info = CreateFFmpegInfo(uri, collection);

            return new FFmpegContext(this, formatContext, nOptions, info, collection);
        }

        #endregion Open

        #region Info

        /// <summary>
        /// 创建FFmpegInfo实例。
        /// </summary>
        /// <param name="uri">URI。</param>
        /// <param name="collection">解码器上下文集合。</param>
        /// <returns>FFmpegInfo实例。</returns>
        private FFmpegInfo CreateFFmpegInfo(string uri, FFmpegDecoderContextCollection collection)
        {
            var videoContext = collection[FFmpegMediaType.VIDEO];
            var audioContext = collection[FFmpegMediaType.AUDIO];

            FFmpegInfo info = new(uri, videoContext.CodecContext.Value->pix_fmt.Convert(), audioContext.CodecContext.Value->sample_fmt.Convert(), GetCodecNameOrDefault(videoContext), GetCodecNameOrDefault(audioContext));
            info.Width = videoContext.CodecParameters.Value->width;
            info.Height = videoContext.CodecParameters.Value->height;
            info.Duration = ffmpeg.av_rescale_q(videoContext.CodecStream.Value->duration, videoContext.CodecStream.Value->time_base, ffmpeg.av_make_q(1, 1000));
            info.Rate = videoContext.CodecStream.Value->avg_frame_rate.num != 0 ? (double)videoContext.CodecStream.Value->avg_frame_rate.num / videoContext.CodecStream.Value->avg_frame_rate.den : DefaultFrameRate;

            info.SampleRate = audioContext.CodecParameters.Value->sample_rate;
            info.Channels = audioContext.CodecParameters.Value->ch_layout.nb_channels;
            info.BitRate = audioContext.CodecContext.Value->bit_rate;
            return info;
        }

        #endregion Info

        #region Create

        /// <summary>
        /// 创建AVFormatContext实例。
        /// </summary>
        /// <returns>AVFormatContext实例。</returns>
        public NativeIntPtr<AVFormatContext> CreateFormatContext()
        {
            NativeIntPtr<AVFormatContext> formatContext = ffmpeg.avformat_alloc_context();
            _intPtrHashSet.Add(formatContext);
            return formatContext;
        }

        /// <summary>
        /// 创建AVDictionary实例。
        /// </summary>
        /// <param name="options">选项。</param>
        /// <returns>AVDictionary实例。</returns>
        public NativeIntPtr<AVDictionary> CreateOptions(Dictionary<string, string>? options)
        {
            AVDictionary** dict = null;
            if (options != null)
            {
                foreach (var option in options)
                {
                    ffmpeg.av_dict_set(dict, option.Key, option.Value, 0);
                }
            }
            return dict;
        }

        /// <summary>
        /// 创建AVFrame实例。
        /// </summary>
        /// <returns>AVFrame实例。</returns>
        public NativeIntPtr<AVFrame> CreateFrame()
        {
            NativeIntPtr<AVFrame> frame = ffmpeg.av_frame_alloc();
            _intPtrHashSet.Add(frame);
            return frame;
        }

        /// <summary>
        /// 创建AVPacket实例。
        /// </summary>
        /// <returns>AVPacket实例。</returns>
        public NativeIntPtr<AVPacket> CreatePacket()
        {
            NativeIntPtr<AVPacket> packet = ffmpeg.av_packet_alloc();
            _intPtrHashSet.Add(packet);
            return packet;
        }

        /// <summary>
        /// 创建图像像素格式转换上下文（SwsContext）。 用于将源图像从指定分辨率和像素格式转换为目标分辨率和像素格式，常用于视频解码后进行缩放或格式转换。
        /// </summary>
        /// <param name="srcW">源图像宽度（像素）。</param>
        /// <param name="srcH">源图像高度（像素）。</param>
        /// <param name="srcFormat">源像素格式（AVPixelFormat）。</param>
        /// <param name="dstW">目标图像宽度（像素）。</param>
        /// <param name="dstH">目标图像高度（像素）。</param>
        /// <param name="dstFormat">目标像素格式（AVPixelFormat）。</param>
        /// <param name="flags">转换标志，通常使用 SWS_FAST_BILINEAR、SWS_BICUBIC 等。</param>
        /// <returns>像素格式转换上下文指针（SwsContext）。</returns>
        public NativeIntPtr<SwsContext> CreateSwsContext(int srcW, int srcH, AVPixelFormat srcFormat, int dstW, int dstH, AVPixelFormat dstFormat, int flags = SWS_BILINEAR)
        {
            NativeIntPtr<SwsContext> swsContext = ffmpeg.sws_getContext(srcW, srcH, srcFormat, dstW, dstH, dstFormat, flags, null, null, null);
            _intPtrHashSet.Add(swsContext);
            return swsContext;
        }

        /// <summary>
        /// 创建带有自定义滤镜的图像像素格式转换上下文（SwsContext）。 可指定源和目标滤镜，用于更复杂的图像处理场景（如锐化、模糊等）。
        /// </summary>
        /// <param name="srcW">源图像宽度（像素）。</param>
        /// <param name="srcH">源图像高度（像素）。</param>
        /// <param name="srcFormat">源像素格式（AVPixelFormat）。</param>
        /// <param name="dstW">目标图像宽度（像素）。</param>
        /// <param name="dstH">目标图像高度（像素）。</param>
        /// <param name="dstFormat">目标像素格式（AVPixelFormat）。</param>
        /// <param name="flags">转换标志，通常使用 SWS_FAST_BILINEAR、SWS_BICUBIC 等。</param>
        /// <param name="srcFilter">源图像滤镜指针（SwsFilter）。</param>
        /// <param name="dstFilter">目标图像滤镜指针（SwsFilter）。</param>
        /// <returns>像素格式转换上下文指针（SwsContext）。</returns>
        public NativeIntPtr<SwsContext> CreateSwsContext(int srcW, int srcH, AVPixelFormat srcFormat, int dstW, int dstH, AVPixelFormat dstFormat, int flags, NativeIntPtr<SwsFilter> srcFilter, NativeIntPtr<SwsFilter> dstFilter)
        {
            NativeIntPtr<SwsContext> swsContext = ffmpeg.sws_getContext(srcW, srcH, srcFormat, dstW, dstH, dstFormat, flags, srcFilter, dstFilter, null);
            _intPtrHashSet.Add(swsContext);
            return swsContext;
        }

        /// <summary>
        /// 创建音频重采样上下文（SwrContext）。
        /// </summary>
        /// <returns>返回创建成功的音频重采样上下文（SwrContext）</returns>
        /// <exception cref="FFmpegException">当创建失败的时候调用</exception>
        public NativeIntPtr<SwrContext> CreateSwrContext()
        {
            NativeIntPtr<SwrContext> swrContext = ffmpeg.swr_alloc();
            if (swrContext.IsEmpty)
            {
                throw new FFmpegException("无法创建 SwrContext");
            }
            _intPtrHashSet.Add(swrContext);
            return swrContext;
        }

        /// <summary>
        /// 创建并分配一个 RGB 图像缓冲区，并将其与指定 AVFrame 进行数据指针和行对齐绑定。 用于将托管字节数组与 FFmpeg 的 AVFrame
        /// 结构关联，便于后续像素数据填充和渲染。 内部会从共享内存池租用字节数组，调用 CreateRGBBuffer 方法完成映射。
        /// </summary>
        /// <param name="rgbFrame">目标 AVFrame 指针，写入 data 和 linesize 信息。</param>
        /// <param name="rgbBufferLength">RGB 缓冲区长度（字节），需根据像素格式和分辨率预先计算。</param>
        /// <param name="pixelFormat">像素格式（AVPixelFormat），如 AV_PIX_FMT_RGB24。</param>
        /// <param name="width">图像宽度（像素）。</param>
        /// <param name="height">图像高度（像素）。</param>
        /// <returns>分配好的 RGB 图像缓冲区（byte数组地址）。</returns>
        public NativeByteMemory CreateRGBBuffer(ref NativeIntPtr<AVFrame> rgbFrame, int rgbBufferLength, AVPixelFormat pixelFormat, int width, int height)
        {
            NativeByteMemory memory = new(rgbBufferLength);
            try
            {
                CreateRGBBuffer(ref rgbFrame, memory.Ptr, pixelFormat, width, height);
            }
            catch
            {
                memory.Dispose();
                throw;
            }
            return memory;
        }

        /// <summary>
        /// 创建并填充 RGB 图像缓冲区，并将其数据指针和行对齐信息写入 AVFrame。 用于将托管字节数组 rgbBuffer 映射到 FFmpeg 的 AVFrame
        /// 结构，便于后续像素数据处理或渲染。 内部调用 FFmpeg 的 av_image_fill_arrays 方法，自动计算 data 和 linesize。
        /// </summary>
        /// <param name="rgbBuffer">托管的 RGB 图像缓冲区（byte[]），需预先分配好大小。</param>
        /// <param name="rgbFrame">目标 AVFrame 指针，写入 data 和 linesize 信息。</param>
        /// <param name="pixelFormat">像素格式（AVPixelFormat），如 AV_PIX_FMT_RGB24。</param>
        /// <param name="width">图像宽度（像素）。</param>
        /// <param name="height">图像高度（像素）。</param>
        public void CreateRGBBuffer(ref NativeIntPtr<AVFrame> rgbFrame, NativeIntPtr<byte> rgbBuffer, AVPixelFormat pixelFormat, int width, int height)
        {
            if (rgbFrame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(rgbFrame));
            }
            if (rgbBuffer.IsEmpty)
            {
                throw new ArgumentNullException(nameof(rgbBuffer));
            }

            // 声明临时变量
            byte_ptrArray4 data4 = default;
            int_array4 linesize4 = default;
            ffmpeg.av_image_fill_arrays(
                ref data4,
                ref linesize4,
                rgbBuffer,
                pixelFormat,
                width,
                height,
                1);

            // 复制到rgbFrame->data和rgbFrame->linesize
            for (uint i = 0; i < 4; i++)
            {
                rgbFrame.Value->data[i] = data4[i];
                rgbFrame.Value->linesize[i] = linesize4[i];
            }
        }

        /// <summary>
        /// 创建解码器上下文集合。
        /// </summary>
        /// <param name="formatContext">AVFormatContext指针。</param>
        /// <param name="options">选项指针。</param>
        /// <param name="uri">URI。</param>
        /// <returns>解码器上下文集合。</returns>
        public FFmpegDecoderContextCollection CreateDecoderContextCollection(AVFormatContext* formatContext, NativeIntPtr<AVDictionary> options, string? uri = null)
        {
            var valure = options.Value;
            var vptr = &valure;

            TryGetDecoderContexts(formatContext, vptr, out var Decoders);

            return new FFmpegDecoderContextCollection(Decoders);
        }

        /// <summary>
        /// 创建解码器集合（FFmpegDecoderCollection）。 用于根据上下文和解码设置，自动初始化视频和音频解码器，并管理解码过程中的资源。 支持自定义取消令牌和解码参数，便于多线程解码和参数调整。
        /// </summary>
        /// <param name="context">FFmpeg上下文，包含解码器集合和媒体信息。</param>
        /// <param name="settings">可选的解码器设置，若为空则使用默认设置。</param>
        /// <param name="source">可选的取消令牌源，若为空则自动创建。</param>
        /// <returns>FFmpegDecoderCollection 实例。</returns>
        public FFmpegDecoderCollection CreateDecoderCollection(FFmpegContext context, FFmpegDecoderSettings? settings, CancellationTokenSource? source = null)
        {
            if (context.IsEmpty)
            {
                throw new ArgumentNullException(nameof(context));
            }
            source = source ?? new CancellationTokenSource();
            settings = settings ?? new FFmpegDecoderSettings();
            return new FFmpegDecoderCollection(this, context.ContextCollection, context.Info, settings);
        }

        /// <summary>
        /// 创建解码控制器（FFmpegDecoderController）。 用于管理解码流程，包括解码启动、停止、跳转、资源释放等操作。 支持多线程解码、取消令牌控制和解码器集合的统一管理。
        /// </summary>
        /// <param name="context">FFmpeg上下文，包含媒体信息和解码器集合。</param>
        /// <param name="settings"></param>
        /// <param name="source">可选的取消令牌源，若为空则自动创建。</param>
        /// <returns>FFmpegDecoderController 实例。</returns>
        public FFmpegDecoderController CreateDecoderController(FFmpegContext context, FFmpegDecoderSettings settings, CancellationTokenSource? source = null)
        {
            source ??= new CancellationTokenSource();
            var collection = CreateDecoderCollection(context, settings, source);
            return new FFmpegDecoderController(this, context, collection, source);
        }

        /// <summary>
        /// 根据指定的声道布局掩码（如 AV_CH_LAYOUT_STEREO），创建并分配一个 AVChannelLayout 结构体指针。 可用于音频重采样、格式转换等场景，便于配置
        /// SwrContext 的输入/输出声道布局参数。 内部调用 FFmpeg 的 av_channel_layout_default 方法进行初始化， 返回的
        /// NativeIntPtr&lt;AVChannelLayout&gt; 可直接用于 FFmpeg 相关接口。
        /// </summary>
        /// <param name="layout">声道布局掩码（如 AV_CH_LAYOUT_STEREO、AV_CH_LAYOUT_MONO 等）。</param>
        /// <returns>分配并初始化好的 AVChannelLayout 指针封装。</returns>
        public NativeIntPtr<AVChannelLayout> CreateChannelLayout(ulong layout)
        {
            AVChannelLayout* ptr = (AVChannelLayout*)ffmpeg.av_malloc((ulong)sizeof(AVChannelLayout));
            ffmpeg.av_channel_layout_default(ptr, (int)layout);
            NativeIntPtr<AVChannelLayout> intPtr = ptr;
            _intPtrHashSet.Add(intPtr);
            return intPtr;
        }

        #endregion Create

        #region Get

        /// <summary>
        /// 获取指定媒体类型的流索引。
        /// </summary>
        /// <param name="formatContext">AVFormatContext指针。</param>
        /// <param name="mediaType">媒体类型。</param>
        /// <returns>流索引，未找到则返回默认索引。</returns>
        private int GetStreamIndex(AVFormatContext* formatContext, AVMediaType mediaType)
        {
            uint count = formatContext->nb_streams;
            for (int i = 0; i < count; i++)
            {
                AVStream* stream = formatContext->streams[i];
                AVMediaType codecType = stream->codecpar->codec_type;
                if (codecType == mediaType)
                {
                    return i;
                }
            }
            return DefaultStreamIndex;
        }

        #endregion Get

        #region Seek

        /// <summary>
        /// 跳转到指定时间戳（毫秒）的位置。 根据给定的 AVStream 指针，自动获取流索引和时间基准，调用重载方法完成跳转。 适用于需要按流类型（如视频或音频）精确定位的场景。
        /// </summary>
        /// <param name="context">格式上下文指针（AVFormatContext），包含媒体流信息。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="streamPtr">目标流指针（AVStream），用于获取流索引和时间基准。</param>
        public void Seek(NativeIntPtr<AVFormatContext> context, long targetTime, NativeIntPtr<AVStream> streamPtr)
        {
            Seek(context, streamPtr.Value->index, targetTime, streamPtr.Value->time_base);
        }

        /// <summary>
        /// 跳转到指定时间戳（毫秒）的位置。 通过指定流索引和时间基准，将媒体流定位到目标时间点（通常为关键帧）。 内部调用 FFmpeg 的 av_seek_frame
        /// 实现跳转，跳转后建议刷新解码器缓冲区（avcodec_flush_buffers）。 跳转失败时会抛出详细异常。
        /// </summary>
        /// <param name="context">格式上下文指针（AVFormatContext），包含媒体流信息。</param>
        /// <param name="streamIndex">目标流索引（如视频流或音频流）。</param>
        /// <param name="targetTime">目标跳转时间（毫秒）。</param>
        /// <param name="timeBase">流的时间基准（AVRational），用于时间戳换算。</param>
        /// <exception cref="FFmpegException">跳转失败时抛出，包含详细错误信息。</exception>
        public void Seek(NativeIntPtr<AVFormatContext> context, int streamIndex, long targetTime, AVRational timeBase)
        {
            long timestamp = ffmpeg.av_rescale_q(targetTime, ffmpeg.av_make_q(1, 1000), timeBase);

            int ret = ffmpeg.av_seek_frame(context, streamIndex, timestamp, ffmpeg.AVSEEK_FLAG_BACKWARD);
            if (ret < 0)
            {
                ShowException($"跳转到指定时间失败{targetTime}", ret);
            }
        }

        #endregion Seek

        #region Format

        /// <summary>
        /// 根据名称查找输入格式。
        /// </summary>
        /// <param name="name">格式名称。</param>
        /// <returns>输入格式对象。</returns>
        public FFmpegInputFormat FindInputFormat(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var input = ffmpeg.av_find_input_format(name);
            NativeIntPtr<AVInputFormat> ptr = new(input);
            return new FFmpegInputFormat(ptr);
        }

        /// <summary>
        /// 根据名称查找输出格式。
        /// </summary>
        /// <param name="name">格式名称。</param>
        /// <returns>输出格式对象。</returns>
        public FFmpegOutputFormat FindOutputFormat(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            var output = ffmpeg.av_guess_format(name, null, null);
            NativeIntPtr<AVOutputFormat> ptr = new(output);
            return new FFmpegOutputFormat(ptr);
        }

        #endregion Format

        #region Decoder

        /// <summary>
        /// 尝试为给定格式上下文中的所有流创建解码器上下文。 此方法会遍历所有流，并为每个流调用 <see
        /// cref="TryGetDecoderContext(AVFormatContext*, int, FFmpegMediaType, AVDictionary**, out
        /// FFmpegDecoderContext)"/> 来初始化解码器。
        /// </summary>
        /// <param name="formatContext">指向 AVFormatContext 的指针，包含媒体文件的所有流信息。</param>
        /// <param name="options">指向 AVDictionary 的指针，用于传递给解码器的额外选项。</param>
        /// <param name="decoders">
        /// 输出参数。当此方法返回时，该数组将包含为每个流成功初始化的 <see
        /// cref="FFmpegDecoderContext"/>。如果某个流无法创建解码器，则数组中对应的元素将是 <see cref="FFmpegDecoderContext.Empty"/>。
        /// </param>
        /// <returns>总是返回 <c>true</c>，表示已完成对所有流的解码器创建尝试。</returns>
        private bool TryGetDecoderContexts(AVFormatContext* formatContext, AVDictionary** options, out FFmpegDecoderContext[] decoders)
        {
            int count = (int)formatContext->nb_streams;
            decoders = new FFmpegDecoderContext[count];
            for (int i = 0; i < count; i++)
            {
                AVStream* stream = formatContext->streams[i];
                AVMediaType codecType = stream->codecpar->codec_type;
                if (TryGetDecoderContext(formatContext, (int)i, codecType.Convert(), options, out FFmpegDecoderContext decoder))
                {
                    decoders[i] = decoder;
                }
            }
            return true;
        }

        /// <summary>
        /// 尝试获取解码器。
        /// </summary>
        /// <param name="formatContext">AVFormatContext指针。</param>
        /// <param name="streamIndex">流索引。</param>
        /// <param name="type">媒体类型。</param>
        /// <param name="options">选项指针。</param>
        /// <param name="decoder">解码器上下文。</param>
        /// <returns>是否成功获取解码器。</returns>
        private bool TryGetDecoderContext(AVFormatContext* formatContext, int streamIndex, FFmpegMediaType type, AVDictionary** options, out FFmpegDecoderContext decoder)
        {
            decoder = FFmpegDecoderContext.Empty;
            NativeIntPtr<AVCodec> codec = NativeIntPtr<AVCodec>.Empty;
            NativeIntPtr<AVCodecParameters> codecParameters = NativeIntPtr<AVCodecParameters>.Empty;
            NativeIntPtr<AVCodecContext> codecContext = NativeIntPtr<AVCodecContext>.Empty;
            NativeIntPtr<AVStream> codecStream = NativeIntPtr<AVStream>.Empty;
            if (!CheckStreamIndex(streamIndex))
            {
                return false;
            }

            codecStream = formatContext->streams[streamIndex];
            codecParameters = codecStream.Value->codecpar;
            codec = ffmpeg.avcodec_find_decoder(codecParameters.Value->codec_id);
            if (codec.IsEmpty)
            {
                //throw new FFmpegException($"未找到解码器: {codecParameters.Item1->codec_id}");
                return false;
            }

            codecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (codecContext.IsEmpty)
            {
                //throw new FFmpegException("无法分配解码器上下文");
                return false;
            }

            //复制流参数到解码器上下文
            int result = ffmpeg.avcodec_parameters_to_context(codecContext, codecParameters);
            if (result < 0)
            {
                //throw new FFmpegException("无法将参数复制到解码器上下文");
                return false;
            }

            //打开解码器
            result = ffmpeg.avcodec_open2(codecContext, codec, options);
            if (result < 0)
            {
                //throw new FFmpegException("无法打开解码器");
                return false;
            }
            _intPtrHashSet.Add(codecContext);
            _intPtrHashSet.Add(codecParameters);
            _intPtrHashSet.Add(codecStream);
            //不需要手动释放
            //_intPtrHashSet.Add(codec);
            decoder = new FFmpegDecoderContext(codec, codecParameters, codecContext, codecStream, streamIndex, type);
            return true;
        }

        #endregion Decoder

        #region Codec

        /// <summary>
        /// 获取解码器名称，如果未找到则返回默认值"未找到"。
        /// </summary>
        /// <param name="decoder">需要被获取的解析器上下文</param>
        /// <returns>获取到的解析器名称，如果获取不到着为：未找到</returns>
        public string GetCodecNameOrDefault(FFmpegDecoderContext decoder)
        {
            string result = "未找到";
            if (decoder.IsEmpty)
            {
                return result;
            }
            string temp = ffmpeg.avcodec_get_name(decoder.Codec.Value->id);
            return string.IsNullOrEmpty(temp) ? result : temp;
        }

        /// <summary>
        /// 获取解码器名称。
        /// </summary>
        /// <param name="decoder">解码器上下文。</param>
        /// <returns>解码器名称。</returns>
        public string GetCodecName(FFmpegDecoderContext decoder)
        {
            if (decoder.IsEmpty)
            {
                return string.Empty;
            }
            return ffmpeg.avcodec_get_name(decoder.Codec.Value->id);
        }

        /// <summary>
        /// 根据编解码器ID获取解码器名称。
        /// </summary>
        /// <param name="codecId">编解码器ID。</param>
        /// <returns>解码器名称。</returns>
        public string GetCodecName(AVCodecID codecId)
        {
            var codec = ffmpeg.avcodec_find_decoder(codecId);
            if (codec == null)
            {
                return string.Empty;
            }
            return ffmpeg.avcodec_get_name(codecId);
        }

        /// <summary>
        /// 检查流索引是否有效。
        /// </summary>
        /// <param name="index">流索引。</param>
        /// <returns>是否有效。</returns>
        private bool CheckStreamIndex(int index)
        {
            return index >= DefaultStreamIndex;
        }

        #endregion Codec

        #region Packet

        /// <summary>
        /// 从AVFormatContext中读取一个数据包，并将其存储到AVPacket中。
        /// </summary>
        /// <param name="context">指向AVFormatContext的指针，包含媒体流信息。</param>
        /// <param name="packet">指向AVPacket的指针，用于存储读取的数据包。</param>
        /// <returns>返回读取的数据包大小，如果读取失败则返回负数。</returns>
        /// <exception cref="ArgumentNullException">如果context或packet为空，则抛出此异常。</exception>
        /// <exception cref="FFmpegException">如果从解码器接收数据包失败，则抛出此异常。</exception>
        public int ReadPacket(NativeIntPtr<AVFormatContext> context, ref NativeIntPtr<AVPacket> packet)
        {
            if (context.IsEmpty)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (packet.IsEmpty)
            {
                packet = GetPacket();
            }
            int result = ffmpeg.av_read_frame(context, packet);
            if (result < 0)
            {
                ShowException("从解码器接收数据包失败", result);
            }
            return result;
        }

        /// <summary>
        /// 向解码器发送一个数据包（AVPacket）。 用于将编码后的数据（如音视频流中的一帧）传递给解码器进行解码处理。
        /// </summary>
        /// <param name="codecContext">解码器上下文指针。</param>
        /// <param name="packet">待发送的数据包指针，若为空则自动分配。</param>
        /// <returns>操作结果码，成功为0，失败抛出异常。</returns>
        public int SendPacket(NativeIntPtr<AVCodecContext> codecContext, ref NativeIntPtr<AVPacket> packet)
        {
            if (codecContext.IsEmpty)
            {
                throw new ArgumentNullException(nameof(codecContext));
            }
            if (packet.IsEmpty)
            {
                packet = GetPacket();
            }
            int result = ffmpeg.avcodec_send_packet(codecContext, packet);
            if (result < 0)
            {
                ShowException("向解码器发送数据包失败", result);
            }
            return result;
        }

        /// <summary>
        /// 从解码器接收一个数据包（AVPacket）。 用于从解码器获取编码后的数据（如转码或编码场景）。
        /// </summary>
        /// <param name="codecContext">解码器上下文指针。</param>
        /// <param name="packet">用于接收的数据包指针，若为空则自动分配。</param>
        /// <returns>操作结果码，成功为0，失败抛出异常。</returns>
        public int ReceivePacket(NativeIntPtr<AVCodecContext> codecContext, ref NativeIntPtr<AVPacket> packet)
        {
            if (codecContext.IsEmpty)
            {
                throw new ArgumentNullException(nameof(codecContext));
            }
            if (packet.IsEmpty)
            {
                packet = CreatePacket();
            }
            int result = ffmpeg.avcodec_receive_packet(codecContext, packet);
            if (result < 0)
            {
                ShowException("从解码器接收数据包失败", result);
            }
            return result;
        }

        /// <summary>
        /// 获取包的流索引。
        /// </summary>
        /// <param name="packet">需要被获取的包</param>
        /// <returns>返回包的流索引</returns>
        /// <exception cref="ArgumentNullException">当包为空时</exception>
        public int GetPacketStreamIndex(NativeIntPtr<AVPacket> packet)
        {
            if (packet.IsEmpty)
            {
                throw new ArgumentNullException(nameof(packet));
            }
            return packet.Value->stream_index;
        }

        /// <summary>
        /// 从数据包队列中获取一个AVPacket实例，如果队列为空则创建一个新的实例。
        /// </summary>
        /// <returns>数据包地址实例</returns>
        public NativeIntPtr<AVPacket> GetPacket()
        {
            if (_packetQueue.TryDequeue(out var packet))
            {
                return packet;
            }
            return CreatePacket();
        }

        /// <summary>
        /// 回收一个AVPacket实例，将其放回数据包队列以供重用。
        /// </summary>
        /// <param name="packet">需要被回收的地址</param>
        public void Return(ref NativeIntPtr<AVPacket> packet)
        {
            if (packet.IsEmpty)
            {
                return;
            }
            else if (_packetQueue.Count >= MaxCachePacketCount)
            {
                Free(ref packet);
                return;
            }
            Flush(ref packet);
            _packetQueue.Enqueue(packet);
            packet = NativeIntPtr<AVPacket>.Empty;
        }

        #endregion Packet

        #region Frame

        /// <summary>
        /// 向解码器发送一个帧（AVFrame）。 用于将解码后的原始帧数据（如音视频帧）传递给编码器或滤镜处理。
        /// </summary>
        /// <param name="codecContext">解码器上下文指针。</param>
        /// <param name="frame">待发送的帧指针，若为空则自动分配。</param>
        /// <returns>操作结果码，成功为0，失败抛出异常。</returns>
        public int SendFrame(NativeIntPtr<AVCodecContext> codecContext, ref NativeIntPtr<AVFrame> frame)
        {
            if (codecContext.IsEmpty)
            {
                throw new ArgumentNullException(nameof(codecContext));
            }
            if (frame.IsEmpty)
            {
                frame = GetFrame();
            }
            int result = ffmpeg.avcodec_send_frame(codecContext, frame);
            if (result < 0)
            {
                ShowException("从解码器发送数据包失败", result);
            }
            return result;
        }

        /// <summary>
        /// 从解码器接收一个帧（AVFrame）。 用于从解码器获取解码后的原始帧数据（如音视频帧）。
        /// </summary>
        /// <param name="codecContext">解码器上下文指针。</param>
        /// <param name="frame">用于接收的帧指针，若为空则自动分配。</param>
        /// <returns>操作结果码，成功为0，失败抛出异常。</returns>
        public int ReceiveFrame(NativeIntPtr<AVCodecContext> codecContext, ref NativeIntPtr<AVFrame> frame)
        {
            if (codecContext.IsEmpty)
            {
                throw new ArgumentNullException(nameof(codecContext));
            }
            if (frame.IsEmpty)
            {
                frame = GetFrame();
            }
            int result = ffmpeg.avcodec_receive_frame(codecContext, frame);
            if (result < 0 && !IsTryAgain(result))
            {
                ShowException("从解码器接收数据包失败", result);
            }
            return result;
        }

        /// <summary>
        /// 为指定的 AVFrame 分配音频或视频缓冲区。 此方法会根据帧的 format、width/height（视频）或 nb_samples/ch_layout（音频）等参数，
        /// 自动分配底层数据缓冲区，并填充 AVFrame 的 data/linesize 指针，确保后续解码、重采样或渲染操作可用。 通常在设置好帧参数后调用本方法，避免 "Output
        /// changed" 等 FFmpeg 错误。
        /// </summary>
        /// <param name="frame">需要分配缓冲区的 AVFrame 指针。</param>
        /// <param name="align">缓冲区对齐方式（字节），音频建议为 1，视频可为 32。</param>
        /// <returns>返回 0 表示分配成功，负值表示分配失败。</returns>
        public int GetBuffer(NativeIntPtr<AVFrame> frame, int align = 1)
        {
            int result = ffmpeg.av_frame_get_buffer(frame, align);
            if (result < 0)
            {
                ShowException("为 AVFrame 分配缓冲区失败", result);
            }
            return result;
        }

        /// <summary>
        /// 根据解码器设置（FFmpegDecoderSettings）批量设置音频输出帧（AVFrame）的参数。
        /// 包括采样格式、采样率、声道布局等，确保输出帧与重采样上下文（SwrContext）参数一致， 便于后续音频重采样和格式转换，避免参数不匹配导致的 FFmpeg 错误。 通常在分配缓冲区前调用本方法。
        /// </summary>
        /// <param name="frame">待设置参数的音频帧指针（AVFrame）。</param>
        /// <param name="dContext">音频解码器上下文，提供输入流参数。</param>
        /// <param name="settings">解码器设置，包含目标采样格式、采样率、声道布局等。</param>
        /// <returns>返回 0 表示设置成功。</returns>
        public int SettingsAudioFrame(NativeIntPtr<AVFrame> frame, FFmpegDecoderContext dContext, FFmpegDecoderSettings settings)
        {
            return SettingsAudioFrame(frame, dContext, (int)settings.SampleFormat, settings.SampleRate, (ulong)settings.Channels);
        }

        /// <summary>
        /// 设置音频输出帧（AVFrame）的参数，使其与指定的解码器上下文和目标采样参数一致。 包括采样格式、采样率、声道布局和采样数，确保后续重采样转换时参数匹配，避免 FFmpeg
        /// "Output changed" 错误。 通常在调用 GetBuffer 分配缓冲区前设置，适用于音频重采样和格式转换场景。
        /// </summary>
        /// <param name="frame">待设置参数的音频帧指针（AVFrame）。</param>
        /// <param name="dContext">音频解码器上下文，提供输入流参数。</param>
        /// <param name="sampleFormat">目标采样格式（如 AV_SAMPLE_FMT_S16）。</param>
        /// <param name="sampleRate">目标采样率（如 44100）。</param>
        /// <param name="channelLayout">目标声道布局掩码（如 AV_CH_LAYOUT_STEREO）。</param>
        /// <returns>返回 0 表示设置成功。</returns>
        public int SettingsAudioFrame(NativeIntPtr<AVFrame> frame, FFmpegDecoderContext dContext, int sampleFormat, int sampleRate, ulong channelLayout)
        {
            if (frame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(frame));
            }
            AVFrame* f = frame;
            f->format = sampleFormat;
            f->ch_layout = *CreateChannelLayout(channelLayout).Value;
            f->sample_rate = sampleRate;
            return 0;
        }

        /// <summary> 将 FFmpeg 解码得到的音频帧数据（AVFrame）拷贝到托管字节数组，并返回实际数据长度。 该方法用于将 native PCM 数据从 FFmpeg
        /// 的 AVFrame 结构体中提取出来，便于后续播放或处理。 参数 targetChannel 通常为声道数（如 1=单声道，2=立体声），dataIndex 表示 FFmpeg
        /// 帧的 data 数组下标（一般为 0）。 注意：返回的字节数组来自共享内存池，使用完毕后应归还（ArrayPool<byte>.Shared.Return）。
        /// </summary> <param name="frame">音频帧指针（NativeIntPtr&lt;AVFrame&gt;），包含PCM 数据。</param>
        /// <param name="targetChannel">目标声道数（如1=单声道，2=立体声）。</param> <param
        /// name="length">输出参数，返回实际拷贝的数据长度（字节）。</param> <param name="dataIndex">FFmpeg 帧的
        /// data数组下标，默认 0。</param> <returns>包含 PCM 音频数据的字节数组。</returns>
        public byte[] CopyFrameToBuffer(NativeIntPtr<AVFrame> frame, long targetChannel, out int length, uint dataIndex = 0)
        {
            if (frame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(frame));
            }
            int dataSize = GetBufferSizeForSamples(frame);
            byte[] data = ArrayPool<byte>.Shared.Rent(dataSize);
            Marshal.Copy((IntPtr)frame.Value->data[dataIndex], data, 0, dataSize);
            length = dataSize;
            return data;
        }

        /// <summary>
        /// 获取指定帧的时间戳（毫秒）。
        /// </summary>
        /// <param name="framePtr">指定的帧</param>
        /// <returns>帧的时间戳</returns>
        /// <exception cref="ArgumentNullException">指定的帧为空时触发</exception>
        public long GetFrameTime(NativeIntPtr<AVFrame> framePtr)
        {
            if (framePtr.IsEmpty)
            {
                throw new ArgumentNullException(nameof(framePtr));
            }
            return framePtr.Value->pts;
        }

        /// <summary>
        /// 获取指定 AVFrame 的宽度（像素）。 适用于视频帧数据处理、渲染等场景。
        /// </summary>
        /// <param name="framePtr">视频帧指针封装。</param>
        /// <returns>帧的宽度（像素）。</returns>
        /// <exception cref="ArgumentNullException">framePtr 为空时抛出。</exception>
        public int GetFrameWidth(NativeIntPtr<AVFrame> framePtr)
        {
            if (framePtr.IsEmpty)
            {
                throw new ArgumentNullException(nameof(framePtr));
            }
            return framePtr.Value->width;
        }

        /// <summary>
        /// 获取指定 AVFrame 的高度（像素）。 适用于视频帧数据处理、渲染等场景。
        /// </summary>
        /// <param name="framePtr">视频帧指针封装。</param>
        /// <returns>帧的高度（像素）。</returns>
        /// <exception cref="ArgumentNullException">framePtr 为空时抛出。</exception>
        public int GetFrameHeight(NativeIntPtr<AVFrame> framePtr)
        {
            if (framePtr.IsEmpty)
            {
                throw new ArgumentNullException(nameof(framePtr));
            }
            return framePtr.Value->height;
        }

        /// <summary>
        /// 从帧队列中获取一个 AVFrame 实例，如果队列为空则自动创建新的 AVFrame。 用于解码、渲染等场景下的帧复用，减少频繁分配和释放内存，提高性能。 推荐在解码循环或帧处理流程中调用，避免重复分配帧资源。
        /// </summary>
        /// <returns>可用的 AVFrame 指针封装。</returns>
        public NativeIntPtr<AVFrame> GetFrame()
        {
            if (_frameQueue.TryDequeue(out var frame))
            {
                return frame;
            }
            return CreateFrame();
        }

        /// <summary>
        /// 回收一个 AVFrame 实例，将其放回帧队列以供后续复用。 会先清空帧内容（调用 FFmpeg 的 av_frame_unref），再入队，避免内存泄漏和脏数据。 推荐在帧处理完毕后调用，便于资源管理和性能优化。
        /// </summary>
        /// <param name="frame">需要被回收的 AVFrame 指针封装。</param>
        public void Return(ref NativeIntPtr<AVFrame> frame)
        {
            if (frame.IsEmpty)
            {
                return;
            }
            else if (_frameQueue.Count >= MaxCacheFrameCount)
            {
                Free(ref frame);
                return;
            }
            Flush(ref frame);
            _frameQueue.Enqueue(frame);
            frame = NativeIntPtr<AVFrame>.Empty;
        }

        #endregion Frame

        #region SwsContext

        /// <summary>
        /// 使用 FFmpeg sws_scale 对视频帧进行像素格式转换和缩放处理（全帧转换）。 按照 FFmpegInfo 提供的高度，将 srcFrame 的全部像素数据转换到
        /// dstFrame。 常用于解码后的视频渲染、帧导出或格式变换等场景。
        /// </summary>
        /// <param name="swsContext">像素格式转换上下文（SwsContext），由 CreateSwsContext 创建。</param>
        /// <param name="srcFrame">源帧指针，包含原始数据和行对齐信息。</param>
        /// <param name="dstFrame">目标帧指针，转换结果写入此帧。</param>
        /// <param name="info">媒体信息，提供转换所需的高度参数。</param>
        /// <exception cref="ArgumentNullException">任一参数为空时抛出异常。</exception>
        public void Scale(NativeIntPtr<SwsContext> swsContext, NativeIntPtr<AVFrame> srcFrame, NativeIntPtr<AVFrame> dstFrame, FFmpegInfo info)
        {
            Scale(swsContext, srcFrame, 0, info.Height, dstFrame);
        }

        /// <summary>
        /// 使用 FFmpeg sws_scale 对视频帧进行像素格式转换和缩放处理（支持分片转换）。
        /// 可指定源帧的起始行（srcSliceY）和转换高度（srcSliceH），将部分像素数据转换到目标帧。 适用于分片渲染、并行处理或自定义区域转换等高级场景。
        /// </summary>
        /// <param name="swsContext">像素格式转换上下文（SwsContext），由 CreateSwsContext 创建。</param>
        /// <param name="srcFrame">源帧指针，包含原始数据和行对齐信息。</param>
        /// <param name="srcSliceY">源帧起始行索引（0 ~ height-1）。</param>
        /// <param name="srcSliceH">转换的行数（必须大于0）。</param>
        /// <param name="dstFrame">目标帧指针，转换结果写入此帧。</param>
        /// <exception cref="ArgumentNullException">任一参数为空时抛出异常。</exception>
        /// <exception cref="ArgumentOutOfRangeException">srcSliceY 或 srcSliceH 参数超出有效范围时抛出异常。</exception>
        public void Scale(NativeIntPtr<SwsContext> swsContext, NativeIntPtr<AVFrame> srcFrame, int srcSliceY, int srcSliceH, NativeIntPtr<AVFrame> dstFrame)
        {
            if (swsContext.IsEmpty)
            {
                throw new ArgumentNullException(nameof(swsContext));
            }
            if (srcFrame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(srcFrame));
            }
            if (dstFrame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(dstFrame));
            }
            if (srcSliceH <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(srcSliceH), "必须大于0");
            }
            if (srcSliceY < 0 || srcSliceY >= srcFrame.Value->height)
            {
                throw new ArgumentOutOfRangeException(nameof(srcSliceY), $"必须在0到{srcFrame.Value->height - 1}之间");
            }
            ffmpeg.sws_scale(swsContext, srcFrame.Value->data, srcFrame.Value->linesize, srcSliceY, srcSliceH, dstFrame.Value->data, dstFrame.Value->linesize);
        }

        #endregion SwsContext

        #region SwrContext

        /// <summary>
        /// 初始化音频重采样上下文（SwrContext）。 在设置好输入/输出参数后，必须调用本方法以完成 SwrContext 的初始化， 使其可以用于音频格式转换和重采样操作。
        /// 内部调用 FFmpeg 的 swr_init 方法，若初始化失败则抛出异常。
        /// </summary>
        /// <param name="swrContext">音频重采样上下文指针。</param>
        /// <exception cref="ArgumentNullException">当 swrContext 为空时抛出。</exception>
        /// <exception cref="FFmpegException">初始化失败时抛出。</exception>
        public void SwrContextInit(NativeIntPtr<SwrContext> swrContext)
        {
            if (swrContext.IsEmpty)
            {
                throw new ArgumentNullException(nameof(swrContext));
            }
            int result = ffmpeg.swr_init(swrContext);
            if (result < 0)
            {
                ShowException("无法初始化 SwrContext", result);
            }
        }

        /// <summary>
        /// 设置并初始化音频重采样上下文（SwrContext）。 根据解码器上下文和解码器设置，配置 SwrContext 的输入输出参数（声道布局、采样格式、采样率）， 并调用
        /// SwrContextInit 完成初始化，使其可用于音频重采样和格式转换。
        /// </summary>
        /// <param name="swrContext">音频重采样上下文指针。</param>
        /// <param name="dContext">音频解码器上下文，提供输入参数。</param>
        /// <param name="settings">解码器设置，指定输出参数。</param>
        /// <exception cref="ArgumentNullException">当 swrContext 为空时抛出。</exception>
        public void SetSwrContextOptionsAndInit(NativeIntPtr<SwrContext> swrContext, FFmpegDecoderContext dContext, FFmpegDecoderSettings settings)
        {
            // 设置 SwrContext 的输入输出参数
            SetSwrContextOptions(
                swrContext,
                (ulong)settings.Channels,                  // 输出声道布局
                settings.SampleFormat.Convert(),   // 输出采样格式
                settings.SampleRate,                     // 输出采样率
                &dContext.CodecContext.Value->ch_layout, // 输入声道布局
                dContext.CodecContext.Value->sample_fmt, // 输入采样格式
                dContext.CodecContext.Value->sample_rate // 输入采样率
            );
            // 初始化 SwrContext
            SwrContextInit(swrContext);
        }

        /// <summary>
        /// 设置音频重采样上下文（SwrContext）的输入输出参数（支持声道布局掩码）。 根据指定的输出声道布局掩码、采样格式、采样率，以及输入声道布局、采样格式、采样率， 配置
        /// SwrContext 以支持多声道和多格式音频转换。 内部自动分配输出声道布局指针，异常时自动释放资源。
        /// </summary>
        /// <param name="swrContext">音频重采样上下文指针。</param>
        /// <param name="outChannelLayout">输出声道布局掩码（如 AV_CH_LAYOUT_STEREO）。</param>
        /// <param name="outSampleFormat">输出采样格式。</param>
        /// <param name="outSampleRate">输出采样率（Hz）。</param>
        /// <param name="inChannelLayout">输入声道布局指针。</param>
        /// <param name="inSampleFormat">输入采样格式。</param>
        /// <param name="inSampleRate">输入采样率（Hz）。</param>
        public void SetSwrContextOptions(NativeIntPtr<SwrContext> swrContext, ulong outChannelLayout, AVSampleFormat outSampleFormat, int outSampleRate, NativeIntPtr<AVChannelLayout> inChannelLayout, AVSampleFormat inSampleFormat, int inSampleRate)
        {
            NativeIntPtr<AVChannelLayout> outChannelLayoutPtr = CreateChannelLayout(outChannelLayout);
            try
            {
                SetSwrContextOptions(swrContext,
                    outChannelLayoutPtr,
                    outSampleFormat,
                    outSampleRate,
                    inChannelLayout,
                    inSampleFormat,
                    inSampleRate);
            }
            catch
            {
                Free(ref outChannelLayoutPtr);
                throw;
            }
        }

        /// <summary>
        /// 设置音频重采样上下文（SwrContext）的输入输出参数（支持声道布局指针）。 直接指定输出和输入的 AVChannelLayout 指针、采样格式和采样率，配置
        /// SwrContext 以支持多声道和多格式音频转换。 内部调用 FFmpeg 的 swr_alloc_set_opts2 方法完成参数设置。
        /// </summary>
        /// <param name="swrContext">音频重采样上下文指针。</param>
        /// <param name="outChannelLayout">输出声道布局指针。</param>
        /// <param name="outSampleFormat">输出采样格式。</param>
        /// <param name="outSampleRate">输出采样率（Hz）。</param>
        /// <param name="inChannelLayout">输入声道布局指针。</param>
        /// <param name="inSampleFormat">输入采样格式。</param>
        /// <param name="inSampleRate">输入采样率（Hz）。</param>
        /// <exception cref="ArgumentNullException">当 swrContext 为空时抛出。</exception>
        public void SetSwrContextOptions(NativeIntPtr<SwrContext> swrContext, NativeIntPtr<AVChannelLayout> outChannelLayout, AVSampleFormat outSampleFormat, int outSampleRate, NativeIntPtr<AVChannelLayout> inChannelLayout, AVSampleFormat inSampleFormat, int inSampleRate)
        {
            if (swrContext.IsEmpty)
            {
                throw new ArgumentNullException(nameof(swrContext));
            }

            SwrContext* swr = swrContext.Value;
            SwrContext** swrPtr = &swr;

            ffmpeg.swr_alloc_set_opts2(swrPtr,
                outChannelLayout,
                outSampleFormat,
                outSampleRate,
                inChannelLayout,
                inSampleFormat,
                inSampleRate,
                0,
                null);
        }

        /// <summary>
        /// 使用 FFmpeg 的 swr_convert_frame 方法进行音频重采样转换。 将输入 AVFrame 的音频数据转换为目标格式并写入输出 AVFrame。
        /// 适用于多声道、多采样率和多格式的音频转换场景。 调用前需确保 SwrContext 已正确初始化，且 inFrame/outFrame 参数均为有效音频帧。 转换失败时会抛出异常并显示详细错误信息。
        /// </summary>
        /// <param name="swrContext">音频重采样上下文指针（SwrContext），由 FFmpegEngine 创建和初始化。</param>
        /// <param name="outFrame">输出音频帧指针（AVFrame），用于存放转换后的音频数据。</param>
        /// <param name="inFrame">输入音频帧指针（AVFrame），包含原始音频数据。</param>
        /// <returns>实际转换的采样数，失败时抛出异常。</returns>
        /// <exception cref="ArgumentNullException">当 swrContext、inFrame 或 outFrame 为空时抛出。</exception>
        /// <exception cref="FFmpegException">转换失败时抛出，错误信息由 FFmpeg 返回。</exception>
        public int SwrConvert(NativeIntPtr<SwrContext> swrContext, NativeIntPtr<AVFrame> outFrame, NativeIntPtr<AVFrame> inFrame)
        {
            if (swrContext.IsEmpty)
            {
                throw new ArgumentNullException(nameof(swrContext));
            }
            if (inFrame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(inFrame));
            }
            if (outFrame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(outFrame));
            }

            int result = 0;
            result = ffmpeg.swr_convert_frame(swrContext, outFrame, inFrame);
            if (result < 0)
            {
                ShowException("SwrContext 转换失败", result);
            }
            return result;
        }

        /// <summary>
        /// 按指定采样率对音频帧样本数进行重采样比例换算。 使用 FFmpeg 的 av_rescale_rnd 方法，将 frame 的 nb_samples
        /// 按目标采样率（targetSampleRate）与当前解码器采样率进行比例缩放， 并可指定舍入方式（rounding），常用于音频重采样、时间戳换算等场景。
        /// </summary>
        /// <param name="frame">音频帧指针，包含原始样本数（nb_samples）。</param>
        /// <param name="targetSampleRate">目标采样率（Hz），用于换算比例。</param>
        /// <param name="context">解码器上下文，提供当前采样率。</param>
        /// <param name="rounding">舍入方式（AVRounding），默认为向上取整。</param>
        /// <returns>按比例换算后的样本数（long）。</returns>
        public long Rescale(NativeIntPtr<AVFrame> frame, long targetSampleRate, FFmpegDecoderContext context, AVRounding rounding = AVRounding.AV_ROUND_UP)
        {
            if (frame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(frame));
            }
            if (context.IsEmpty)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.CodecContext.Value->codec_type != AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                throw new ArgumentException("仅支持音频解码器上下文", nameof(context));
            }
            return ffmpeg.av_rescale_rnd(frame.Value->nb_samples, targetSampleRate, context.CodecContext.Value->sample_rate, rounding);
        }

        #endregion SwrContext

        #region ChannelLayout

        /// <summary>
        /// 将源 AVChannelLayout 结构体的数据复制到目标 AVChannelLayout 结构体。 用于在音频处理、重采样等场景下，安全地复制声道布局信息，
        /// 保证目标布局与源布局一致。内部调用 FFmpeg 的 av_channel_layout_copy 方法。
        /// </summary>
        /// <param name="src">源声道布局指针（NativeIntPtr&lt;AVChannelLayout&gt;）。</param>
        /// <param name="dst">目标声道布局指针（NativeIntPtr&lt;AVChannelLayout&gt;）。</param>
        /// <exception cref="ArgumentNullException">当 src 或 dst 为空时抛出。</exception>
        public void ChannelLayoutCopyTo(NativeIntPtr<AVChannelLayout> src, NativeIntPtr<AVChannelLayout> dst)
        {
            if (src.IsEmpty)
            {
                throw new ArgumentNullException(nameof(src));
            }
            if (dst.IsEmpty)
            {
                throw new ArgumentNullException(nameof(dst));
            }
            ffmpeg.av_channel_layout_copy(dst, src);
        }

        #endregion ChannelLayout

        #region Free

        /// <summary>
        /// 释放 FFmpegContext 相关资源，包括格式上下文、选项字典和解码器上下文集合。 用于确保所有底层指针和资源被正确释放，防止内存泄漏。
        /// </summary>
        /// <param name="context">待释放的 FFmpegContext 实例。</param>
        public void Free(ref FFmpegContext context)
        {
            if (context.IsEmpty)
            {
                return;
            }

            Free(ref context.FormatContext);
            Free(ref context.Options);
            Free(ref context.ContextCollection);
        }

        /// <summary>
        /// 释放 FFmpegDecoderContext 相关资源，包括解码器上下文、参数和流指针。 用于安全释放解码器相关的底层资源。
        /// </summary>
        /// <param name="decoder">待释放的 FFmpegDecoderContext 实例。</param>
        public void Free(ref FFmpegDecoderContext decoder)
        {
            if (decoder.IsEmpty)
            {
                return;
            }

            Free(ref decoder.CodecContext);
        }

        /// <summary>
        /// 释放 FFmpegDeciderContextCollection 集合中的所有解码器上下文资源。 会遍历集合并逐一释放每个解码器上下文。
        /// </summary>
        /// <param name="conllection">待释放的 FFmpegDeciderContextCollection 实例。</param>
        public void Free(ref FFmpegDecoderContextCollection conllection)
        {
            if (conllection.IsEmpty)
            {
                return;
            }

            foreach (var decoder in conllection)
            {
                var temp = decoder;
                Free(ref temp);
            }
        }

        /// <summary>
        /// 释放 AVCodecContext 指针资源。
        /// </summary>
        /// <param name="ptr">待释放的 AVCodecContext 指针封装。</param>
        public void Free(ref NativeIntPtr<AVCodecContext> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var valure = ptr.Value;
                var vptr = &valure;
                ffmpeg.avcodec_free_context(vptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 AVCodecParameters 指针资源。
        /// </summary>
        /// <param name="ptr">待释放的 AVCodecParameters 指针封装。</param>
        public void Free(ref NativeIntPtr<AVCodecParameters> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var valure = ptr.Value;
                var vptr = &valure;
                ffmpeg.avcodec_parameters_free(vptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 AVDictionary 指针资源。
        /// </summary>
        /// <param name="ptr">待释放的 AVDictionary 指针封装。</param>
        public void Free(ref NativeIntPtr<AVDictionary> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var valure = ptr.Value;
                var vptr = &valure;
                ffmpeg.av_dict_free(vptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 AVFormatContext 指针资源。
        /// </summary>
        /// <param name="ptr">待释放的 AVFormatContext 指针封装。</param>
        public void Free(ref NativeIntPtr<AVFormatContext> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                ffmpeg.avformat_free_context(ptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 AVFrame 指针资源。
        /// </summary>
        /// <param name="ptr">待释放的 AVFrame 指针封装。</param>
        public void Free(ref NativeIntPtr<AVFrame> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var valure = ptr.Value;
                var vptr = &valure;
                ffmpeg.av_frame_free(vptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 AVPacket 指针资源。
        /// </summary>
        /// <param name="ptr">需要被释放的指针资源</param>
        public void Free(ref NativeIntPtr<AVPacket> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var valure = ptr.Value;
                var vptr = &valure;
                ffmpeg.av_packet_free(vptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 SwsContext（图像像素格式转换上下文）资源。 用于释放由 FFmpeg sws_getContext 创建的像素格式转换上下文，防止内存泄漏。
        /// </summary>
        /// <param name="ptr">待释放的 SwsContext 指针封装。</param>
        public void Free(ref NativeIntPtr<SwsContext> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                ffmpeg.sws_freeContext(ptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 SwsFilter（图像滤镜）资源。 用于释放由 FFmpeg sws_getContext 创建的滤镜资源，防止内存泄漏。
        /// </summary>
        /// <param name="ptr">待释放的 SwsFilter 指针封装。</param>
        public void Free(ref NativeIntPtr<SwsFilter> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                ffmpeg.sws_freeFilter(ptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 AVFilterGraph（滤镜图）资源。 用于释放由 FFmpeg 创建的滤镜图结构，防止内存泄漏。
        /// </summary>
        /// <param name="ptr">待释放的 AVFilterGraph 指针封装。</param>
        public void Free(ref NativeIntPtr<AVFilterGraph> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var valure = ptr.Value;
                var vptr = &valure;
                ffmpeg.avfilter_graph_free(vptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放 SwrContext（音频重采样上下文）资源。
        /// </summary>
        /// <param name="ptr">需要被释放的音频重采样上下文的地址</param>
        public void Free(ref NativeIntPtr<SwrContext> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var valure = ptr.Value;
                var vptr = &valure;
                ffmpeg.swr_free(vptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 释放任意类型的非托管指针资源。 用于安全释放通过 FFmpeg 分配的结构体或缓冲区（如 AVChannelLayout、AVFrame 等），防止内存泄漏。 内部调用
        /// FFmpeg 的 av_freep 方法释放底层内存，并移除指针引用。 推荐在不再需要相关资源时调用，确保及时回收内存。
        /// </summary>
        /// <typeparam name="T">非托管类型（如 AVChannelLayout、AVFrame 等）。</typeparam>
        /// <param name="ptr">待释放的指针封装。</param>
        public void Free<T>(ref NativeIntPtr<T> ptr) where T : unmanaged
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var valure = ptr.Value;
                var vptr = &valure;
                ffmpeg.av_freep(vptr);
                ptr.Dispose();
            }
        }

        /// <summary>
        /// 检查指针是否有效并从内部集合移除。 用于辅助资源释放。
        /// </summary>
        /// <typeparam name="T">非托管类型。</typeparam>
        /// <param name="ptr">待检查的指针封装。</param>
        /// <returns>如果指针有效并已移除则返回 true，否则返回 false。</returns>
        private bool CheckIntPtrAndRemove<T>(NativeIntPtr<T> ptr) where T : unmanaged
        {
            if (ptr.IsEmpty)
            {
                return false;
            }
            _intPtrHashSet.Remove(ptr);
            return true;
        }

        #endregion Free

        #region Flush

        /// <summary>
        /// 刷新解码器上下文缓冲区，清空内部缓存的数据。 常用于解码器重置或切换流时，确保后续解码操作的正确性。
        /// </summary>
        /// <param name="ptr">待刷新的 AVCodecContext 指针封装。</param>
        public void Flush(ref NativeIntPtr<AVCodecContext> ptr)
        {
            if (ptr.IsEmpty)
            {
                return;
            }
            ffmpeg.avcodec_flush_buffers(ptr);
        }

        /// <summary>
        /// 刷新格式上下文缓冲区，清空内部缓存的数据。 常用于格式上下文重置或切换流时，确保后续操作的正确性。
        /// </summary>
        /// <param name="ptr">待刷新的 AVFormatContext 指针封装。</param>
        public void Flush(ref NativeIntPtr<AVFormatContext> ptr)
        {
            if (ptr.IsEmpty)
            {
                return;
            }
            ffmpeg.avformat_flush(ptr);
        }

        /// <summary>
        /// 清除数据包内容，释放其内部缓冲区。
        /// </summary>
        /// <param name="ptr">需要被清除的数据包地址</param>
        public void Flush(ref NativeIntPtr<AVPacket> ptr)
        {
            if (ptr.IsEmpty)
            {
                return;
            }
            ffmpeg.av_packet_unref(ptr);
        }

        /// <summary>
        /// 清除帧内容，释放其内部缓冲区。
        /// </summary>
        /// <param name="ptr">需要被清除的帧数据。</param>
        public void Flush(ref NativeIntPtr<AVFrame> ptr)
        {
            if (ptr.IsEmpty)
            {
                return;
            }
            ffmpeg.av_frame_unref(ptr);
        }

        /// <summary>
        /// 清空 AVChannelLayout 声道布局结构体的内容。 用于重置或释放声道布局相关的内部数据，防止脏数据影响后续音频处理。 内部调用 FFmpeg 的
        /// av_channel_layout_uninit 方法， 仅清空结构体内容，不释放指针本身的内存。 推荐在复用或回收 AVChannelLayout 结构体前调用。
        /// </summary>
        /// <param name="ptr">待清空的 AVChannelLayout 指针封装。</param>
        public void Flush(ref NativeIntPtr<AVChannelLayout> ptr)
        {
            if (ptr.IsEmpty)
            {
                return;
            }
            ffmpeg.av_channel_layout_uninit(ptr);
        }

        #endregion Flush

        #region Check

        /// <summary>
        /// 检查操作结果，如果失败则显示错误信息。
        /// </summary>
        /// <param name="result">需要被检查的操作结果</param>
        /// <returns>操作成功返回true，否者返回false</returns>
        public bool CheckResult(int result)
        {
            if (IsSuccess(result))
                return true;

            ShowException("FFmpeg操作失败", result);
            return false;
        }

        /// <summary>
        /// 判断操作是否成功。
        /// </summary>
        /// <param name="result">需要被判断的结果</param>
        /// <returns>成功返回true，否者返回false</returns>
        public bool IsSuccess(int result)
        {
            return result >= 0;
        }

        /// <summary>
        /// 判断是否为需要重试的错误
        /// </summary>
        /// <param name="result">操作结果</param>
        /// <returns>如果是需要重试的错误，则返回true；否则返回false</returns>
        public bool IsTryAgain(int result)
        {
            return result == ffmpeg.AVERROR(ffmpeg.EAGAIN);
        }

        /// <summary>
        /// 判断是否为文件结束的错误
        /// </summary>
        /// <param name="result">操作结果</param>
        /// <returns>如果是文件结束的错误，则返回true；否则返回false</returns>
        public bool IsEndOfFile(int result)
        {
            return result == ffmpeg.AVERROR_EOF;
        }

        #endregion Check

        #region Common

        /// <summary>
        /// 获取指定像素格式和媒体信息（FFmpegInfo）下的图像缓冲区所需字节数。 通常用于分配视频帧内存或判断缓冲区大小，底层调用 FFmpeg 的 av_image_get_buffer_size。
        /// </summary>
        /// <param name="pixelFormat">像素格式（AVPixelFormat），如 YUV420P、RGB24 等。</param>
        /// <param name="info">媒体信息，包含宽度和高度等参数。</param>
        /// <param name="align">对齐方式（字节），默认为 1。</param>
        /// <returns>所需的缓冲区字节数。</returns>
        /// <exception cref="ArgumentNullException">当 info 的宽度或高度为默认值时抛出。</exception>
        public int GetBufferSizeForImage(AVPixelFormat pixelFormat, FFmpegInfo info, int align = 1)
        {
            if (info.Width == FFmpegInfo.DefaultValue || info.Height == FFmpegInfo.DefaultValue)
            {
                throw new ArgumentNullException(nameof(info));
            }
            return ffmpeg.av_image_get_buffer_size(pixelFormat, info.Width, info.Height, align);
        }

        /// <summary>
        /// 获取指定像素格式和帧（AVFrame）下的图像缓冲区所需字节数。 通常用于分配视频帧内存或判断缓冲区大小，底层调用 FFmpeg 的 av_image_get_buffer_size。
        /// </summary>
        /// <param name="pixelFormat">像素格式（AVPixelFormat），如 YUV420P、RGB24 等。</param>
        /// <param name="frame">视频帧指针，包含宽度和高度等参数。</param>
        /// <param name="align">对齐方式（字节），默认为 1。</param>
        /// <returns>所需的缓冲区字节数。</returns>
        /// <exception cref="ArgumentNullException">当 frame 为空时抛出。</exception>
        public int GetBufferSizeForImage(AVPixelFormat pixelFormat, NativeIntPtr<AVFrame> frame, int align = 1)
        {
            if (frame.IsEmpty)
            {
                throw new ArgumentNullException(nameof(frame));
            }
            return ffmpeg.av_image_get_buffer_size(pixelFormat, frame.Value->width, frame.Value->height, align);
        }

        /// <summary>
        /// 获取指定像素格式、宽度、高度和对齐方式下的图像缓冲区所需字节数。 通常用于分配视频帧内存或判断缓冲区大小，底层调用 FFmpeg 的 av_image_get_buffer_size。
        /// </summary>
        /// <param name="pixelFormat">像素格式（AVPixelFormat），如 YUV420P、RGB24 等。</param>
        /// <param name="width">图像宽度（像素）。</param>
        /// <param name="height">图像高度（像素）。</param>
        /// <param name="align">对齐方式（字节），默认为 1。</param>
        /// <returns>所需的缓冲区字节数。</returns>
        public int GetBufferSizeForImage(AVPixelFormat pixelFormat, int width, int height, int align = 1)
        {
            return ffmpeg.av_image_get_buffer_size(pixelFormat, width, height, align);
        }

        /// <summary>
        /// 获取指定 AVFrame 音频帧的 PCM 数据缓冲区所需字节数。 该方法通过 FFmpeg 的 av_samples_get_buffer_size 计算实际音频数据长度，
        /// 适用于分配托管缓冲区或进行数据拷贝时的长度判断。 参数 align 通常为 1，表示按字节对齐。
        /// </summary>
        /// <param name="frame">音频帧指针（NativeIntPtr&lt;AVFrame&gt;），包含声道数、采样数和采样格式等信息。</param>
        /// <param name="align">对齐方式（字节），默认值为 1。</param>
        /// <returns>音频数据所需的总字节数。</returns>
        public int GetBufferSizeForSamples(NativeIntPtr<AVFrame> frame, int align = 1)
        {
            AVFrame* f = frame;
            return ffmpeg.av_samples_get_buffer_size(null,
                f->ch_layout.nb_channels,
                f->nb_samples,
                (AVSampleFormat)f->format,
                1);
        }

        /// <summary>
        /// 获取指定帧的时间戳（毫秒）。 此方法会根据解码器上下文中的流信息，优先使用帧的 PTS（显示时间戳），如果无效则使用 DTS（解码时间戳）。 时间戳会根据流的
        /// time_base 转换为毫秒，便于音视频同步和显示。
        /// </summary>
        /// <param name="framePtr">AVFrame 指针封装，包含帧的原始数据。</param>
        /// <param name="context">解码器上下文，提供流的时间基准。</param>
        /// <returns>当前帧的时间戳（单位：毫秒）。</returns>
        public long GetFrameTimestampMs(NativeIntPtr<AVFrame> framePtr, FFmpegDecoderContext context)
        {
            return GetFrameTimestampMs(framePtr, context.CodecStream);
        }

        /// <summary>
        /// 获取指定帧的时间戳（毫秒）。 优先使用 PTS（显示时间戳），如果无效则使用 DTS（解码时间戳）。 时间戳会根据流的 time_base 转换为毫秒，便于同步和显示。
        /// </summary>
        /// <param name="framePtr">AVFrame 指针封装，包含帧的原始数据。</param>
        /// <param name="streamPtr">AVStream 指针封装，包含流的时间基准。</param>
        /// <returns>当前帧的时间戳（单位：毫秒）。</returns>
        /// <exception cref="ArgumentNullException">framePtr 或 streamPtr 为空时抛出。</exception>
        public long GetFrameTimestampMs(NativeIntPtr<AVFrame> framePtr, NativeIntPtr<AVStream> streamPtr)
        {
            AVStream* stream = streamPtr;
            AVRational timeBase = stream->time_base;
            return GetFrameTimestampMs(framePtr, timeBase);
        }

        /// <summary>
        /// 获取指定帧的时间戳（毫秒）。 优先使用 PTS（显示时间戳），如果无效则使用 DTS（解码时间戳）。 时间戳会根据传入的
        /// time_base（流的时间基准）转换为毫秒，便于音视频同步和显示。 注意：视频流和音频流的 time_base 可以不同，需根据各自流的 time_base 进行换算，最终统一为标准时间单位（如毫秒）。
        /// </summary>
        /// <param name="framePtr">AVFrame 指针封装，包含帧的原始数据。</param>
        /// <param name="timeBase">流的时间基准（AVRational），用于时间戳换算。</param>
        /// <returns>当前帧的时间戳（单位：毫秒）。</returns>
        public long GetFrameTimestampMs(NativeIntPtr<AVFrame> framePtr, AVRational timeBase)
        {
            AVFrame* frame = framePtr;
            long timestamp = frame->pts != ffmpeg.AV_NOPTS_VALUE ? frame->pts : frame->pkt_dts;
            return ffmpeg.av_rescale_q(timestamp, timeBase, ffmpeg.av_make_q(1, 1000));
        }

        /// <summary>
        /// 获取指定 AVFrame 的持续时间（以毫秒为单位）。 持续时间原始值为流的时间基（time_base）单位，需结合流的时间基进行换算。 推荐使用本方法获取标准时间单位（毫秒），便于音视频同步和显示。
        /// </summary>
        /// <param name="framePtr">帧指针封装。</param>
        /// <param name="context">解码器上下文，提供流的时间基准。</param>
        /// <returns>帧的持续时间（单位：毫秒）。</returns>
        public long GetFrameDuration(NativeIntPtr<AVFrame> framePtr, FFmpegDecoderContext context)
        {
            return GetFrameDuration(framePtr, context.CodecStream);
        }

        /// <summary>
        /// 获取指定 AVFrame 的持续时间（以毫秒为单位）。 持续时间原始值为流的时间基（time_base）单位，需结合流的时间基进行换算。 推荐使用本方法获取标准时间单位（毫秒），便于音视频同步和显示。
        /// </summary>
        /// <param name="framePtr">帧指针封装。</param>
        /// <param name="streamPtr">流指针封装，包含时间基准。</param>
        /// <returns>帧的持续时间（单位：毫秒）。</returns>
        public long GetFrameDuration(NativeIntPtr<AVFrame> framePtr, NativeIntPtr<AVStream> streamPtr)
        {
            return GetFrameDuration(framePtr, streamPtr.Value->time_base);
        }

        /// <summary>
        /// 获取指定 AVFrame 的持续时间，并将其从流的时间基（time_base）单位转换为毫秒。 持续时间原始值为 time_base 单位，需通过 av_rescale_q 换算为标准时间单位（毫秒）。
        /// </summary>
        /// <param name="framePtr">帧指针封装。</param>
        /// <param name="timeBase">流的时间基准（AVRational）。</param>
        /// <returns>帧的持续时间（单位：毫秒）。</returns>
        public long GetFrameDuration(NativeIntPtr<AVFrame> framePtr, AVRational timeBase)
        {
            long duration = framePtr.Value->duration;
            // 转换为毫秒
            long durationMs = ffmpeg.av_rescale_q(duration, timeBase, ffmpeg.av_make_q(1, 1000));
            return durationMs;
        }

        #endregion Common

        #region Exception

        /// <summary>
        /// 显示并抛出 FFmpeg 错误信息。 根据 FFmpeg 错误码获取详细错误描述，并抛出自定义异常（FFmpegException）。
        /// </summary>
        /// <param name="errorCode">FFmpeg 返回的错误码。</param>
        /// <exception cref="FFmpegException">始终抛出，包含详细错误描述和错误码。</exception>
        public void ShowException(int errorCode)
        {
            throw GetException(errorCode);
        }

        /// <summary>
        /// 显示并抛出带有自定义消息的 FFmpeg 错误信息。 此方法会根据 FFmpeg 错误码获取标准错误描述，并将其与提供的自定义消息结合，最终抛出一个包含详细信息的 FFmpegException。
        /// </summary>
        /// <param name="message">自定义错误消息，将附加在 FFmpeg 标准错误描述之前。</param>
        /// <param name="errorCode">FFmpeg 返回的错误码。</param>
        /// <exception cref="FFmpegException">始终抛出，包含自定义消息和详细的 FFmpeg 错误描述。</exception>
        public void ShowException(string message, int errorCode)
        {
            FFmpegException ex = GetException(errorCode);
            throw new FFmpegException(message + "，" + ex.Message, ex);
        }

        /// <summary>
        /// 获取 FFmpeg 错误信息异常。 根据 FFmpeg 错误码获取详细错误描述，并封装为自定义异常（FFmpegException）。
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public FFmpegException GetException(int errorCode)
        {
            ulong errorBufferLength = 1024;
            byte[] errorBuffer = new byte[errorBufferLength];
            string errorMessage = string.Empty;
            fixed (byte* errorBufferPtr = errorBuffer)
            {
                ffmpeg.av_strerror(errorCode, errorBufferPtr, errorBufferLength);

                IntPtr ptr = (IntPtr)errorBufferPtr;
                errorMessage = Marshal.PtrToStringAnsi(ptr)!;
                Marshal.FreeHGlobal(ptr);
            }
            return new FFmpegException($"错误消息: {errorMessage} (错误代码: {errorCode})");
        }

        #endregion Exception

        protected override void DisposeUnmanagedResources()
        {
            while (_frameQueue.TryDequeue(out var frame))
            {
                Free(ref frame);
            }
            while (_packetQueue.TryDequeue(out var packet))
            {
                Free(ref packet);
            }
            ffmpeg.avformat_network_deinit();
        }
    }
}