using System.Windows.Media;

namespace ExtenderApp.FFmpegEngines
{
    public static class FFmpegPixelFormatWpfExtensions
    {
        /// <summary>
        /// 将 FFmpegPixelFormat 转换为 WPF 的 PixelFormat。
        /// </summary>
        /// <param name="format">FFmpeg 像素格式。</param>
        /// <returns>对应的 WPF PixelFormat，如果不支持则返回 PixelFormats.Default。</returns>
        public static PixelFormat ToPixelFormat(this FFmpegPixelFormat format)
        {
            return format switch
            {
                FFmpegPixelFormat.PIX_FMT_BGR24 => PixelFormats.Bgr24,
                FFmpegPixelFormat.PIX_FMT_RGB24 => PixelFormats.Rgb24,
                FFmpegPixelFormat.PIX_FMT_BGRA => PixelFormats.Bgra32,
                FFmpegPixelFormat.PIX_FMT_BGR555LE => PixelFormats.Bgr555,
                FFmpegPixelFormat.PIX_FMT_BGR565LE => PixelFormats.Bgr565,
                FFmpegPixelFormat.PIX_FMT_GRAY8 => PixelFormats.Gray8,
                FFmpegPixelFormat.PIX_FMT_GRAY16LE => PixelFormats.Gray16,
                FFmpegPixelFormat.PIX_FMT_RGBA64LE => PixelFormats.Rgba64,
                FFmpegPixelFormat.PIX_FMT_RGB48LE => PixelFormats.Rgb48,
                _ => PixelFormats.Default
            };
        }
    }
}
