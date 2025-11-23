using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义对本地文件的同步/异步读写、容量扩展与元信息访问的契约。
    /// </summary>
    public interface IFileOperate : IDisposable
    {
        /// <summary>
        /// 获取文件的本地信息快照。
        /// </summary>
        LocalFileInfo Info { get; }

        /// <summary>
        /// 获取最后一次成功完成的读/写操作时间。
        /// </summary>
        DateTime LastOperateTime { get; }

        /// <summary>
        /// 获取或设置是否由外部宿主管理此实例（生命周期/资源）。
        /// </summary>
        /// <remarks>语义由实现定义，可用于控制文件句柄托管、缓冲复用或资源回收策略等。</remarks>
        bool IsHosted { get; set; }

        /// <summary>
        /// 获取当前逻辑容量（字节）。
        /// </summary>
        long Capacity { get; }

        /// <summary>
        ///  获取一个值，指示当前文件是否支持读取操作。
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 获取一个值，指示当前文件是否支持写入操作。
        /// </summary>
        public bool CanWrite { get; }

        #region Write

        /// <summary>
        /// 将整个字节数组写入文件（从文件起始或实现定义的位置）。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> 为 null。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(byte[] bytes);

        /// <summary>
        /// 将字节数组从指定文件偏移开始写入。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(long filePosition, byte[] bytes);

        /// <summary>
        /// 将源数组的指定区间写入到文件的指定位置。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="bytesPosition">源数组中的起始下标。</param>
        /// <param name="bytesLength">写入的字节数。</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="filePosition"/>、<paramref name="bytesPosition"/> 或 <paramref name="bytesLength"/> 非法。
        /// </exception>
        /// <exception cref="ArgumentException">源数组区间无效（越界/长度不足）。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(long filePosition, byte[] bytes, int bytesPosition, int bytesLength);

        /// <summary>
        /// 将只读跨度写入文件（实现应一次写入完 <paramref name="span"/> 的全部内容）。
        /// </summary>
        /// <param name="span">要写入的数据视图。</param>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(ReadOnlySpan<byte> span);

        /// <summary>
        /// 从指定文件偏移开始，将只读跨度写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="span">要写入的数据视图。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(long filePosition, ReadOnlySpan<byte> span);

        /// <summary>
        /// 将只读内存写入文件。适用于需要在异步期间保持缓冲区稳定的场景。
        /// </summary>
        /// <param name="memory">要写入的只读内存。</param>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 从指定文件偏移开始，将只读内存写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="memory">要写入的只读内存。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(long filePosition, ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 从指定文件偏移开始，将 <see cref="ByteBuffer"/> 的可读数据写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="buffer">顺序缓冲；实现应写入其“可读区”。</param>
        /// <remarks>实现可在写入成功后前进 <paramref name="buffer"/> 的读指针。</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(long filePosition, ref ByteBuffer buffer);

        /// <summary>
        /// 将 <see cref="ByteBuffer"/> 的可读数据写入文件（从文件起始或实现定义的位置）。
        /// </summary>
        /// <param name="buffer">顺序缓冲；实现应写入其“可读区”。</param>
        /// <remarks>实现可在写入成功后前进 <paramref name="buffer"/> 的读指针。</remarks>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(ref ByteBuffer buffer);

        /// <summary>
        /// 从指定文件偏移开始，将 <see cref="ByteBlock"/> 的可读数据写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="block">缓冲块；实现应写入其 <c>[Consumed, Length)</c> 区间。</param>
        /// <remarks>实现可在写入成功后相应推进 <paramref name="block"/>.Consumed。</remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(long filePosition, ref ByteBlock block);

        /// <summary>
        /// 将 <see cref="ByteBlock"/> 的可读数据写入文件（从文件起始或实现定义的位置）。
        /// </summary>
        /// <param name="block">缓冲块；实现应写入其 <c>[Consumed, Length)</c> 区间。</param>
        /// <remarks>实现可在写入成功后相应推进 <paramref name="block"/>.Consumed。</remarks>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(ref ByteBlock block);

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步将整个字节数组写入文件。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示异步写入的任务。</returns>
        ValueTask WriteAsync(byte[] bytes, CancellationToken token = default);

        /// <summary>
        /// 异步将字节数组从指定位置写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示异步写入的任务。</returns>
        ValueTask WriteAsync(long filePosition, byte[] bytes, CancellationToken token = default);

        /// <summary>
        /// 异步将源数组的指定区间写入到文件的指定位置。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="bytesPosition">源数组的起始下标。</param>
        /// <param name="bytesLength">写入的字节数。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示异步写入的任务。</returns>
        ValueTask WriteAsync(long filePosition, byte[] bytes, int bytesPosition, int bytesLength, CancellationToken token = default);

        /// <summary>
        /// 异步将只读内存写入文件。
        /// </summary>
        /// <param name="memory">要写入的只读内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示异步写入的任务。</returns>
        ValueTask WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default);

        #endregion WriteAsync

        #region Read

        /// <summary>
        /// 读取文件数据并返回字节数组。
        /// </summary>
        /// <returns>读取的数据。</returns>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        byte[] Read();

        /// <summary>
        /// 从指定位置读取指定长度的数据，返回读取到的字节数组。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>读取到的字节数组；无法完整读取时的具体行为由实现定义。</returns>
        byte[]? Read(long filePosition, int length);

        /// <summary>
        /// 从指定位置读取指定长度的数据到目标数组。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="bytes">目标数组。</param>
        /// <param name="bytesStart">目标数组的起始写入下标。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>实际读取的字节数（可能小于 <paramref name="length"/>）。</returns>
        /// <remarks>当读取范围超出文件末尾或目标数组不足时，常见行为为抛出异常。</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/>、<paramref name="length"/> 或 <paramref name="bytesStart"/> 非法。</exception>
        /// <exception cref="ArgumentException">目标数组区间无效（越界/长度不足）。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        int Read(long filePosition, byte[] bytes, int bytesStart, int length);

        /// <summary>
        /// 读取数据到给定的跨度。
        /// </summary>
        /// <param name="span">目标数据视图。</param>
        /// <returns>实际读取的字节数（可能小于 <paramref name="span"/> 的长度）。</returns>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        int Read(Span<byte> span);

        /// <summary>
        /// 从指定位置读取数据到给定的跨度。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="span">目标数据视图。</param>
        /// <returns>实际读取的字节数（可能小于 <paramref name="span"/> 的长度）。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        int Read(long filePosition, Span<byte> span);

        /// <summary>
        /// 读取数据到给定的内存块。
        /// </summary>
        /// <param name="memory">目标内存。</param>
        /// <returns>实际读取的字节数。</returns>
        int Read(Memory<byte> memory);

        /// <summary>
        /// 从指定位置读取数据到给定的内存块。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="memory">目标内存。</param>
        /// <returns>实际读取的字节数。</returns>
        int Read(long filePosition, Memory<byte> memory);

        /// <summary>
        /// 读取文件（或实现定义的默认范围）到 <see cref="ByteBuffer"/> 的写入端。
        /// </summary>
        /// <param name="buffer">顺序缓冲；实现应将数据追加到其写入区尾部。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <remarks>实现可调用 GetSpan/GetMemory + WriteAdvance 等方式写入。</remarks>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        int Read(ref ByteBuffer buffer);

        /// <summary>
        /// 从指定位置读取数据到 <see cref="ByteBuffer"/> 的写入端。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="buffer">顺序缓冲；实现应将数据追加到其写入区尾部。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        int Read(long filePosition, ref ByteBuffer buffer);

        /// <summary>
        /// 从指定位置读取指定长度的数据到 <see cref="ByteBuffer"/> 的写入端。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="buffer">顺序缓冲；实现应将数据追加到其写入区尾部。</param>
        /// <returns>实际读取的字节数（可能小于 <paramref name="length"/>）。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 或 <paramref name="length"/> 非法。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        int Read(long filePosition, int length, ref ByteBuffer buffer);

        /// <summary>
        /// 读取文件（或实现定义的默认范围）到 <see cref="ByteBlock"/> 的写入端（在其 <c>Length</c> 位置追加）。
        /// </summary>
        /// <param name="block">缓冲块；实现应写入到其可写区。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        int Read(ref ByteBlock block);

        /// <summary>
        /// 从指定位置读取数据到 <see cref="ByteBlock"/> 的写入端（在其 <c>Length</c> 位置追加）。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="block">缓冲块；实现应写入到其可写区。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        int Read(long filePosition, ref ByteBlock block);

        /// <summary>
        /// 从指定位置读取指定长度的数据到 <see cref="ByteBlock"/> 的写入端（在其 <c>Length</c> 位置追加）。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="block">缓冲块；实现应写入到其可写区。</param>
        /// <returns>实际读取的字节数（可能小于 <paramref name="length"/>）。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 或 <paramref name="length"/> 非法。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        int Read(long filePosition, int length, ref ByteBlock block);

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步读取整个文件或实现定义的默认范围的数据。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>包含读取数据的数组。</returns>
        ValueTask<byte[]> ReadAsync(CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>包含读取数据的数组。</returns>
        ValueTask<byte[]> ReadAsync(long filePosition, int length, CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取指定长度的数据到目标数组。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="bytes">目标数组。</param>
        /// <param name="bytesStart">目标数组的起始写入下标。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>实际读取的字节数。</returns>
        ValueTask<int> ReadAsync(long filePosition, int length, byte[] bytes, int bytesStart, CancellationToken token = default);

        /// <summary>
        /// 异步读取数据到给定的内存块。
        /// </summary>
        /// <param name="memory">目标内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>实际读取的字节数。</returns>
        ValueTask<int> ReadAsync(Memory<byte> memory, CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取数据到给定的内存块。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="memory">目标内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>实际读取的字节数。</returns>
        ValueTask<int> ReadAsync(long filePosition, Memory<byte> memory, CancellationToken token = default);

        #endregion ReadAsync

        /// <summary>
        /// 扩展底层存储容量至指定大小。
        /// </summary>
        /// <param name="newCapacity">目标容量（字节）。</param>
        /// <remarks>
        /// 实现建议（可择一或结合）：稀疏文件以降低预扩容成本；或按文件系统分配单元对齐预分配以减少碎片与提升 IO。
        /// 具体平台可用能力依文件系统/权限而定。
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newCapacity"/> 小于当前容量。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        void ExpandCapacity(long newCapacity);

        /// <summary>
        /// 获取文件的唯一标识符 (GUID)。
        /// 在 Windows 上，此 GUID 基于卷序列号和文件索引号，即使文件移动或重命名也能保持不变。
        /// 在其他操作系统上，它基于文件完整路径的 SHA1 哈希值。
        /// </summary>
        /// <returns>表示文件的唯一 Guid。</returns>
        Guid GetFileGuid();
    }
}