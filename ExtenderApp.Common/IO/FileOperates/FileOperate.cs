using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ExtenderApp.Abstract;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;
using Microsoft.Win32.SafeHandles;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件操作抽象基类：提供基于偏移的同步/异步读写、按策略扩容、以及容量与时间元信息维护。 具体的 IO 行为由派生类通过 ExecuteRead/ExecuteWrite* 系列抽象方法实现。
    /// </summary>
    public abstract class FileOperate : DisposableObject, IFileOperate
    {
        /// <summary>
        /// 文件读写时使用的默认分块大小（字节）。用于分批读取/写入以避免一次性分配过大缓冲区。
        /// </summary>
        private const int FileChunkSize = 32 * 1024; // 32KB 分块大小，适合大多数文件系统和应用场景

        /// <summary>
        /// 默认的容量扩展对齐粒度（字节）。在按策略预分配或对齐时使用（例如 Windows 常见的 64KB 对齐）。
        /// </summary>
        private const int AllocationGranularity = 64 * 1024; // 保守对齐（Windows 常见）

        /// <summary>
        /// 当前逻辑容量（字节）。通常等于底层文件长度。 扩容成功后应同步更新该值（派生类在 ChangeCapacity 中完成）。
        /// </summary>
        public long Capacity { get; private set; }

        /// <summary>
        /// 本地文件信息快照。是否实时刷新由上层代码决定。
        /// </summary>
        public LocalFileInfo Info => OperateInfo.LocalFileInfo;

        /// <summary>
        /// 打开/访问文件所需的上下文信息（路径、模式、访问权限等）。
        /// </summary>
        public FileOperateInfo OperateInfo { get; private set; }

        /// <summary>
        /// 最后一次成功执行读/写操作的时间。
        /// </summary>
        public DateTime LastOperateTime { get; private set; }

        /// <summary>
        /// 容量分配策略。派生类可在扩容时结合该策略（例如对齐预分配/稀疏文件等）。
        /// </summary>
        public AllocationStrategy AllocationStrategy { get; private set; }

        /// <summary>
        /// 受管的底层文件流。派生类可用其句柄进行 RandomAccess 操作。
        /// </summary>
        protected FileStream Stream { get; }

        /// <summary>
        /// 标记是否由外部宿主管理此实例（生命周期/资源）。
        /// </summary>
        public bool IsHosted { get; set; }

        /// <summary>
        /// 获取一个值，指示当前文件是否支持读取操作。
        /// </summary>
        public bool CanRead => Stream.CanRead;

        /// <summary>
        /// 获取一个值，指示当前文件是否支持写入操作。
        /// </summary>
        public bool CanWrite => Stream.CanWrite;

        /// <summary>
        /// 使用本地文件信息创建文件操作对象（具体打开参数由 LocalFileInfo 转换/适配）。
        /// </summary>
        /// <param name="info">本地文件信息。</param>
        public FileOperate(LocalFileInfo info) : this(operateInfo: info)
        {
        }

        /// <summary>
        /// 使用文件操作上下文创建文件操作对象并打开底层文件。
        /// </summary>
        /// <param name="operateInfo">文件操作上下文。</param>
        /// <exception cref="InvalidOperationException">当上下文为空时。</exception>
        public FileOperate(FileOperateInfo operateInfo)
        {
            if (operateInfo.IsEmpty)
            {
                throw new InvalidOperationException("文件信息为空，无法创建文件操控类。");
            }

            OperateInfo = operateInfo;
            Stream = operateInfo.OpenFile();
            Capacity = Info.Exists ? Info.FileSize : 1;
        }

        #region Write

        /// <inheritdoc/>
        public Result<int> Write(ReadOnlySpan<byte> span, long filePosition = 0)
        {
            try
            {
                if (span.Length == 0)
                    return Result.Success(0);

                if (span.IsEmpty)
                    throw new ArgumentNullException(nameof(span));
                if (filePosition < 0)
                    throw new ArgumentOutOfRangeException(nameof(filePosition));
                if (!CanWrite)
                    return Result.Failure<int>("文件不支持写入操作。");

                EnsureCapacityForWrite(filePosition, span.Length);
                ExecuteWrite(filePosition, span);
                LastOperateTime = DateTime.Now;
                return Result.Success(span.Length);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        /// <inheritdoc/>
        public Result<long> Write<TBuffer>(TBuffer buffer, long filePosition = 0) where TBuffer : AbstractBuffer<byte>
        {
            try
            {
                ThrowIfDisposed();
                ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
                if (!CanWrite)
                    return Result.Failure<long>("文件不支持写入操作。");

                var sequence = buffer.CommittedSequence;
                if (sequence.IsEmpty)
                    return Result.Success(0L);

                EnsureCapacityForWrite(filePosition, sequence.Length);

                buffer.Freeze();
                long written = ExecuteWrite(filePosition, sequence);

                LastOperateTime = DateTime.Now;
                return Result.Success(written);
            }
            catch (Exception ex)
            {
                return Result.FromException<long>(ex);
            }
            finally
            {
                buffer.TryRelease();
            }
        }

        /// <inheritdoc/>
        public Result<long> Write(ref BinaryReaderAdapter reader, long filePosition = 0)
        {
            try
            {
                ThrowIfDisposed();
                if (reader.IsEmpty)
                    return Result.Failure<long>("提供的读取器没有数据可写入。");
                if (!CanWrite)
                    return Result.Failure<long>("文件不支持写入操作。");
                if (reader.Remaining == 0)
                    return Result.Success(0L);

                EnsureCapacityForWrite(filePosition, reader.Remaining);

                long written = ExecuteWrite(filePosition, reader.UnreadSequence);

                LastOperateTime = DateTime.Now;
                return Result.Success(written);
            }
            catch (Exception ex)
            {
                return Result.FromException<long>(ex);
            }
        }

        #endregion Write

        #region WriteAsync

        /// <inheritdoc/>
        public async ValueTask<Result<long>> WriteAsync<TBuffer>(TBuffer buffer, long filePosition = 0, CancellationToken token = default) where TBuffer : AbstractBuffer<byte>
        {
            try
            {
                ThrowIfDisposed();
                ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
                if (!CanWrite)
                    return Result.Failure<long>("文件不支持写入操作。");

                var sequence = buffer.CommittedSequence;
                if (sequence.IsEmpty)
                    return Result.Success(0L);

                EnsureCapacityForWrite(filePosition, sequence.Length);

                buffer.Freeze();
                long written = await ExecuteWriteAsync(filePosition, sequence, token);

                LastOperateTime = DateTime.Now;
                return Result.Success(written);
            }
            catch (Exception ex)
            {
                return Result.FromException<long>(ex);
            }
            finally
            {
                buffer.TryRelease();
            }
        }

        #endregion WriteAsync

        #region Read

        /// <inheritdoc/>
        public Result<byte[]> Read(long filePosition = 0)
        {
            return Read((int)Info.Length, filePosition);
        }

        /// <inheritdoc/>
        public Result<byte[]> Read(int length, long filePosition = 0)
        {
            try
            {
                ThrowIfDisposed();
                if (length == 0)
                    return Result.Success(Array.Empty<byte>());
                if (filePosition < 0)
                    throw new ArgumentOutOfRangeException(nameof(filePosition));
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length));
                if (!CanRead)
                    return Result.Failure<byte[]>("文件不支持读取操作。");

                var result = ExecuteRead(filePosition, length);
                LastOperateTime = DateTime.Now;
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.FromException<byte[]>(ex);
            }
        }

        /// <inheritdoc/>
        public Result<int> Read(Span<byte> span, long filePosition = 0)
        {
            try
            {
                if (span.Length == 0)
                    return Result.Success(0);

                if (span.IsEmpty)
                    throw new ArgumentNullException(nameof(span));
                if (filePosition + span.Length > Info.Length)
                    throw new ArgumentOutOfRangeException(nameof(span), "读取范围超出文件长度。");
                if (!CanRead)
                    return Result.Failure<int>("文件不支持读取操作。");

                var read = ExecuteRead(filePosition, span);
                LastOperateTime = DateTime.Now;
                return Result.Success(read);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        public Result<long> Read(long length, ref BinaryWriterAdapter writer, long filePosition = 0)
        {
            try
            {
                ThrowIfDisposed();
                if (writer.IsEmpty)
                    return Result.Failure<long>("提供的写入器没有可用空间来写入数据。");
                if (length == 0)
                    return Result.Success(0L);
                if (filePosition < 0)
                    throw new ArgumentOutOfRangeException(nameof(filePosition));
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length));
                if (!CanRead)
                    return Result.Failure<long>("文件不支持读取操作。");

                long remaining = length;
                long totalRead = 0;
                while (remaining > 0)
                {
                    int readSize = (int)System.Math.Min(remaining, FileChunkSize);
                    var span = writer.GetSpan(readSize).Slice(0, readSize);
                    int read = ExecuteRead(filePosition + totalRead, span);
                    if (read <= 0)
                        break;

                    writer.Advance(read);
                    totalRead += read;
                    remaining -= read;

                    if (read < readSize)
                        break;
                }

                LastOperateTime = DateTime.Now;
                return Result.Success(totalRead);
            }
            catch (Exception ex)
            {
                return Result.FromException<long>(ex);
            }
        }

        /// <inheritdoc/>
        public Result<long> Read<TBuffer>(long length, TBuffer buffer, long filePosition = 0) where TBuffer : AbstractBuffer<byte>
        {
            try
            {
                ThrowIfDisposed();
                ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
                if (length == 0)
                    return Result.Success(0L);
                if (filePosition < 0)
                    throw new ArgumentOutOfRangeException(nameof(filePosition));
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length));
                if (!CanRead)
                    return Result.Failure<long>("文件不支持读取操作。");

                buffer.Freeze();
                long remaining = length;
                long totalRead = 0;
                while (remaining > 0)
                {
                    int readSize = (int)System.Math.Min(remaining, FileChunkSize);
                    var span = buffer.GetSpan(readSize).Slice(0, readSize);
                    int read = ExecuteRead(filePosition + totalRead, span);
                    if (read <= 0)
                        break;

                    buffer.Advance(read);
                    totalRead += read;
                    remaining -= read;

                    if (read < readSize)
                        break;
                }

                LastOperateTime = DateTime.Now;
                return Result.Success(totalRead);
            }
            catch (Exception ex)
            {
                return Result.FromException<long>(ex);
            }
        }

        /// <inheritdoc/>
        public Result<long> Read(long length, out AbstractBuffer<byte> buffer, long filePosition = 0)
        {
            buffer = default;
            Result<long> result = default;
            try
            {
                if (length < FileChunkSize)
                {
                    var block = MemoryBlock<byte>.GetBuffer((int)length);
                    buffer = block;
                    result = Read(length, block, filePosition);
                }
                else
                {
                    SequenceBuffer<byte> sequence = SequenceBuffer<byte>.GetBuffer();
                    buffer = sequence;
                    result = Read(length, sequence, filePosition);
                }
                return Result.Success(result)!;
            }
            catch (Exception ex)
            {
                buffer?.TryRelease();
                return Result.FromException<Result<long>>(ex);
            }
        }

        #endregion Read

        #region ReadAsync

        /// <inheritdoc/>
        public ValueTask<Result<byte[]>> ReadAsync(long filePosition = 0, CancellationToken token = default)
        {
            return ReadAsync((int)Info.Length, filePosition, token);
        }

        /// <inheritdoc/>
        public async ValueTask<Result<byte[]>> ReadAsync(int length, long filePosition = 0, CancellationToken token = default)
        {
            try
            {
                if (length == 0)
                    return Result.Success(Array.Empty<byte>());
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length), "读取长度必须大于零。");

                if (!CanRead)
                    return Result.Failure<byte[]>("文件不支持读取操作。");

                var result = await ExecuteReadAsync(filePosition, length, token);
                LastOperateTime = DateTime.Now;
                return Result.Success(result);
            }
            catch (Exception ex)
            {
                return Result.FromException<byte[]>(ex);
            }
        }

        /// <inheritdoc/>
        public async ValueTask<Result<AbstractBuffer<byte>>> ReadAsync(long length, long filePosition = 0, CancellationToken token = default)
        {
            AbstractBuffer<byte> buffer = default!;
            try
            {
                if (length < FileChunkSize)
                {
                    var block = MemoryBlock<byte>.GetBuffer((int)length);
                    buffer = block;
                    var result = await ReadAsync(length, block, filePosition, token);
                }
                else
                {
                    SequenceBuffer<byte> sequence = SequenceBuffer<byte>.GetBuffer();
                    buffer = sequence;
                    await ReadAsync(length, sequence, filePosition, token);
                }
                return Result.Success(buffer)!;
            }
            catch (Exception ex)
            {
                buffer?.TryRelease();
                return Result.FromException<AbstractBuffer<byte>>(ex);
            }
        }

        /// <inheritdoc/>
        public async ValueTask<Result<long>> ReadAsync<TBuffer>(long length, TBuffer buffer, long filePosition = 0, CancellationToken token = default) where TBuffer : AbstractBuffer<byte>
        {
            try
            {
                ThrowIfDisposed();
                ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
                if (length == 0)
                    return Result.Success(0L);
                if (filePosition < 0)
                    throw new ArgumentOutOfRangeException(nameof(filePosition));
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length));
                if (!CanRead)
                    return Result.Failure<long>("文件不支持读取操作。");

                buffer.Freeze();
                long remaining = length;
                long totalRead = 0;
                while (remaining > 0)
                {
                    int readSize = (int)System.Math.Min(remaining, FileChunkSize);
                    var memory = buffer.GetMemory(readSize).Slice(0, readSize);
                    int read = (int)await ExecuteReadAsync(filePosition + totalRead, memory, token);
                    if (read <= 0)
                        break;

                    buffer.Advance(read);
                    totalRead += read;
                    remaining -= read;

                    if (read < readSize)
                        break;
                }

                LastOperateTime = DateTime.Now;
                return Result.Success(totalRead);
            }
            catch (Exception ex)
            {
                return Result.FromException<long>(ex);
            }
        }

        #endregion ReadAsync

        #region Execute

        #region ExecuteWrite

        /// <summary>
        /// 派生类同步写入实现：将 span 写入到 filePosition。
        /// </summary>
        protected abstract long ExecuteWrite(long filePosition, ReadOnlySpan<byte> span);

        /// <summary>
        /// 派生类同步写入实现：将缓冲区内容写入到 filePosition，并返回实际写入的字节数。
        /// </summary>
        /// <param name="filePosition">写入的文件偏移位置。</param>
        /// <param name="sequence">提供数据的只读序列。</param>
        /// <returns>实际写入的字节数。</returns>
        protected abstract long ExecuteWrite(long filePosition, ReadOnlySequence<byte> sequence);

        /// <summary>
        /// 派生类异步写入实现：将缓冲区内容写入到 filePosition，并返回实际写入的字节数。
        /// </summary>
        protected abstract ValueTask<long> ExecuteWriteAsync(long filePosition, ReadOnlySequence<byte> sequence, CancellationToken token);

        #endregion ExecuteWrite

        #region ExecuteRead

        /// <summary>
        /// 派生类同步读取实现：从 filePosition 读取 _intLength 字节并返回。
        /// </summary>
        protected abstract byte[] ExecuteRead(long filePosition, int length);

        /// <summary>
        /// 派生类同步读取实现：从 filePosition 读取并填充 span，返回实际读取。
        /// </summary>
        protected abstract int ExecuteRead(long filePosition, Span<byte> span);

        /// <summary>
        /// 派生类异步读取实现：从 filePosition 读取 _intLength 字节并返回。
        /// </summary>
        protected abstract ValueTask<byte[]> ExecuteReadAsync(long filePosition, int length, CancellationToken token);

        /// <summary>
        /// 派生类异步读取实现：从 filePosition 读取并填充 memory，返回实际读取。
        /// </summary>
        protected abstract ValueTask<long> ExecuteReadAsync(long filePosition, Memory<byte> memory, CancellationToken token);

        #endregion ExecuteRead

        #endregion Execute

        /// <summary>
        /// 确保写入 [filePosition, filePosition + count) 区间前容量足够；不足则按策略扩容。
        /// </summary>
        private void EnsureCapacityForWrite(long filePosition, long count)
        {
            ThrowIfDisposed();
            if (filePosition < 0) throw new ArgumentOutOfRangeException(nameof(filePosition));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            long end;
            try { end = checked(filePosition + count); }
            catch (OverflowException) { throw new ArgumentOutOfRangeException("写入区间溢出。"); }

            if (end <= Capacity) return;

            long target = (AllocationStrategy == AllocationStrategy.PreallocateAligned)
                ? AlignUp(end, AllocationGranularity)
                : end;

            ExpandCapacity(target);
        }

        #region AllocationStrategy

        /// <summary>
        /// 向上对齐数值到 alignment 的整数倍。
        /// </summary>
        private static long AlignUp(long value, long alignment)
            => ((value + alignment - 1) / alignment) * alignment;

        /// <summary>
        /// 在 Windows 上尝试将文件标记为稀疏，以便预留“空洞”区间。
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static bool TryMarkSparse(SafeFileHandle handle)
        {
            const uint FSCTL_SET_SPARSE = 0x000900C4;
            return DeviceIoControl(handle, FSCTL_SET_SPARSE, IntPtr.Zero, 0, IntPtr.Zero, 0, out _, IntPtr.Zero);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool DeviceIoControl(
                SafeFileHandle hDevice,
                uint dwIoControlCode,
                IntPtr lpInBuffer,
                int nInBufferSize,
                IntPtr lpOutBuffer,
                int nOutBufferSize,
                out int lpBytesReturned,
                IntPtr lpOverlapped);
        }

        #endregion AllocationStrategy

        #region ExpandCanpacity

        /// <summary>
        /// 将底层存储扩展至指定容量，并更新最后操作时间。 实际扩容由派生类在 <see cref="ChangeCapacity"/> 中实现。 调用方应避免与其他并发写操作同时调用（本方法未加锁，通常由写入路径持锁后调用）。
        /// </summary>
        public void ExpandCapacity(long newCapacity)
        {
            LastOperateTime = DateTime.Now;
            ChangeCapacity(newCapacity);
            Capacity = newCapacity;
        }

        /// <summary>
        /// 派生类扩容实现：应设置底层长度。
        /// </summary>
        protected abstract void ChangeCapacity(long length);

        #endregion ExpandCanpacity

        #region GetFileGuid

        /// <summary>
        /// 用于调用 Win32 API 获取文件句柄信息的结构映射。仅在 Windows 平台用于内部文件标识/元信息检索。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        /// <summary>
        /// 调用 Win32 的 GetFileInformationByHandle 来检索文件相关的底层信息（仅 Windows）。
        /// </summary>
        /// <param name="hFile">要查询的文件句柄。</param>
        /// <param name="lpFileInformation">输出的文件信息结构。</param>
        /// <returns>如果调用成功则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        #endregion GetFileGuid

        /// <inheritdoc/>
        protected override void DisposeManagedResources()
        {
            Stream.Dispose();
        }
    }
}