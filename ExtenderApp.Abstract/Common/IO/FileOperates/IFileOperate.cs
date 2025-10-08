using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 文件操作接口，定义对本地文件的同步/异步读写、容量扩展与元信息访问。
    /// 同时继承 <see cref="IConcurrentOperate"/> 以约束并发操作行为。
    /// </summary>
    /// <remarks>
    /// 约定：
    /// - 实现应在并发/线程安全方面给出明确策略（如串行化访问，或基于 <see cref="IConcurrentOperate"/> 的队列执行）。<br/>
    /// - 所有读写均以字节为单位，偏移和长度的合法性需要由调用方与实现共同保障。<br/>
    /// - 文档中的异常为常见情形，具体行为以实现为准。
    /// </remarks>
    public interface IFileOperate
    {
        /// <summary>
        /// 获取文件的本地信息快照。
        /// </summary>
        /// <remarks>
        /// 通常为读取时刻的快照；是否实时更新由实现决定。
        /// 如需刷新，可调用 <see cref="LocalFileInfo.UpdateFileInfo"/>（若可用）。
        /// </remarks>
        LocalFileInfo Info { get; }

        /// <summary>
        /// 获取最后一次成功执行文件读/写操作的时间。
        /// </summary>
        /// <returns>最后一次操作的时间，若尚未发生过操作，可能为默认值。</returns>
        DateTime LastOperateTime { get; }

        /// <summary>
        /// 获取或设置是否由外部宿主管理此实例的生命周期/资源。
        /// </summary>
        /// <remarks>
        /// 该标志具体含义由实现定义，可用于控制文件句柄托管、缓冲区复用等策略。
        /// </remarks>
        bool IsHosted { get; set; }

        #region Write

        /// <summary>
        /// 将字节数组写入文件（从文件起始位置或实现定义的位置）。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> 为 null。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限访问文件。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(byte[] bytes);

        /// <summary>
        /// 将字节数组从指定文件位置开始写入。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">文件中的起始写入位置（字节偏移）。</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 为负数。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限访问文件。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(byte[] bytes, long filePosition);

        /// <summary>
        /// 将字节数组的指定区间写入到文件的指定位置。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">文件中的起始写入位置（字节偏移）。</param>
        /// <param name="bytesPosition">源数组中的起始下标。</param>
        /// <param name="bytesLength">写入的字节长度。</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="filePosition"/>、<paramref name="bytesPosition"/> 或 <paramref name="bytesLength"/> 非法。
        /// </exception>
        /// <exception cref="ArgumentException">源数组区间无效（越界或长度不足）。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限访问文件。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(byte[] bytes, long filePosition, int bytesPosition, int bytesLength);

        /// <summary>
        /// 使用二进制写入器将数据写入到指定文件位置。
        /// </summary>
        /// <param name="writer">用于写入数据的 <see cref="ExtenderBinaryWriter"/> 对象（其缓冲数据将被写入）。</param>
        /// <param name="filePosition">文件中的起始写入位置（字节偏移）。</param>
        /// <remarks>
        /// 调用方负责管理 <paramref name="writer"/> 的生命周期（包括其租赁的缓冲区）。
        /// 实现应将写入器中已缓冲的数据持久化到文件。
        /// </remarks>
        /// <exception cref="ArgumentException">写入器当前状态不合法或无数据。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Write(ExtenderBinaryWriter writer, long filePosition);

        #endregion

        #region WriteAsync

        /// <summary>
        /// 异步将字节数组写入文件。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <returns>表示异步写入操作的任务。</returns>
        Task WriteAsync(byte[] bytes);

        /// <summary>
        /// 异步将字节数组从指定位置写入文件。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">文件中的起始写入位置（字节偏移）。</param>
        /// <returns>表示异步写入操作的任务。</returns>
        Task WriteAsync(byte[] bytes, long filePosition);

        /// <summary>
        /// 异步将字节数组的指定区间写入到文件的指定位置。
        /// </summary>
        /// <param name="bytes">要写入的字节数组。</param>
        /// <param name="filePosition">文件中的起始写入位置（字节偏移）。</param>
        /// <param name="bytesPosition">源数组中的起始下标。</param>
        /// <param name="bytesLength">写入的字节长度。</param>
        /// <returns>表示异步写入操作的任务。</returns>
        Task WriteAsync(byte[] bytes, long filePosition, int bytesPosition, int bytesLength);

        /// <summary>
        /// 使用二进制写入器异步写入数据至指定文件位置。
        /// </summary>
        /// <param name="writer">用于写入数据的二进制写入器。</param>
        /// <param name="filePosition">文件中的起始写入位置（字节偏移）。</param>
        /// <returns>表示异步写入操作的任务。</returns>
        Task WriteAsync(ExtenderBinaryWriter writer, long filePosition);

        #endregion

        #region Read

        /// <summary>
        /// 读取文件数据并返回字节数组。
        /// </summary>
        /// <returns>读取的数据。</returns>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="UnauthorizedAccessException">无权限访问文件。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        byte[] Read();

        /// <summary>
        /// 从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="filePosition">文件中的起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节长度。</param>
        /// <returns>读取到的字节数组；当无法读取到期望长度时的行为由实现决定（可能返回 null 或抛出异常）。</returns>
        byte[]? Read(long filePosition, int length);

        /// <summary>
        /// 从指定位置读取指定长度的数据并写入到给定的 <see cref="ExtenderBinaryWriter"/>。
        /// </summary>
        /// <param name="filePosition">文件中的起始读取位置（字节偏移）。</param>
        /// <param name="length">需要读取的长度。</param>
        /// <param name="writer">用于接收数据的写入器，数据将被追加写入。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/> 或 <paramref name="length"/> 非法。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        /// <exception cref="ObjectDisposedException">对象已释放。</exception>
        void Read(long filePosition, int length, ref ExtenderBinaryWriter writer);

        /// <summary>
        /// 从指定位置读取指定长度的数据到目标字节数组。
        /// </summary>
        /// <param name="filePosition">文件中的起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节长度。</param>
        /// <param name="bytes">目标字节数组。</param>
        /// <param name="bytesStart">目标数组的起始写入下标。</param>
        /// <remarks>
        /// 当读取范围超出文件末尾或目标数组不足时，通常应抛出异常；具体行为由实现决定。
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePosition"/>、<paramref name="length"/> 或 <paramref name="bytesStart"/> 非法。</exception>
        /// <exception cref="ArgumentException">目标数组区间无效（越界或长度不足）。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        void Read(long filePosition, int length, byte[] bytes, int bytesStart = 0);

        /// <summary>
        /// 同步读取数据，并尽量使用内部数组池以减少分配。
        /// </summary>
        /// <param name="length">输出：实际读取的字节数。</param>
        /// <returns>包含读取数据的字节数组。</returns>
        /// <remarks>实现可使用 ArrayPool 复用缓冲区；调用方仅需使用返回数组，无需手动归还。</remarks>
        byte[] ReadForArrayPool(out int length);

        /// <summary>
        /// 同步从指定位置读取指定长度的数据，并尽量使用内部数组池以减少分配。
        /// </summary>
        /// <param name="filePosition">文件中的起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节长度。</param>
        /// <returns>包含读取数据的字节数组。</returns>
        /// <remarks>实现可使用 ArrayPool 复用缓冲区；调用方仅需使用返回数组，无需手动归还。</remarks>
        byte[] ReadForArrayPool(long filePosition, int length);

        #endregion

        #region ReadAsync

        /// <summary>
        /// 异步从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="filePosition">文件中的起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节长度。</param>
        /// <returns>表示异步读取操作的任务。</returns>
        Task ReadAsync(long filePosition, int length);

        /// <summary>
        /// 异步从指定位置读取指定长度的数据到目标数组。
        /// </summary>
        /// <param name="filePosition">文件中的起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节长度。</param>
        /// <param name="bytes">目标数组。</param>
        /// <param name="bytesStart">目标数组的起始写入下标。</param>
        /// <returns>表示异步读取操作的任务。</returns>
        Task ReadAsync(long filePosition, int length, byte[] bytes, int bytesStart = 0);

        /// <summary>
        /// 异步读取整个文件或实现定义的默认范围的数据。
        /// </summary>
        /// <returns>包含读取数据的字节数组。</returns>
        Task<byte[]> ReadAsync();

        /// <summary>
        /// 异步读取数据，并将结果缓存在内部数组池以减少分配（实现可使用 ArrayPool）。
        /// </summary>
        /// <param name="length">输出：实际读取的字节数。</param>
        /// <returns>包含读取数据的字节数组。</returns>
        /// <remarks>调用方仅需使用返回的数组，无需手动归还。</remarks>
        Task<byte[]> ReadForArrayPoolAsync(out int length);

        /// <summary>
        /// 异步从指定位置读取指定长度的数据，并尽量使用内部数组池以减少分配。
        /// </summary>
        /// <param name="filePosition">文件中的起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节长度。</param>
        /// <returns>包含读取数据的字节数组。</returns>
        /// <remarks>调用方仅需使用返回的数组，无需手动归还。</remarks>
        Task<byte[]> ReadForArrayPoolAsync(long filePosition, int length);

        #endregion

        /// <summary>
        /// 扩展底层存储容量到指定大小。
        /// </summary>
        /// <param name="newCapacity">目标容量（字节）。</param>
        /// <remarks>
        /// 实现建议（择一或结合）：
        /// - 稀疏文件：为大文件预留“空洞”区域（读为 0，不占磁盘），仅在写入时分配实际块，可显著降低预扩容成本。<br/>
        /// - 按块对齐预分配：将长度扩展至文件系统分配单元（簇/块）的整数倍，减少碎片并提升 IO 性能。<br/>
        /// 实现要点：
        /// - Windows：可通过 DeviceIoControl 的 FSCTL_SET_SPARSE 标记文件为稀疏；使用 FSCTL_SET_ZERO_DATA 打洞/释放区间；或用 SetLength 结合 SetFileValidData（需特权）进行快速预分配。<br/>
        /// - Linux/macOS：可调用 posix_fallocate/fallocate 进行预分配；fallocate+PUNCH_HOLE 支持打洞（需文件系统支持）。<br/>
        /// - 对齐策略：查询分配单元（Windows: GetDiskFreeSpace；Linux: statvfs），将 newCapacity 向上对齐至分配单元的整数倍再扩容。
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newCapacity"/> 小于当前大小。</exception>
        /// <exception cref="IOException">底层 IO 错误。</exception>
        void ExpandCapacity(long newCapacity);
    }
}
