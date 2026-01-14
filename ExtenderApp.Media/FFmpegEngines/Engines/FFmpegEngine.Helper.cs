using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace ExtenderApp.FFmpegEngines
{
    public unsafe partial class FFmpegEngine
    {
        /// <summary>
        /// 根据解码器设置和媒体信息，计算视频帧的行跨度（Stride，单位：字节）。
        /// 行跨度用于表示一行像素在内存中的实际字节数，常用于图像处理和视频帧数据读取。
        /// </summary>
        /// <param name="settings">解码器设置，包含像素格式等信息。</param>
        /// <param name="info">媒体信息，包含视频宽度等参数。</param>
        /// <returns>视频帧的行跨度（字节数）。</returns>
        public static int GetStride(FFmpegDecoderSettings settings, FFmpegInfo info)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            return GetStride(settings.PixelFormat, info.Width, 0);
        }

        /// <summary>
        /// 根据像素格式、图像宽度和分量（平面）计算视频帧的行跨度（Stride，单位：字节）。
        /// 行跨度用于表示一行像素在内存中的实际字节数，常用于多平面格式（如YUV）下的图像处理。
        /// </summary>
        /// <param name="pixelFormat">FFmpegPixelFormat 枚举值，指定像素格式。</param>
        /// <param name="width">图像宽度（像素）。</param>
        /// <param name="plane">图像的分量（如YUV格式下的Y/U/V平面）默认为零。</param>
        /// <returns>指定平面的一行像素实际占用的字节数。</returns>
        public static int GetStride(FFmpegPixelFormat pixelFormat, int width, int plane = 0)
        {
            return GetStride(pixelFormat.Convert(), width, plane);
        }

        /// <summary>
        /// 根据像素格式和宽度计算视频帧的行跨度（Stride，单位：字节）。
        /// </summary>
        /// <param name="pixelFormat">像素格式。</param>
        /// <param name="width">图像宽度（像素）。</param>
        /// <param name="plane">图像的分量（分平面）默认为零。</param>
        /// <returns>行跨度（字节数）。</returns>
        public static int GetStride(AVPixelFormat pixelFormat, int width, int plane = 0)
        {
            return ffmpeg.av_image_get_linesize(pixelFormat, width, plane);
        }

        /// <summary>
        /// 获取指定像素格式的每像素字节数。
        /// </summary>
        /// <param name="pixelFormat">FFmpegPixelFormat 枚举值。</param>
        /// <returns>每像素字节数。</returns>
        public static int GetBytesPerPixel(FFmpegPixelFormat pixelFormat)
        {
            return GetBytesPerPixel(pixelFormat.Convert());
        }

        /// <summary>
        /// 获取指定像素格式的每像素字节数。
        /// </summary>
        /// <param name="pixelFormat">AVPixelFormat 枚举值。</param>
        /// <returns>每像素字节数。</returns>
        public static int GetBytesPerPixel(AVPixelFormat pixelFormat)
        {
            return ffmpeg.av_get_bits_per_pixel(ffmpeg.av_pix_fmt_desc_get(pixelFormat)) / 8;
        }

        /// <summary>
        /// 获取解码器设置中像素格式的每像素字节数。
        /// </summary>
        /// <param name="settings">解码器设置。</param>
        /// <returns>每像素字节数。</returns>
        public static int GetBytesPerPixel(FFmpegDecoderSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            return GetBytesPerPixel(settings.PixelFormat);
        }

        /// <summary>
        /// 获取媒体信息中像素格式的每像素字节数。
        /// </summary>
        /// <param name="info">媒体信息。</param>
        /// <returns>每像素字节数。</returns>
        public static int GetBytesPerPixel(FFmpegInfo info)
        {
            return GetBytesPerPixel(info.PixelFormat);
        }

        /// <summary>
        /// 获取指定音频采样格式的每采样点字节数。
        /// </summary>
        /// <param name="sampleFormat">FFmpegSampleFormat 枚举值，表示音频采样格式（如 S16、FLT 等）。</param>
        /// <returns>每采样点的字节数。例如，S16 返回 2，FLT 返回 4。</returns>
        public static int GetBytesPerSample(FFmpegSampleFormat sampleFormat)
        {
            return GetBytesPerSample(sampleFormat.Convert());
        }

        /// <summary>
        /// 获取指定 FFmpeg 原生音频采样格式的每采样点字节数。
        /// </summary>
        /// <param name="sampleFormat">AVSampleFormat 枚举值，表示 FFmpeg 的原生音频采样格式。</param>
        /// <returns>每采样点的字节数。例如，AV_SAMPLE_FMT_S16 返回 2。</returns>
        public static int GetBytesPerSample(AVSampleFormat sampleFormat)
        {
            return ffmpeg.av_get_bytes_per_sample(sampleFormat);
        }

        /// <summary>
        /// 获取解码器设置中音频采样格式的每采样点字节数。
        /// </summary>
        /// <param name="settings">解码器设置，包含音频采样格式信息。</param>
        /// <returns>每采样点的字节数。</returns>
        public static int GetBytesPerSample(FFmpegDecoderSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            return GetBytesPerSample(settings.SampleFormat);
        }

        /// <summary>
        /// 获取媒体信息中音频采样格式的每采样点字节数。
        /// </summary>
        /// <param name="info">媒体信息，包含音频采样格式信息。</param>
        /// <returns>每采样点的字节数。</returns>
        public static int GetBytesPerSample(FFmpegInfo info)
        {
            return GetBytesPerSample(info.SampleFormat);
        }

        /// <summary>
        /// 将AVMediaType转换为字符串表示。
        /// </summary>
        /// <param name="mediaType">AVMediaType枚举值。</param>
        /// <returns>字符串表示。</returns>
        public static string MediaTypeToString(AVMediaType mediaType)
        {
            return ffmpeg.av_get_media_type_string(mediaType);
        }

        /// <summary>
        /// 判断指定采样格式是否为 planar（分平面）格式。
        /// <para>
        /// 内部调用 FFmpeg 的 <c>av_sample_fmt_is_planar</c>：
        /// 通常返回 1 表示 planar，返回 0 表示 packed（或非 planar）。
        /// </para>
        /// </summary>
        /// <param name="sampleFormat">FFmpeg 采样格式枚举。</param>
        /// <returns>如果是 planar 返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
        public static bool IsPlanar(AVSampleFormat sampleFormat)
        {
            return ffmpeg.av_sample_fmt_is_planar(sampleFormat) == 1;
        }

        #region IO

        /// <summary>
        /// FFmpeg <see cref="AVIOContext"/> 的“读数据”回调（read_packet）。
        /// <para>
        /// 当 <c>avio_alloc_context</c> 创建的自定义 IO 被绑定到 <c>AVFormatContext->pb</c> 后，
        /// FFmpeg 在需要更多输入字节时会调用该函数。
        /// </para>
        /// <para>
        /// 约定（与 FFmpeg C API 保持一致）：
        /// <list type="bullet">
        /// <item><description>返回值 &gt; 0：实际读取的字节数。</description></item>
        /// <item><description>返回值 = <see cref="ffmpeg.AVERROR_EOF"/>：数据源结束（EOF）。</description></item>
        /// <item><description>返回值 &lt; 0：错误码（AVERROR(...)）。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="opaque">FFmpeg 侧 opaque 指针（此处为托管对象的 <see cref="GCHandle"/>）。</param>
        /// <param name="buf">FFmpeg 提供的目标缓冲区指针，需将读到的数据写入其中。</param>
        /// <param name="bufSize">FFmpeg 请求的最大读取字节数。</param>
        /// <returns>读取到的字节数，或 FFmpeg 约定的负错误码/EOF。</returns>
        private static int StreamReadPacket(void* opaque, byte* buf, int bufSize)
        {
            try
            {
                Stream stream = GetStream(opaque);

                Span<byte> span = new(buf, bufSize);
                int read = stream.Read(span);

                // 自定义IO的约定：读到0表示EOF
                if (read == 0)
                {
                    return ffmpeg.AVERROR_EOF;
                }

                return read;
            }
            catch (IOException)
            {
                // 不依赖平台 errno（例如 EIO）；统一返回 FFmpeg 内置“未知错误”
                return ffmpeg.AVERROR_UNKNOWN;
            }
            catch
            {
                return ffmpeg.AVERROR_UNKNOWN;
            }
        }

        /// <summary>
        /// FFmpeg <see cref="AVIOContext"/> 的“写数据”回调（write_packet）。
        /// <para>
        /// 当自定义 IO 用于输出（mux/encode，<c>writeFlag=1</c>）时，FFmpeg 会回调该函数要求写入数据。
        /// </para>
        /// <para>
        /// 返回值语义：
        /// <list type="bullet">
        /// <item><description>返回值 &gt;= 0：写入的字节数（通常等于 <paramref name="bufSize"/>）。</description></item>
        /// <item><description>返回值 &lt; 0：负错误码（AVERROR(...)）。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="opaque">FFmpeg 侧 opaque 指针（此处为托管对象的 <see cref="GCHandle"/>）。</param>
        /// <param name="buf">FFmpeg 提供的源缓冲区指针，包含待写入的数据。</param>
        /// <param name="bufSize">待写入的字节数。</param>
        /// <returns>实际写入的字节数；失败返回负错误码。</returns>
        private static int StreamWritePacket(void* opaque, byte* buf, int bufSize)
        {
            try
            {
                Stream stream = GetStream(opaque);
                Span<byte> span = new(buf, bufSize);
                stream.Write(span);
                return bufSize;
            }
            catch
            {
                return ffmpeg.AVERROR_UNKNOWN;
            }
        }

        /// <summary>
        /// FFmpeg <see cref="AVIOContext"/> 的 Seek 回调（seek）。
        /// <para>
        /// 用于支持随机访问：FFmpeg 可能会在探测、读取索引、跳转（seek）等场景调用。
        /// 若数据源不支持随机访问，应返回 -1（FFmpeg 会按不可 seek 的“实时流”方式处理）。
        /// </para>
        /// <para>
        /// 特殊约定：当 <paramref name="whence"/> 等于 <see cref="ffmpeg.AVSEEK_SIZE"/> 时，表示 FFmpeg 查询数据总长度，
        /// 此时应返回数据源长度（字节），若未知则返回 -1。
        /// </para>
        /// <para>
        /// 其余 whence 值与标准 C 的 <c>SEEK_*</c> 含义一致：
        /// <list type="bullet">
        /// <item><description>0：SEEK_SET（从起始位置）</description></item>
        /// <item><description>1：SEEK_CUR（从当前位置）</description></item>
        /// <item><description>2：SEEK_END（从末尾位置）</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="opaque">FFmpeg 侧 opaque 指针（此处为托管对象的 <see cref="GCHandle"/>）。</param>
        /// <param name="offset">目标偏移（字节）。</param>
        /// <param name="whence">定位方式（SEEK_SET/SEEK_CUR/SEEK_END 或 AVSEEK_SIZE）。</param>
        /// <returns>定位后的绝对位置；不支持或失败返回 -1。</returns>
        private static long StreamSeek(void* opaque, long offset, int whence)
        {
            try
            {
                Stream stream = GetStream(opaque);

                // FFmpeg 查询长度
                if (whence == ffmpeg.AVSEEK_SIZE)
                {
                    return stream.CanSeek ? stream.Length : -1;
                }

                if (!stream.CanSeek)
                {
                    return -1;
                }

                SeekOrigin origin = whence switch
                {
                    0 => SeekOrigin.Begin,   // SEEK_SET
                    1 => SeekOrigin.Current, // SEEK_CUR
                    2 => SeekOrigin.End,     // SEEK_END
                    _ => SeekOrigin.Begin,
                };

                return stream.Seek(offset, origin);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 从 FFmpeg 的 opaque 指针中取回托管侧绑定的 <see cref="Stream"/>。
        /// <para>
        /// opaque 来自创建 <see cref="AVIOContext"/> 时传入的 <see cref="GCHandle.ToIntPtr(GCHandle)"/>。
        /// </para>
        /// </summary>
        /// <param name="opaque">FFmpeg 侧透传的 opaque 指针。</param>
        /// <returns>回调绑定的 <see cref="Stream"/> 实例。</returns>
        private static Stream GetStream(void* opaque)
        {
            var handle = GCHandle.FromIntPtr((nint)opaque);
            return (Stream)handle.Target!;
        }

        #endregion IO
    }
}