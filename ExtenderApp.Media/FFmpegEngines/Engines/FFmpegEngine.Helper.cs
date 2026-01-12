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
    }
}