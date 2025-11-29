using System.IO.MemoryMappedFiles;
using System.Windows.Interop;
using System.Windows.Media;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 管理一个基于共享内存的位图，用于高性能视频渲染。 此类负责创建和管理共享内存、写入像素数据以及触发UI刷新。
    /// </summary>
    public sealed class SharedMemoryBitmap : DisposableObject
    {
        /// <summary>
        /// 共享内存映射文件，用于存储位图的像素数据。
        /// </summary>
        private readonly MemoryMappedFile? _mmf;

        /// <summary>
        /// 共享内存视图访问器，用于访问共享内存中的数据。
        /// </summary>
        private readonly MemoryMappedViewAccessor? _viewAccessor;

        /// <summary>
        /// 内存数据长度（以字节为单位）。
        /// </summary>
        private readonly int _dataLength;

        /// <summary>
        /// 获取由该管理器创建的、可直接用于WPF绑定的 <see cref="InteropBitmap"/>。
        /// </summary>
        public InteropBitmap Bitmap { get; }

        /// <summary>
        /// 获取指向共享内存块的指针，后台线程可向此地址写入像素数据。
        /// </summary>
        public IntPtr BitmapBuffer { get; private set; }

        /// <summary>
        /// 获取图像一行的字节数（也称为步幅或跨距）。
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// 获取位图的宽度（以像素为单位）。
        /// </summary>
        public int PixelWidth { get; }

        /// <summary>
        /// 获取位图的高度（以像素为单位）。
        /// </summary>
        public int PixelHeight { get; }

        /// <summary>
        /// 获取位图的像素格式。
        /// </summary>
        public PixelFormat Format { get; }

        /// <summary>
        /// 初始化一个新的 <see
        /// cref="SharedMemoryBitmap"/> 实例。
        /// </summary>
        /// <param name="pixelWidth">位图的宽度（像素）。</param>
        /// <param name="pixelHeight">位图的高度（像素）。</param>
        /// <param name="dpiY">位图的垂直DPI。</param>
        /// <param name="dpiX">位图的水平DPI。</param>
        /// <param name="pixelFormat">位图的像素格式。</param>
        public SharedMemoryBitmap(int pixelWidth, int pixelHeight, PixelFormat pixelFormat)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            Format = pixelFormat;

            Stride = (pixelWidth * Format.BitsPerPixel + 7) / 8;
            _dataLength = Stride * PixelHeight;

            _mmf = MemoryMappedFile.CreateNew(null, _dataLength, MemoryMappedFileAccess.ReadWrite);
            _viewAccessor = _mmf.CreateViewAccessor();

            BitmapBuffer = _viewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle();

            Bitmap = (InteropBitmap)Imaging.CreateBitmapSourceFromMemorySection(
                _mmf.SafeMemoryMappedFileHandle.DangerousGetHandle(),
                pixelWidth,
                pixelHeight,
                pixelFormat,
                Stride,
                0);
        }

        /// <summary>
        /// 将像素数据写入共享内存。此方法是线程安全的。
        /// </summary>
        /// <param name="block">要写入像素数据的内存块</param>
        public void Write(ByteBlock block)
        {
            Write(block.UnreadSpan);
        }

        /// <summary>
        /// 将像素数据写入共享内存。此方法是线程安全的。
        /// </summary>
        /// <param name="sourceSpan">包含要写入的像素数据的内存跨度。</param>
        public unsafe void Write(ReadOnlySpan<byte> sourceSpan)
        {
            if (BitmapBuffer == IntPtr.Zero || sourceSpan.IsEmpty)
                return;

            var destinationSpan = new Span<byte>(BitmapBuffer.ToPointer(), _dataLength);
            sourceSpan.CopyTo(destinationSpan);
        }

        /// <summary>
        /// 通知UI此位图的内容已更改，需要重新渲染。此方法是线程安全的。
        /// </summary>
        public void Invalidate()
        {
            if (!Bitmap.Dispatcher.CheckAccess())
            {
                Bitmap.Dispatcher.Invoke(Invalidate);
                return;
            }
            Bitmap.Invalidate();
        }

        /// <summary>
        /// 释放由该实例持有的所有托管资源。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _viewAccessor?.Dispose();
            _mmf?.Dispose();
            BitmapBuffer = IntPtr.Zero;
        }
    }
}