using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.Data;
using ExtenderApp.Views;

namespace ExtenderApp.FFmpegEngines.Medias.Outputs
{
    /// <summary>
    /// 视频输出接口实现类
    /// </summary>
    internal class VideoOutput : DisposableObject, IVideoOutput
    {
        public FFmpegMediaType MediaType => FFmpegMediaType.VIDEO;

        public NativeMemoryBitmap NativeMemoryBitmap { get; }

        public VideoOutput(int width, int height, int dpix, int dpiy, PixelFormat format, BitmapPalette palette)
        {
            NativeMemoryBitmap = new(width, height, dpix, dpiy, format, palette);
        }

        public void PlayerStateChange(PlayerState state)
        {
        }

        public void WriteFrame(FFmpegFrame frame)
        {
            NativeMemoryBitmap.Write(frame.Block.UnreadSpan);
            NativeMemoryBitmap.UpdateBitmap();
        }

        protected override void DisposeManagedResources()
        {
            NativeMemoryBitmap.Dispose();
        }
    }
}