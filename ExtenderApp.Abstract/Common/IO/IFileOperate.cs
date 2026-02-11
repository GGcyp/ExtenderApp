using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供对本地文件的读写、异步操作、容量扩展与元信息访问的契约。 实现者应保证在成功操作后更新 <see cref="LastOperateTime"/> 并按照契约返回 <see cref="Result"/> / <see cref="Result{T}"/>。 调用方可以通过
    /// <see cref="IsHosted"/> 控制实现者是否由外部宿主管理生命周期。
    /// </summary>
    public interface IFileOperate : IDisposable
    {
        #region Info

        /// <summary>
        /// 获取文件的本地信息快照（包含路径、存在性、大小等）。
        /// </summary>
        LocalFileInfo Info { get; }

        /// <summary>
        /// 获取最后一次成功完成的读/写操作时间（本地时间）。
        /// </summary>
        DateTime LastOperateTime { get; }

        /// <summary>
        /// 获取或设置是否由外部宿主管理此实例的生命周期与资源释放。 若为 <c>true</c>，调用方负责释放资源；否则实现者负责生命周期管理。
        /// </summary>
        bool IsHosted { get; set; }

        /// <summary>
        /// 获取当前逻辑容量（以字节为单位）。通常表示底层可用或预分配的长度。
        /// </summary>
        long Capacity { get; }

        /// <summary>
        /// 指示当前文件是否支持读取操作（可读取）。
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 指示当前文件是否支持写入操作（可写入）。
        /// </summary>
        bool CanWrite { get; }

        #endregion Info

        #region Write

        /// <summary>
        /// 将指定的数据写入文件（同步）。
        /// </summary>
        /// <param name="span">要写入的字节跨度。</param>
        /// <param name="filePosition">写入起始位置（字节偏移）。默认值为 0（从文件开头写入）。</param>
        /// <returns>返回操作结果的 <see cref="Result{T}"/>；成功时其 <see cref="Result{T}.Value"/> 表示实际写入的字节数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="filePosition"/> 为负值时抛出。</exception>
        Result<int> Write(ReadOnlySpan<byte> span, long filePosition = 0);

        /// <summary>
        /// 将指定的缓冲区内容同步写入文件。
        /// </summary>
        /// <param name="buffer">包含要写入数据的缓冲区，不能为空。</param>
        /// <param name="filePosition">写入起始位置（字节偏移）。默认值为 0（从文件开头写入）。</param>
        /// <returns>返回操作结果的 <see cref="Result{T}"/>；成功时其 <see cref="Result{T}.Value"/> 为实际写入的字节数。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="filePosition"/> 为负值时抛出。</exception>
        Result<long> Write(AbstractBuffer<byte> buffer, long filePosition = 0);

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步将缓冲区内容写入文件。
        /// </summary>
        /// <param name="buffer">包含要写入数据的缓冲区，不能为空。</param>
        /// <param name="filePosition">写入起始位置（字节偏移）。</param>
        /// <param name="token">可选的取消令牌，用于取消异步写入操作。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>。成功时其 <see cref="Result{T}.Value"/> 为实际写入的字节数。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为 null 时抛出。</exception>
        ValueTask<Result<long>> WriteAsync(AbstractBuffer<byte> buffer, long filePosition = 0, CancellationToken token = default);

        #endregion WriteAsync

        #region Read

        /// <summary>
        /// 读取整个文件或从指定偏移到文件末尾的字节数据（同步）。
        /// </summary>
        /// <param name="filePosition">起始读取位置（字节偏移）。默认值为 0（从文件开头读取）。</param>
        /// <returns>包含读取结果的 <see cref="Result{T}"/>；成功时其 <see cref="Result{T}.Value"/> 为字节数组。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="filePosition"/> 为负值时抛出。</exception>
        Result<byte[]> Read(long filePosition = 0);

        /// <summary>
        /// 从指定位置读取固定长度的数据（同步）。
        /// </summary>
        /// <param name="length">要读取的字节数（非负）。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <returns>包含读取结果的 <see cref="Result{T}"/>；成功时其 <see cref="Result{T}.Value"/> 为字节数组（长度可能小于请求长度，取决于文件剩余数据）。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="length"/> 或 <paramref name="filePosition"/> 非法时抛出。</exception>
        Result<byte[]> Read(int length, long filePosition = 0);

        /// <summary>
        /// 将文件数据读取到调用方提供的目标跨度中（同步）。
        /// </summary>
        /// <param name="span">目标写入跨度，数据将从 <c>span[0]</c> 开始放入。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <returns>包含操作结果的 <see cref="Result{T}"/>；成功时其 <see cref="Result{T}.Value"/> 为实际读取的字节数。</returns>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="filePosition"/> 非法或 <paramref name="span"/> 长度不足以容纳读取数据（视实现而定）时可能抛出。</exception>
        Result<int> Read(Span<byte> span, long filePosition = 0);

        /// <summary>
        /// 将文件数据读取到提供的缓冲区中（同步），目标缓冲区由调用方提供并写入数据。
        /// </summary>
        /// <param name="length">期望读取的字节数（非负）。</param>
        /// <param name="buffer">目标缓冲区，用于接收读取到的数据。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <returns>返回实际写入目标缓冲区的字节数（封装在 <see cref="Result{T}"/> 中）。</returns>
        Result<long> Read(long length, AbstractBuffer<byte> buffer, long filePosition = 0);

        /// <summary>
        /// 将文件数据读取到提供的缓冲区中（同步），并通过 out 返回用于接收数据的缓冲区实例。
        /// </summary>
        /// <param name="length">期望读取的字节数（非负）。</param>
        /// <param name="buffer">输出参数，返回存放读取数据的缓冲区实例。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <returns>返回实际写入目标缓冲区的字节数（封装在 <see cref="Result{T}"/> 中）。</returns>
        Result<long> Read(long length, out AbstractBuffer<byte> buffer, long filePosition = 0);

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步读取整个文件或从指定偏移到文件末尾。
        /// </summary>
        /// <param name="filePosition">起始读取位置（字节偏移）。默认值为 0（从文件开头读取）。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>；成功时其 <see cref="Result{T}.Value"/> 包含读取到的字节数组。</returns>
        ValueTask<Result<byte[]>> ReadAsync(long filePosition = 0, CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取固定长度的数据。
        /// </summary>
        /// <param name="length">要读取的字节数（非负）。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>；成功时其 <see cref="Result{T}.Value"/> 包含读取到的字节数组。</returns>
        ValueTask<Result<byte[]>> ReadAsync(int length, long filePosition = 0, CancellationToken token = default);

        /// <summary>
        /// 异步将文件数据读取到调用方提供的缓冲区中。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="length">期望读取的字节数（非负）。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>；成功时其 <see cref="Result{T}.Value"/> 为实际读取的字节数。</returns>
        ValueTask<Result<long>> ReadAsync(long length, AbstractBuffer<byte> buffer, long filePosition = 0, CancellationToken token = default);

        /// <summary>
        /// 异步读取指定长度的数据并返回作为缓冲区实例（实现者可复用/池化返回的缓冲区）。
        /// </summary>
        /// <param name="length">要读取的字节数（非负）。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>异步返回一个 <see cref="Result{T}"/>，成功时其 <see cref="Result{T}.Value"/> 为包含读取数据的 <see cref="AbstractBuffer{T}"/> 实例。</returns>
        ValueTask<Result<AbstractBuffer<byte>>> ReadAsync(long length, long filePosition = 0, CancellationToken token = default);

        #endregion ReadAsync

        /// <summary>
        /// 将底层存储扩展至指定容量。调用者期待扩展后可进行后续读写操作。
        /// </summary>
        /// <param name="newCapacity">新的目标容量（字节），应大于或等于当前容量。</param>
        void ExpandCapacity(long newCapacity);
    }
}