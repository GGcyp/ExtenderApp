using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
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
        /// 根据像素格式和宽度计算视频帧的行跨度（Stride，单位：字节）。
        /// </summary>
        /// <param name="pixelFormat">像素格式。</param>
        /// <param name="width">图像宽度（像素）。</param>
        /// <returns>行跨度（字节数）。</returns>
        public static int GetStride(AVPixelFormat pixelFormat, int width)
        {
            return GetStride(pixelFormat, width, 0);
        }

        /// <summary>
        /// 根据像素格式和宽度计算视频帧的行跨度（Stride，单位：字节）。
        /// </summary>
        /// <param name="pixelFormat">像素格式。</param>
        /// <param name="width">图像宽度（像素）。</param>
        /// <param name="plane">图像的分量（分平面）。</param>
        /// <returns>行跨度（字节数）。</returns>
        public static int GetStride(AVPixelFormat pixelFormat, int width, int plane)
        {
            return ffmpeg.av_image_get_linesize(pixelFormat, width, plane);
        }
    }
}
