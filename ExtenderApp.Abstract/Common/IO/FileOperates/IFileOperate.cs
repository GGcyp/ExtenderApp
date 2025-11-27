using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义对本地文件的同步/异步读写、容量扩展与元信息访问的契约。
    /// </summary>
    public interface IFileOperate : IDisposable
    {
        #region Info

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
        bool CanWrite { get; }

        #endregion

        #region Write

        /// <summary>
        /// 将整个字节数组写入文件。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(byte[] bytes);

        /// <summary>
        /// 将字节数组从指定文件偏移开始写入。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(long filePosition, byte[] bytes);

        /// <summary>
        /// 将源数组的指定区间写入到文件的指定位置。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="bytesPosition">源数组中的起始下标。</param>
        /// <param name="bytesLength">写入的字节数。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(long filePosition, byte[] bytes, int bytesPosition, int bytesLength);

        /// <summary>
        /// 将只读跨度写入文件。
        /// </summary>
        /// <param name="span">要写入的数据视图。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(ReadOnlySpan<byte> span);

        /// <summary>
        /// 从指定文件偏移开始，将只读跨度写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="span">要写入的数据视图。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(long filePosition, ReadOnlySpan<byte> span);

        /// <summary>
        /// 将只读内存写入文件。
        /// </summary>
        /// <param name="memory">要写入的只读内存。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 从指定文件偏移开始，将只读内存写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="memory">要写入的只读内存。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(long filePosition, ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 从指定文件偏移开始，将 <see cref="ByteBuffer"/> 的可读数据写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="buffer">顺序缓冲；实现应写入其“可读区”。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(long filePosition, ref ByteBuffer buffer);

        /// <summary>
        /// 将 <see cref="ByteBuffer"/> 的可读数据写入文件。
        /// </summary>
        /// <param name="buffer">顺序缓冲；实现应写入其“可读区”。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(ref ByteBuffer buffer);

        /// <summary>
        /// 从指定文件偏移开始，将 <see cref="ByteBlock"/> 的可读数据写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="block">缓冲块；实现应写入其 <c>[Consumed, Length)</c> 区间。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(long filePosition, ref ByteBlock block);

        /// <summary>
        /// 将 <see cref="ByteBlock"/> 的可读数据写入文件。
        /// </summary>
        /// <param name="block">缓冲块；实现应写入其 <c>[Consumed, Length)</c> 区间。</param>
        /// <returns>一个表示操作结果的 <see cref="Result"/>。</returns>
        Result Write(ref ByteBlock block);

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步将整个字节数组写入文件。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result"/>。</returns>
        ValueTask<Result> WriteAsync(byte[] bytes, CancellationToken token = default);

        /// <summary>
        /// 异步将字节数组从指定位置写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result"/>。</returns>
        ValueTask<Result> WriteAsync(long filePosition, byte[] bytes, CancellationToken token = default);

        /// <summary>
        /// 异步将源数组的指定区间写入到文件的指定位置。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="bytesPosition">源数组的起始下标。</param>
        /// <param name="bytesLength">写入的字节数。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result"/>。</returns>
        ValueTask<Result> WriteAsync(long filePosition, byte[] bytes, int bytesPosition, int bytesLength, CancellationToken token = default);

        /// <summary>
        /// 异步将只读内存写入文件。
        /// </summary>
        /// <param name="memory">要写入的只读内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result"/>。</returns>
        ValueTask<Result> WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default);

        #endregion WriteAsync

        #region Read

        /// <summary>
        /// 读取整个文件的数据。
        /// </summary>
        /// <returns>一个 <see cref="Result{T}"/>，成功时包含读取到的字节数组。</returns>
        Result<byte[]> Read();

        /// <summary>
        /// 从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时包含读取到的字节数组。</returns>
        Result<byte[]> Read(long filePosition, int length);

        /// <summary>
        /// 从指定位置读取数据到目标数组。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="bytes">目标数组。</param>
        /// <param name="bytesStart">目标数组的起始写入下标。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        Result<int> Read(long filePosition, byte[] bytes, int bytesStart, int length);

        /// <summary>
        /// 从文件起始位置读取数据到给定的跨度。
        /// </summary>
        /// <param name="span">目标数据视图。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        Result<int> Read(Span<byte> span);

        /// <summary>
        /// 从指定位置读取数据到给定的跨度。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="span">目标数据视图。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        Result<int> Read(long filePosition, Span<byte> span);

        /// <summary>
        /// 从文件起始位置读取数据到给定的内存块。
        /// </summary>
        /// <param name="memory">目标内存。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        Result<int> Read(Memory<byte> memory);

        /// <summary>
        /// 从指定位置读取数据到给定的内存块。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="memory">目标内存。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        Result<int> Read(long filePosition, Memory<byte> memory);

        /// <summary>
        /// 从文件起始位置读取数据到 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="buffer">顺序缓冲。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        Result<int> Read(ref ByteBuffer buffer);

        /// <summary>
        /// 从指定位置读取数据到 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="buffer">顺序缓冲。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        Result<int> Read(long filePosition, ref ByteBuffer buffer);

        /// <summary>
        /// 从指定位置读取指定长度的数据到 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="buffer">顺序缓冲。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        Result<int> Read(long filePosition, int length, ref ByteBuffer buffer);

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步读取整个文件。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result{T}"/>，成功时包含读取到的字节数组。</returns>
        ValueTask<Result<byte[]>> ReadAsync(CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result{T}"/>，成功时包含读取到的字节数组。</returns>
        ValueTask<Result<byte[]>> ReadAsync(long filePosition, int length, CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取数据到目标数组。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="bytes">目标数组。</param>
        /// <param name="bytesStart">目标数组的起始写入下标。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        ValueTask<Result<int>> ReadAsync(long filePosition, int length, byte[] bytes, int bytesStart, CancellationToken token = default);

        /// <summary>
        /// 异步从文件起始位置读取数据到内存块。
        /// </summary>
        /// <param name="memory">目标内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        ValueTask<Result<int>> ReadAsync(Memory<byte> memory, CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取数据到内存块。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="memory">目标内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>一个表示异步操作的 <see cref="ValueTask"/>，其结果是一个 <see cref="Result{T}"/>，成功时其值为实际读取的字节数。</returns>
        ValueTask<Result<int>> ReadAsync(long filePosition, Memory<byte> memory, CancellationToken token = default);

        #endregion ReadAsync

        /// <summary>
        /// 将底层存储扩展至指定容量。
        /// </summary>
        /// <param name="newCapacity">新的目标容量。</param>
        void ExpandCapacity(long newCapacity);

        /// <summary>
        /// 获取文件的唯一标识符 (GUID)。
        /// </summary>
        /// <returns>表示文件的唯一 Guid。</returns>
        Guid GetFileGuid();
    }
}