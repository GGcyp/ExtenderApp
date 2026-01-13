using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.FFmpegEngines.Medias.Outputs;

namespace ExtenderApp.FFmpegEngines.Medias
{
    /// <summary>
    /// 提供 MediaPlayer 的扩展方法，用于简化音视频输出的配置。
    /// </summary>
    public static class MediaPlayerExpansions
    {
        /// <summary>
        /// 为 MediaPlayer 设置视频输出，并返回用于显示的 BitmapSource。
        /// </summary>
        /// <param name="mediaPlayer">目标 MediaPlayer 实例。</param>
        /// <param name="dpix">水平 DPI，默认值为 96。</param>
        /// <param name="dpiy">垂直 DPI，默认值为 96。</param>
        /// <returns>绑定到视频输出缓冲区的 BitmapSource，可直接用于 UI 绑定。</returns>
        /// <exception cref="ArgumentNullException">当 mediaPlayer 为 null 时抛出。</exception>
        public static BitmapSource SetVideoOutput(this IMediaPlayer mediaPlayer, int dpix = 96, int dpiy = 96)
        {
            if (mediaPlayer is null)
                throw new ArgumentNullException(nameof(mediaPlayer));
            VideoOutput videoOutput = new(mediaPlayer.Info.Width, mediaPlayer.Info.Height, dpix, dpiy, mediaPlayer.Settings.PixelFormat.ToPixelFormat(), null);
            mediaPlayer.FrameProcessCollection.AddMediaOutput(videoOutput);
            return videoOutput.NativeMemoryBitmap;
        }

        /// <summary>
        /// 为 MediaPlayer 设置默认的音频输出。 使用 MediaPlayer 当前的解码设置初始化音频播放器。
        /// </summary>
        /// <param name="mediaPlayer">目标 MediaPlayer 实例。</param>
        /// <exception cref="ArgumentNullException">当 mediaPlayer 为 null 时抛出。</exception>
        public static void SetAudioOutput(this IMediaPlayer mediaPlayer)
        {
            if (mediaPlayer is null)
                throw new ArgumentNullException(nameof(mediaPlayer));
            AudioOutput audioOutput = new(mediaPlayer.Settings);
            mediaPlayer.FrameProcessCollection.AddMediaOutput(audioOutput);
        }

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

        /// <summary>
        /// 获取 MediaPlayer 当前的视频帧作为 BitmapSource。
        /// </summary>
        /// <param name="mediaPlayer">指定的媒体播放器</param>
        /// <returns>当前视频帧的 BitmapSource，如果没有视频输出则返回 null。</returns>
        public static BitmapSource? GetBitmapSource(this IMediaPlayer mediaPlayer)
        {
            var videoOutput = mediaPlayer.FrameProcessCollection.GetMediaOutput(FFmpegMediaType.VIDEO) as VideoOutput;
            if (videoOutput == null)
                return null;
            return videoOutput.NativeMemoryBitmap;
        }
    }
}