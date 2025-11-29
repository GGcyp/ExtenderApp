using System.IO.MemoryMappedFiles;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 表示一个使用共享内存（内存映射文件）的高性能位图源。 此类允许后台线程直接写入像素数据，而UI线程可以高效地渲染，适用于视频播放等场景。
    /// </summary>
    public sealed class SharedMemoryBitmap : BitmapSource, IDisposable
    {
        /// <summary>
        /// 默认DPI值。
        /// </summary>
        private const double DefaultDpi = 96.0d;

        /// <summary>
        /// 内存映射文件对象，表示共享内存块。
        /// </summary>
        private readonly MemoryMappedFile? _mmf;

        /// <summary>
        /// 内存映射视图访问器，用于访问共享内存的数据。
        /// </summary>
        private readonly MemoryMappedViewAccessor? _viewAccessor;

        /// <summary>
        /// 视图位图，用于与WPF图像系统交互。
        /// </summary>
        private readonly InteropBitmap _interopBitmap;

        /// <summary>
        /// 数据长度（以字节为单位）。
        /// </summary>
        private readonly int _dataLength;

        /// <summary>
        /// 获取指向共享内存块的指针，后台线程可向此地址写入像素数据。
        /// </summary>
        public IntPtr BitmapBuffer { get; private set; }

        /// <summary>
        /// 获取图像一行的字节数（也称为步幅或跨距）。
        /// </summary>
        public int Stride { get; }

        #region BitmapSource Overrides

        /// <summary>
        /// 获取位图的宽度（以像素为单位）。
        /// </summary>
        public override int PixelWidth { get; }

        /// <summary>
        /// 获取位图的高度（以像素为单位）。
        /// </summary>
        public override int PixelHeight { get; }

        /// <summary>
        /// 获取位图的水平DPI。
        /// </summary>
        public override double DpiX { get; }

        /// <summary>
        /// 获取位图的垂直DPI。
        /// </summary>
        public override double DpiY { get; }

        /// <summary>
        /// 获取位图的像素格式。
        /// </summary>
        public override PixelFormat Format { get; }

        /// <summary>
        /// 获取位图的调色板。对于非索引格式，此项为null。
        /// </summary>
        public override BitmapPalette? Palette => null;

        #endregion BitmapSource Overrides

        /// <summary>
        /// 初始化一个新的 <see
        /// cref="SharedMemoryBitmap"/> 实例。
        /// </summary>
        /// <param name="pixelWidth">位图的宽度（像素）。</param>
        /// <param name="pixelHeight">位图的高度（像素）。</param>
        /// <param name="pixelFormat">位图的像素格式。</param>
        public SharedMemoryBitmap(int pixelWidth, int pixelHeight, PixelFormat pixelFormat) : this(pixelWidth, pixelHeight, DefaultDpi, DefaultDpi, pixelFormat)
        {
        }

        /// <summary>
        /// 生成一个新的 <see
        /// cref="SharedMemoryBitmap"/> 实例，指定宽度、高度、像素格式和DPI。
        /// </summary>
        /// <param name="pixelWidth">位图的宽度（像素）。</param>
        /// <param name="pixelHeight">位图的高度（像素）。</param>
        /// <param name="dpiY">位图的水平DPI。</param>
        /// <param name="dpiX">位图的垂直DPI。</param>
        /// <param name="pixelFormat">位图的像素格式。</param>
        public SharedMemoryBitmap(int pixelWidth, int pixelHeight, double dpiY, double dpiX, PixelFormat pixelFormat)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            Format = pixelFormat;
            _dataLength = Stride * PixelHeight;

            // 计算跨距和缓冲区总大小
            Stride = (pixelWidth * Format.BitsPerPixel + 7) / 8;
            long bufferSize = (long)Stride * pixelHeight;

            // 创建内存映射文件作为共享内存
            _mmf = MemoryMappedFile.CreateNew(null, bufferSize, MemoryMappedFileAccess.ReadWrite);
            _viewAccessor = _mmf.CreateViewAccessor();

            // 获取指向共享内存的指针
            BitmapBuffer = _viewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle();

            // 从共享内存段创建 InteropBitmap
            _interopBitmap = (InteropBitmap)Imaging.CreateBitmapSourceFromMemorySection(
                _mmf.SafeMemoryMappedFileHandle.DangerousGetHandle(),
                pixelWidth,
                pixelHeight,
                pixelFormat,
                Stride,
                0); // 内存偏移量
        }

        /// <summary>
        /// 将字节块的数据写入位图的共享内存中。此方法是线程安全的，可以在后台线程调用。
        /// </summary>
        /// <param name="block">包含要写入的像素数据的字节块。</param>
        public void Write(ByteBlock block)
        {
            Write(block.UnreadSpan);
        }

        /// <summary>
        /// 写入像素数据到共享内存的私有方法。
        /// </summary>
        /// <param name="sourceSpan">需要写入的数据</param>
        public unsafe void Write(ReadOnlySpan<byte> sourceSpan)
        {
            if (BitmapBuffer == IntPtr.Zero || sourceSpan.IsEmpty)
                return;

            var destinationSpan = new Span<byte>(BitmapBuffer.ToPointer(), _dataLength);
            // 将源数据高效地复制到共享内存中
            sourceSpan.CopyTo(destinationSpan);
        }

        /// <summary>
        /// 通知UI此位图的内容已更改，需要重新渲染。
        /// </summary>
        public void Invalidate()
        {
            _interopBitmap.Invalidate();
        }

        /// <summary>
        /// 释放由该实例持有的所有资源。
        /// </summary>
        public void Dispose()
        {
            _viewAccessor?.Dispose();
            _mmf?.Dispose();
            BitmapBuffer = IntPtr.Zero;
        }

        /// <summary>
        /// 创建此对象的可修改克隆，从而深度复制此对象的值。
        /// </summary>
        protected override Freezable CreateInstanceCore()
        {
            // 此类设计为不可克隆，因为它管理着唯一的内存映射文件句柄。
            throw new NotSupportedException();
        }

        /// <summary>
        /// 将位图的像素复制到指定数组中。
        /// </summary>
        public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
        {
            _interopBitmap.CopyPixels(sourceRect, pixels, stride, offset);
        }

        /// <summary>
        /// 将位图的像素复制到指定数组中。
        /// </summary>
        public override void CopyPixels(Array pixels, int stride, int offset)
        {
            _interopBitmap.CopyPixels(pixels, stride, offset);
        }
    }
}