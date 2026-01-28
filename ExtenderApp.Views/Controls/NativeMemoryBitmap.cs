using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.Common;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 一个封装了 WriteableBitmap 和原生内存块的类，支持在后台线程中写入像素数据，
    /// 并在 UI 线程上以与渲染同步的方式安全更新（使用 CompositionTarget.Rendering 拉取）。
    /// </summary>
    public sealed class NativeMemoryBitmap : DisposableObject
    {
        private readonly object _lock;
        private readonly Action _updateBitmapAction;

        public WriteableBitmap Bitmap { get; }

        private NativeByteMemory block;

        // 0 = 无待刷帧；1 = 有待刷帧（由写线程设置）
        private int _hasPendingFrame;

        // 是否已订阅 CompositionTarget.Rendering（UI 线程访问）
        private bool _renderSubscribed;

        public NativeMemoryBitmap(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette? palette = null)
        {
            Bitmap = new(pixelWidth, pixelHeight, dpiX, dpiY, pixelFormat, palette);

            int bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;
            int stride = pixelWidth * bytesPerPixel;
            block = new(stride * pixelHeight);

            _lock = new();
            _updateBitmapAction = UpdateBitmapOnUiThread;

            // 延迟订阅：在构造时也可以订阅一次，或在第一次写入时在 UI 线程订阅。
            // 为简洁在构造时确保订阅（CompositionTarget.Rendering 在 UI 线程触发）。
            EnsureRenderingSubscription();
        }

        private void EnsureRenderingSubscription()
        {
            // 订阅必须在 UI 线程执行，使用 Bitmap.Dispatcher 检查/调度
            if (Bitmap.Dispatcher.CheckAccess())
            {
                if (!_renderSubscribed)
                {
                    CompositionTarget.Rendering += OnRendering;
                    _renderSubscribed = true;
                }
            }
            else
            {
                Bitmap.Dispatcher.InvokeAsync(() =>
                {
                    if (!_renderSubscribed)
                    {
                        CompositionTarget.Rendering += OnRendering;
                        _renderSubscribed = true;
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
        }

        /// <summary>
        /// 后台/生产线程写入像素数据到本地原生缓冲，并打标志，实际刷新由 UI 渲染循环拉取。
        /// </summary>
        public void Write(ReadOnlySpan<byte> source)
        {
            lock (_lock)
            {
                source.CopyTo(block);
            }

            // 标记有待刷新帧（线程安全）
            Interlocked.Exchange(ref _hasPendingFrame, 1);
        }

        /// <summary>
        /// 每次 WPF 渲染循环回调（UI 线程），若存在待刷帧则执行刷新。
        /// </summary>
        private void OnRendering(object? sender, EventArgs e)
        {
            // 快速检查标志并归零；只有在标志为 1 时才执行刷新
            if (Interlocked.Exchange(ref _hasPendingFrame, 0) == 1)
            {
                // 在 UI 线程直接刷新
                _updateBitmapAction();
            }
        }

        private unsafe void UpdateBitmapOnUiThread()
        {
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

        protected override void DisposeManagedResources()
        {
            // 取消渲染订阅（UI 线程）
            try
            {
                if (Bitmap.Dispatcher.CheckAccess())
                {
                    if (_renderSubscribed)
                    {
                        CompositionTarget.Rendering -= OnRendering;
                        _renderSubscribed = false;
                    }
                }
                else
                {
                    Bitmap.Dispatcher.InvokeAsync(() =>
                    {
                        if (_renderSubscribed)
                        {
                            CompositionTarget.Rendering -= OnRendering;
                            _renderSubscribed = false;
                        }
                    }, System.Windows.Threading.DispatcherPriority.Normal).Task.Wait();
                }
            }
            catch
            {
                // 忽略任何异常取消订阅
            }

            block.DisposeSafe();
        }

        public static implicit operator BitmapSource(NativeMemoryBitmap memoryBitmap)
            => memoryBitmap.Bitmap;
    }
}