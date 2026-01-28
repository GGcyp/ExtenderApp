using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Caches;
using ExtenderApp.Data;
using Microsoft.Win32.SafeHandles;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件操作抽象基类：提供基于偏移的同步/异步读写、按策略扩容、以及容量与时间元信息维护。
    /// 具体的 IO 行为由派生类通过 ExecuteRead/ExecuteWrite* 系列抽象方法实现。
    /// </summary>
    public abstract class FileOperate : DisposableObject, IFileOperate
    {
        private const int AllocationGranularity = 64 * 1024; // 保守对齐（Windows 常见）

        /// <summary>
        /// 当前逻辑容量（字节）。通常等于底层文件长度。
        /// 扩容成功后应同步更新该值（派生类在 ChangeCapacity 中完成）。
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

        #region Read

        /// <inheritdoc/>
        public Result<byte[]> Read()
        {
            return Read(0, (int)Info.Length);
        }

        /// <inheritdoc/>
        public Result<byte[]> Read(long filePosition, int length)
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
        public Result<int> Read(long filePosition, byte[] bytes, int bytesStart, int length)
        {
            try
            {
                ThrowIfDisposed();
                if (length == 0) return Result.Success(0);
                if (bytes is null) throw new ArgumentNullException(nameof(bytes));
                if (bytesStart < 0 || length < 0 || bytesStart > bytes.Length - length)
                    throw new ArgumentOutOfRangeException("目标数组区间无效。");
                if (filePosition < 0) throw new ArgumentOutOfRangeException(nameof(filePosition));
                if (!CanRead) return Result.Failure<int>("文件不支持读取操作。");

                var read = ExecuteRead(filePosition, bytes, bytesStart, length);
                LastOperateTime = DateTime.Now;
                return Result.Success(read);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        /// <inheritdoc/>
        public Result<int> Read(Span<byte> span)
        {
            return Read(0, span);
        }

        /// <inheritdoc/>
        public Result<int> Read(long filePosition, Span<byte> span)
        {
            try
            {
                if (span.Length == 0)
                    return Result.Success(0);

                CheckSpan(span);
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

        /// <inheritdoc/>
        public Result<int> Read(Memory<byte> memory)
        {
            return Read(0, memory);
        }

        /// <inheritdoc/>
        public Result<int> Read(long filePosition, Memory<byte> memory)
        {
            try
            {
                if (memory.Length == 0)
                    return Result.Success(0);

                if (!CanRead)
                    return Result.Failure<int>("文件不支持读取操作。");

                var read = ExecuteRead(filePosition, memory);
                LastOperateTime = DateTime.Now;
                return Result.Success(read);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        #endregion Read

        #region ReadAsync

        /// <inheritdoc/>
        public ValueTask<Result<byte[]>> ReadAsync(CancellationToken token = default)
        {
            return ReadAsync(0, (int)Info.Length, token);
        }

        /// <inheritdoc/>
        public async ValueTask<Result<byte[]>> ReadAsync(long filePosition, int length, CancellationToken token = default)
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
        public async ValueTask<Result<int>> ReadAsync(long filePosition, int length, byte[] bytes, int bytesStart, CancellationToken token = default)
        {
            try
            {
                if (length == 0)
                    return Result.Success(0);
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length), "读取长度必须大于零。");

                CheckBytes(bytes);
                if (bytesStart < 0 || bytesStart >= bytes.Length)
                    throw new ArgumentOutOfRangeException(nameof(bytesStart));
                if (filePosition + length > Info.Length)
                    throw new ArgumentOutOfRangeException(nameof(length), "读取范围超出文件长度。");
                if (!CanRead)
                    return Result.Failure<int>("文件不支持读取操作。");

                var read = await ExecuteReadAsync(filePosition, bytes, bytesStart, length, token);
                LastOperateTime = DateTime.Now;
                return Result.Success(read);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        /// <inheritdoc/>
        public ValueTask<Result<int>> ReadAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return ReadAsync(0, memory, token);
        }

        /// <inheritdoc/>
        public async ValueTask<Result<int>> ReadAsync(long filePosition, Memory<byte> memory, CancellationToken token = default)
        {
            try
            {
                if (memory.Length == 0)
                    return Result.Success(0);

                CheckMemory(memory);
                if (!CanRead)
                    return Result.Failure<int>("文件不支持读取操作。");

                var read = await ExecuteReadAsync(filePosition, memory, token);
                LastOperateTime = DateTime.Now;
                return Result.Success(read);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        #endregion ReadAsync

        #region Write

        /// <inheritdoc/>
        public Result<int> Write(ReadOnlySpan<byte> span)
        {
            return Write(0, span);
        }

        /// <inheritdoc/>
        public Result<int> Write(long filePosition, ReadOnlySpan<byte> span)
        {
            try
            {
                if (span.Length == 0)
                    return Result.Success(0);

                CheckSpan(span);
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
        public Result<int> Write(ReadOnlyMemory<byte> memory)
        {
            return Write(0, memory);
        }

        /// <inheritdoc/>
        public Result<int> Write(long filePosition, ReadOnlyMemory<byte> memory)
        {
            try
            {
                if (memory.Length == 0)
                    return Result.Success(0);

                CheckMemory(memory);
                if (!CanWrite)
                    return Result.Failure<int>("文件不支持写入操作。");

                EnsureCapacityForWrite(filePosition, memory.Length);
                ExecuteWrite(filePosition, memory);
                LastOperateTime = DateTime.Now;
                return Result.Success(memory.Length);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        /// <inheritdoc/>
        public Result<int> Write(ReadOnlySequence<byte> sequence)
        {
            return Write(0, sequence);
        }

        /// <inheritdoc/>
        public Result<int> Write(long filePosition, ReadOnlySequence<byte> sequence)
        {
            try
            {
                if (sequence.Length == 0)
                    return Result.Success(0);
                CheckSequence(sequence);
                if (!CanWrite)
                    return Result.Failure<int>("文件不支持写入操作。");

                EnsureCapacityForWrite(filePosition, sequence.Length);

                foreach (var memory in sequence)
                {
                    ExecuteWrite(filePosition, memory);
                    filePosition += memory.Length;
                }
                return Result.Success((int)sequence.Length);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        #endregion Write

        #region WriteAsync

        /// <inheritdoc/>
        public ValueTask<Result<int>> WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
        {
            return WriteAsync(0, memory, token);
        }

        /// <inheritdoc/>
        public async ValueTask<Result<int>> WriteAsync(long filePosition, ReadOnlyMemory<byte> memory, CancellationToken token = default)
        {
            try
            {
                if (memory.Length == 0)
                    return Result.Success(0);

                CheckMemory(memory);
                if (!CanWrite)
                    return Result.Failure<int>("文件不支持写入操作。");

                EnsureCapacityForWrite(filePosition, memory.Length);
                await ExecuteWriteAsync(filePosition, memory, token);
                LastOperateTime = DateTime.Now;
                return Result.Success(memory.Length);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        public ValueTask<Result<int>> WriteAsync(ReadOnlySequence<byte> sequence, CancellationToken token = default)
        {
            return WriteAsync(0, sequence, token);
        }

        public async ValueTask<Result<int>> WriteAsync(long filePosition, ReadOnlySequence<byte> sequence, CancellationToken token = default)
        {
            try
            {
                if (sequence.Length == 0)
                    return Result.Success(0);
                if (!CanWrite)
                    return Result.Failure<int>("文件不支持写入操作。");

                CheckSequence(sequence);
                EnsureCapacityForWrite(filePosition, sequence.Length);

                foreach (var memory in sequence)
                {
                    await ExecuteWriteAsync(filePosition, memory, token);
                    filePosition += memory.Length;
                }
                LastOperateTime = DateTime.Now;
                return Result.Success((int)sequence.Length);
            }
            catch (Exception ex)
            {
                return Result.FromException<int>(ex);
            }
        }

        #endregion WriteAsync

        #region Check

        /// <summary>
        /// 校验字节数组非空。
        /// </summary>
        private void CheckBytes([NotNull] byte[] bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
        }

        /// <summary>
        /// 校验可写跨度非空。
        /// </summary>
        private void CheckSpan(Span<byte> span)
        {
            if (span.IsEmpty)
            {
                throw new ArgumentNullException(nameof(span));
            }
        }

        /// <summary>
        /// 校验只读跨度非空。
        /// </summary>
        private void CheckSpan(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
            {
                throw new ArgumentNullException(nameof(span));
            }
        }

        /// <summary>
        /// 校验只读内存非空。
        /// </summary>
        private void CheckMemory(ReadOnlyMemory<byte> memory)
        {
            if (memory.IsEmpty)
            {
                throw new ArgumentNullException(nameof(memory));
            }
        }

        private void CheckSequence(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsEmpty)
            {
                throw new ArgumentNullException(nameof(sequence));
            }
        }

        #endregion Check

        #region Execute

        #region ExecuteWrite

        /// <summary>
        /// 派生类同步写入实现：将 span 写入到 filePosition。
        /// </summary>
        protected abstract void ExecuteWrite(long filePosition, ReadOnlySpan<byte> span);

        /// <summary>
        /// 派生类同步写入实现：将 memory 写入到 filePosition。
        /// </summary>
        protected abstract void ExecuteWrite(long filePosition, ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 派生类异步写入实现：将 memory 写入到 filePosition。
        /// </summary>
        protected abstract ValueTask ExecuteWriteAsync(long filePosition, ReadOnlyMemory<byte> memory, CancellationToken token);

        #endregion ExecuteWrite

        #region ExecuteRead

        /// <summary>
        /// 派生类同步读取实现：从 filePosition 读取 length 字节并返回。
        /// </summary>
        protected abstract byte[] ExecuteRead(long filePosition, int length);

        /// <summary>
        /// 派生类同步读取实现：从 filePosition 读取 length 字节到 bytes[bytesStart..)，返回实际读取。
        /// </summary>
        protected abstract int ExecuteRead(long filePosition, byte[] bytes, int bytesStart, int length);

        /// <summary>
        /// 派生类同步读取实现：从 filePosition 读取并填充 span，返回实际读取。
        /// </summary>
        protected abstract int ExecuteRead(long filePosition, Span<byte> span);

        /// <summary>
        /// 派生类同步读取实现：从 filePosition 读取并填充 memory，返回实际读取。
        /// </summary>
        protected abstract int ExecuteRead(long filePosition, Memory<byte> memory);

        /// <summary>
        /// 派生类异步读取实现：从 filePosition 读取 length 字节并返回。
        /// </summary>
        protected abstract ValueTask<byte[]> ExecuteReadAsync(long filePosition, int length, CancellationToken token);

        /// <summary>
        /// 派生类异步读取实现：从 filePosition 读取 length 字节到 bytes[bytesStart..)，返回实际读取。
        /// </summary>
        protected abstract ValueTask<int> ExecuteReadAsync(long filePosition, byte[] bytes, int bytesStart, int length, CancellationToken token);

        /// <summary>
        /// 派生类异步读取实现：从 filePosition 读取并填充 memory，返回实际读取。
        /// </summary>
        protected abstract ValueTask<int> ExecuteReadAsync(long filePosition, Memory<byte> memory, CancellationToken token);

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
        /// 将底层存储扩展至指定容量，并更新最后操作时间。
        /// 实际扩容由派生类在 <see cref="ChangeCapacity"/> 中实现。
        /// 调用方应避免与其他并发写操作同时调用（本方法未加锁，通常由写入路径持锁后调用）。
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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        /// <summary>
        /// 获取文件的唯一标识符 (GUID)。
        /// 在 Windows 上，此 GUID 基于卷序列号和文件索引号，即使文件移动或重命名也能保持不变。
        /// 在其他操作系统上，它基于文件完整路径的 SHA1 哈希值。
        /// </summary>
        /// <returns>表示文件的唯一 Guid。</returns>
        public Guid GetFileGuid()
        {
            if (OperatingSystem.IsWindows())
            {
                if (GetFileInformationByHandle(Stream.SafeFileHandle, out var fileInfo))
                {
                    long fileId = ((long)fileInfo.FileIndexHigh << 32) | fileInfo.FileIndexLow;
                    Span<byte> guidSpan = stackalloc byte[16];
                    BitConverter.GetBytes(fileId).CopyTo(guidSpan.Slice(0, 8));
                    BitConverter.GetBytes(fileInfo.VolumeSerialNumber).CopyTo(guidSpan.Slice(8, 8));
                    return new Guid(guidSpan);
                }
            }

            return Info.FileName.GetGuid();
        }

        #endregion GetFileGuid

        protected override void DisposeManagedResources()
        {
            Stream.Dispose();
        }
    }
}