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
        /// <summary>
        /// 用于确保对原生内存块进行线程安全访问的锁对象。
        /// </summary>
        private readonly object _lock;

        /// <summary>
        /// 更新位图的委托。
        /// </summary>
        private readonly Action _updateBitmapAction;

        /// <summary>
        /// 获取可写的位图对象。
        /// </summary>
        public WriteableBitmap Bitmap { get; }

        /// <summary>
        /// 用于存储像素数据的非托管内存块。
        /// </summary>
        private NativeByteMemory block;

        public NativeMemoryBitmap(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette? palette = null)
        {
            Bitmap = new(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette);

            int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
            int stride = pixelWidth * bytesPerPixel;
            block = new(stride * pixelHeight);

            _lock = new();
            _updateBitmapAction = UpdateBitmap;
        }

        /// <summary>
        /// 将非托管内存中的像素数据更新到 WriteableBitmap。
        /// 此方法是线程安全的，可以从任何线程调用。如果从非UI线程调用，它会自动将更新操作调度到UI线程。
        /// </summary>
        public unsafe void UpdateBitmap()
        {
            // 检查是否在UI线程上
            if (Bitmap.Dispatcher.CheckAccess())
            {
                // 如果在UI线程，直接更新
                lock (_lock)
                {
                    Bitmap.WritePixels(
                        new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight),
                        (nint)block.Ptr,
                        block.Length,
                        Bitmap.BackBufferStride);
                }
            }
            else
            {
                // 如果在后台线程，则异步调度到UI线程执行
                Bitmap.Dispatcher.InvokeAsync(_updateBitmapAction);
            }
        }

        /// <summary>
        /// 将提供的 Span 数据写入到位图的非托管内存缓冲区。
        /// 此方法是线程安全的，可以从多个线程并发调用。
        /// </summary>
        /// <param name="source">包含像素数据的源 Span。</param>
        public void Write(ReadOnlySpan<byte> source)
        {
            lock (_lock)
            {
                block.Write(source);
            }
        }

        /// <summary>
        /// 释放由该实例持有的所有托管资源。
        /// </summary>
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