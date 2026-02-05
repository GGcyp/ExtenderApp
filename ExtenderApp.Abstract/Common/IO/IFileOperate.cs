using System.Buffers;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义对本地文件的同步/异步读写、容量扩展与元信息访问的契约。 实现者应在失败时返回相应的 <see cref="Result"/> / <see cref="Result{T}"/>，在成功时更新 <see cref="LastOperateTime"/> 并返回实际读取/写入的字节数或数据。
    /// </summary>
    public interface IFileOperate : IDisposable
    {
        #region Info

        /// <summary>
        /// 获取文件的本地信息快照（路径、存在性、大小等）。
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
        /// 获取当前逻辑容量（字节）。通常表示底层可用或预分配的长度。
        /// </summary>
        long Capacity { get; }

        /// <summary>
        /// 获取一个值，指示当前文件是否支持读取操作。
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 获取一个值，指示当前文件是否支持写入操作。
        /// </summary>
        bool CanWrite { get; }

        #endregion Info

        #region Write

        /// <summary>
        /// 将指定的数据（ <paramref name="span"/>）同步写入文件，从 <paramref name="filePosition"/> 开始。
        /// </summary>
        /// <param name="span">要写入的只读跨度。</param>
        /// <param name="filePosition">写入起始位置（字节偏移），默认从文件开头开始写入。</param>
        /// <returns>表示操作结果的 <see cref="Result{T}"/>。成功时其 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。</returns>
        Result<int> Write(ReadOnlySpan<byte> span, long filePosition = 0);

        /// <summary>
        /// 将指定的数据（ <paramref name="memory"/>）同步写入文件，从 <paramref name="filePosition"/> 开始。
        /// </summary>
        /// <param name="memory">要写入的只读内存。</param>
        /// <param name="filePosition">写入起始位置（字节偏移），默认从文件开头开始写入。</param>
        /// <returns>表示操作结果的 <see cref="Result{T}"/>。成功时其 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。</returns>
        Result<int> Write(ReadOnlyMemory<byte> memory, long filePosition = 0);

        /// <summary>
        /// 将指定的 <paramref name="sequence"/>（可能由多段组成）同步写入文件，从 <paramref name="filePosition"/> 开始。
        /// </summary>
        /// <param name="sequence">要写入的只读序列。</param>
        /// <param name="filePosition">写入起始位置（字节偏移），默认从文件开头开始写入。</param>
        /// <returns>表示操作结果的 <see cref="Result{T}"/>。成功时其 <see cref="Result{T}.Value"/> 为实际写入的总字节数（int）。</returns>
        Result<int> Write(ReadOnlySequence<byte> sequence, long filePosition = 0);

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步将 <paramref name="memory"/> 写入文件，从 <paramref name="filePosition"/> 开始。
        /// </summary>
        /// <param name="memory">要写入的只读内存。</param>
        /// <param name="filePosition">写入起始位置（字节偏移），默认从文件开头开始写入。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>。成功时其 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。</returns>
        ValueTask<Result<int>> WriteAsync(ReadOnlyMemory<byte> memory, long filePosition = 0, CancellationToken token = default);

        /// <summary>
        /// 异步将 <paramref name="sequence"/> 的所有分段写入文件，从 <paramref name="filePosition"/> 开始。
        /// </summary>
        /// <param name="sequence">要写入的只读序列。</param>
        /// <param name="filePosition">写入起始位置（字节偏移），默认从文件开头开始写入。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>见 <see cref="WriteAsync(ReadOnlyMemory{byte}, long, CancellationToken)"/>。</returns>
        ValueTask<Result<int>> WriteAsync(ReadOnlySequence<byte> sequence, long filePosition = 0, CancellationToken token = default);

        #endregion WriteAsync

        #region Read

        /// <summary>
        /// 读取整个文件（或从指定偏移到文件末尾）的字节数组。
        /// </summary>
        /// <param name="filePosition">起始读取位置（字节偏移），默认从文件开头开始读取。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时包含读取到的字节数组。</returns>
        Result<byte[]> Read(long filePosition = 0);

        /// <summary>
        /// 从指定位置读取固定长度的字节数组。
        /// </summary>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时包含读取到的字节数组。</returns>
        Result<byte[]> Read(int length, long filePosition);

        /// <summary>
        /// 将文件数据读取到提供的 <paramref name="span"/> 中。
        /// </summary>
        /// <param name="span">目标缓冲区（写入从 span[0] 开始）。</param>
        /// <param name="filePosition">起始读取位置（字节偏移），默认从文件开头开始读取。</param>
        /// <returns>一个 <see cref="Result{T}"/>，成功时其 <see cref="Result{T}.Value"/> 为实际读取的字节数（int）。</returns>
        Result<int> Read(Span<byte> span, long filePosition = 0);

        /// <summary>
        /// 将文件数据读取到提供的 <paramref name="memory"/> 中。
        /// </summary>
        /// <param name="memory">目标内存块。</param>
        /// <param name="filePosition">起始读取位置（字节偏移），默认从文件开头开始读取。</param>
        /// <returns>见 <see cref="Read(Span{byte}, long)"/>。</returns>
        Result<int> Read(Memory<byte> memory, long filePosition = 0);

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步读取整个文件（或从指定偏移到文件末尾）。
        /// </summary>
        /// <param name="filePosition">起始读取位置（字节偏移），默认从文件开头开始读取。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>，成功时包含读取到的字节数组。</returns>
        ValueTask<Result<byte[]>> ReadAsync(long filePosition = 0, CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取固定长度的数据。
        /// </summary>
        /// <param name="filePosition">起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>，成功时包含读取到的字节数组。</returns>
        ValueTask<Result<byte[]>> ReadAsync(long filePosition, int length, CancellationToken token = default);

        /// <summary>
        /// 异步将文件数据读取到提供的 <paramref name="memory"/> 中。
        /// </summary>
        /// <param name="memory">目标内存块。</param>
        /// <param name="filePosition">起始读取位置（字节偏移），默认从文件开头开始读取。</param>
        /// <param name="token">可选的取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>，其结果为实际读取的字节数（int）。</returns>
        ValueTask<Result<int>> ReadAsync(Memory<byte> memory, long filePosition = 0, CancellationToken token = default);

        #endregion ReadAsync

        /// <summary>
        /// 将底层存储扩展至指定容量。实现应保证扩展后可用于后续读写操作。
        /// </summary>
        /// <param name="newCapacity">新的目标容量（字节）。</param>
        void ExpandCapacity(long newCapacity);
    }
}