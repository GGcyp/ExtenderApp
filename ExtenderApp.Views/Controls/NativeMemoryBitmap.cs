using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 一个封装了 WriteableBitmap 和原生内存块的类，支持在后台线程中写入像素数据，并在UI线程上进行安全的更新。
    /// </summary>
    public sealed class NativeMemoryBitmap : DisposableObject
    {
        private readonly object _lock;
        private readonly Action _updateBitmapAction;

        public WriteableBitmap Bitmap { get; }

        private NativeByteMemory block;

        // 0=没有UI刷新排队，1=已有UI刷新排队（合并多次请求，只保留一次UI回调）
        private int _uiUpdateQueued;

        public NativeMemoryBitmap(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette? palette = null)
        {
            Bitmap = new(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette);

            int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
            int stride = pixelWidth * bytesPerPixel;
            block = new(stride * pixelHeight);

            _lock = new();
            _updateBitmapAction = UpdateBitmapOnUiThread;
        }

        /// <summary>
        /// 请求刷新：多次调用会被合并，UI线程只会执行一次，并显示执行时刻的“最新一帧”。
        /// </summary>
        public void UpdateBitmap()
        {
            if (Bitmap.Dispatcher.CheckAccess())
            {
                UpdateBitmapOnUiThread();
                return;
            }

            // 若已排队则不重复排队；从而丢弃中间帧，只保留最后一次刷新请求
            if (Interlocked.Exchange(ref _uiUpdateQueued, 1) == 1)
                return;

            Bitmap.Dispatcher.InvokeAsync(_updateBitmapAction);
        }

        private unsafe void UpdateBitmapOnUiThread()
        {
            // 允许下一次排队（无论是否成功刷新，都要释放“排队锁”）
            Interlocked.Exchange(ref _uiUpdateQueued, 0);

            lock (_lock)
            {
                if (IsDisposed)
                    return;

                Bitmap.WritePixels(
                    new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight),
                    (nint)block.Ptr,
                    block.Length,
                    Bitmap.BackBufferStride);
            }
        }

        public void Write(ReadOnlySpan<byte> source)
        {
            lock (_lock)
            {
                source.CopyTo(block);
            }
        }

        protected override void DisposeManagedResources()
        {
            lock (_lock)
            {
                block.Dispose();
            }
        }

        public static implicit operator BitmapSource(NativeMemoryBitmap memoryBitmap)
            => memoryBitmap.Bitmap;
    }
}