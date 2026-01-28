using System.Buffers;
using System.Threading;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 定义对本地文件的同步/异步读写、容量扩展与元信息访问的契约。
    /// 实现者应保证在失败时返回相应的 <see cref="Result"/>/ <see cref="Result{T}"/>，
    /// 在成功时更新 <see cref="LastOperateTime"/> 并返回实际读取/写入的字节数或数据。
    /// </summary>
    public interface IFileOperate : IDisposable
    {
        #region Info

        /// <summary>
        /// 获取文件的本地信息快照。
        /// </summary>
        LocalFileInfo Info { get; }

        /// <summary>
        /// 获取最后一次成功完成的读/写操作时间（以本地时间表示）。
        /// </summary>
        DateTime LastOperateTime { get; }

        /// <summary>
        /// 获取或设置是否由外部宿主管理此实例（生命周期/资源）。
        /// 若为 <c>true</c>，调用方负责释放资源；若为 <c>false</c>，实现者应负责生命周期管理。
        /// </summary>
        bool IsHosted { get; set; }

        /// <summary>
        /// 获取当前逻辑容量（字节）。
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
        /// 将只读跨度写入文件（从文件起始位置写入）。
        /// </summary>
        /// <param name="span">要写入的数据视图；实现不应保留对该跨度的引用。</param>
        /// <returns>
        /// 表示操作结果的 <see cref="Result{T}"/>。成功时 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。
        /// 失败时返回包含错误消息或异常的 <see cref="Result{T}"/>。
        /// </returns>
        Result<int> Write(ReadOnlySpan<byte> span);

        /// <summary>
        /// 从指定文件偏移开始，将只读跨度写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="span">要写入的数据视图。</param>
        /// <returns>
        /// 表示操作结果的 <see cref="Result{T}"/>。成功时 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。
        /// </returns>
        Result<int> Write(long filePosition, ReadOnlySpan<byte> span);

        /// <summary>
        /// 将只读内存写入文件（从文件起始位置写入）。
        /// </summary>
        /// <param name="memory">要写入的只读内存。</param>
        /// <returns>
        /// 表示操作结果的 <see cref="Result{T}"/>。成功时 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。
        /// </returns>
        Result<int> Write(ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 从指定文件偏移开始，将只读内存写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="memory">要写入的只读内存。</param>
        /// <returns>
        /// 表示操作结果的 <see cref="Result{T}"/>。成功时 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。
        /// </returns>
        Result<int> Write(long filePosition, ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 将只读序列写入文件（从文件起始位置写入）。
        /// </summary>
        /// <param name="sequence">要写入的只读序列。</param>
        /// <returns>
        /// 表示操作结果的 <see cref="Result{T}"/>。成功时 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。
        /// </returns>
        Result<int> Write(ReadOnlySequence<byte> sequence);

        /// <summary>
        /// 从指定文件偏移开始，将只读序列写入文件。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="sequence">要写入的只读序列。</param>
        /// <returns>
        /// 表示操作结果的 <see cref="Result{T}"/>。成功时 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。
        /// </returns>
        Result<int> Write(long filePosition, ReadOnlySequence<byte> sequence);

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步将只读内存写入文件（从文件起始位置写入）。
        /// </summary>
        /// <param name="memory">要写入的只读内存。</param>
        /// <param name="token">取消令牌；若用户取消操作，应返回失败结果或抛出相应异常并由调用方处理。</param>
        /// <returns>
        /// 表示异步操作的 <see cref="ValueTask{Result{T}}"/>，其结果为 <see cref="Result{T}"/>。
        /// 成功时 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。
        /// </returns>
        ValueTask<Result<int>> WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default);

        /// <summary>
        /// 异步将只读内存写入文件（指定写入位置）。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="memory">要写入的只读内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>
        /// 表示异步操作的 <see cref="ValueTask{Result{T}}"/>，其结果为 <see cref="Result{T}"/>。
        /// 成功时 <see cref="Result{T}.Value"/> 为实际写入的字节数（int）。
        /// </returns>
        ValueTask<Result<int>> WriteAsync(long filePosition, ReadOnlyMemory<byte> memory, CancellationToken token = default);

        /// <summary>
        /// 异步将只读序列写入文件（从文件起始位置写入）。
        /// </summary>
        /// <param name="sequence">要写入的只读序列。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>见 <see cref="WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>。</returns>
        ValueTask<Result<int>> WriteAsync(ReadOnlySequence<byte> sequence, CancellationToken token = default);

        /// <summary>
        /// 异步将只读序列写入文件（指定写入位置）。
        /// </summary>
        /// <param name="filePosition">文件起始写入位置（字节偏移）。</param>
        /// <param name="sequence">要写入的只读序列。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>见 <see cref="WriteAsync(ReadOnlyMemory{byte}, CancellationToken)"/>。</returns>
        ValueTask<Result<int>> WriteAsync(long filePosition, ReadOnlySequence<byte> sequence, CancellationToken token = default);

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
        /// <param name="bytesStart">目标数组的起始写入下标（从 0 开始）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <returns>
        /// 一个 <see cref="Result{T}"/>，成功时其 <see cref="Result{T}.Value"/> 为实际读取的字节数（int）。
        /// </returns>
        Result<int> Read(long filePosition, byte[] bytes, int bytesStart, int length);

        /// <summary>
        /// 从文件起始位置读取数据到给定的跨度。
        /// </summary>
        /// <param name="span">目标数据视图。</param>
        /// <returns>
        /// 一个 <see cref="Result{T}"/>，成功时其 <see cref="Result{T}.Value"/> 为实际读取的字节数（int）。
        /// </returns>
        Result<int> Read(Span<byte> span);

        /// <summary>
        /// 从指定位置读取数据到给定的跨度。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="span">目标数据视图。</param>
        /// <returns>见 <see cref="Read(Span{byte})"/>。</returns>
        Result<int> Read(long filePosition, Span<byte> span);

        /// <summary>
        /// 从文件起始位置读取数据到给定的内存块。
        /// </summary>
        /// <param name="memory">目标内存。</param>
        /// <returns>见 <see cref="Read(Span{byte})"/>。</returns>
        Result<int> Read(Memory<byte> memory);

        /// <summary>
        /// 从指定位置读取数据到给定的内存块。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="memory">目标内存。</param>
        /// <returns>见 <see cref="Read(Span{byte})"/>。</returns>
        Result<int> Read(long filePosition, Memory<byte> memory);

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步读取整个文件。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>，成功时包含读取到的字节数组。</returns>
        ValueTask<Result<byte[]>> ReadAsync(CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取指定长度的数据。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>，成功时包含读取到的字节数组。</returns>
        ValueTask<Result<byte[]>> ReadAsync(long filePosition, int length, CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取数据到目标数组。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="length">要读取的字节数（最多写入到 <paramref name="bytes"/> 的可用空间）。</param>
        /// <param name="bytes">目标数组。</param>
        /// <param name="bytesStart">目标数组的起始写入下标。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>
        /// 表示异步操作的 <see cref="ValueTask{Result{T}}"/>，其结果为实际读取的字节数（int）。
        /// </returns>
        ValueTask<Result<int>> ReadAsync(long filePosition, int length, byte[] bytes, int bytesStart, CancellationToken token = default);

        /// <summary>
        /// 异步从文件起始位置读取数据到内存块。
        /// </summary>
        /// <param name="memory">目标内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>，其结果为实际读取的字节数（int）。</returns>
        ValueTask<Result<int>> ReadAsync(Memory<byte> memory, CancellationToken token = default);

        /// <summary>
        /// 异步从指定位置读取数据到内存块。
        /// </summary>
        /// <param name="filePosition">文件起始读取位置（字节偏移）。</param>
        /// <param name="memory">目标内存。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示异步操作的 <see cref="ValueTask{Result{T}}"/>，其结果为实际读取的字节数（int）。</returns>
        ValueTask<Result<int>> ReadAsync(long filePosition, Memory<byte> memory, CancellationToken token = default);

        #endregion ReadAsync

        /// <summary>
        /// 将底层存储扩展至指定容量。实现应保证扩展后可用于后续读写操作。
        /// </summary>
        /// <param name="newCapacity">新的目标容量（字节）。</param>
        void ExpandCapacity(long newCapacity);

        /// <summary>
        /// 获取文件的唯一标识符 (GUID)。
        /// </summary>
        /// <returns>表示文件的唯一 <see cref="Guid"/>。</returns>
        Guid GetFileGuid();
    }
}