using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ExtenderApp.Abstract;
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
            Capacity = Info.FileSize;
            Stream = operateInfo.OpenFile();
        }

        #region Read

        /// <summary>
        /// 读取整个文件并返回字节数组。
        /// </summary>
        public byte[] Read()
        {
            return Read(0, (int)Info.Length);
        }

        /// <summary>
        /// 从指定位置读取指定长度的数据并返回字节数组。
        /// </summary>
        /// <param name="filePosition">起始偏移。</param>
        /// <param name="length">读取长度。</param>
        /// <exception cref="ArgumentOutOfRangeException">读取范围越界。</exception>
        public byte[] Read(long filePosition, int length)
        {
            if (filePosition + length > Info.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "读取范围超出文件长度。");
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");


            LastOperateTime = DateTime.Now;
            return ExecuteRead(filePosition, length);
        }

        /// <summary>
        /// 读取整个文件，尽量使用内部数组池以减少分配。
        /// </summary>
        /// <param name="length">输出：实际读取长度。</param>
        public byte[] ReadForArrayPool(out int length)
        {
            length = (int)Info.Length;
            return ReadForArrayPool(0, length);
        }

        /// <summary>
        /// 从指定位置读取指定长度的数据，尽量使用内部数组池以减少分配。
        /// </summary>
        /// <param name="filePosition">起始偏移。</param>
        /// <param name="length">读取长度。</param>
        /// <exception cref="ArgumentOutOfRangeException">读取范围越界。</exception>
        public byte[] ReadForArrayPool(long filePosition, int length)
        {
            if (filePosition + length > Info.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "读取范围超出文件长度。");
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");

            LastOperateTime = DateTime.Now;
            return ExecuteReadForArrayPool(filePosition, length);
        }

        /// <summary>
        /// 从指定位置读取指定长度的数据到目标数组。
        /// </summary>
        /// <returns>实际读取的字节数。</returns>
        public int Read(long filePosition, byte[] bytes, int bytesStart, int length)
        {
            CheckBytes(bytes);
            if (bytesStart < 0 || bytesStart >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesStart));
            if (filePosition + length > Info.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "读取范围超出文件长度。");
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");

            LastOperateTime = DateTime.Now;
            return ExecuteRead(filePosition, bytes, bytesStart, length);
        }

        /// <summary>
        /// 从文件起始位置读取数据到给定跨度。
        /// </summary>
        public int Read(Span<byte> span)
        {
            return Read(0, span);
        }

        /// <summary>
        /// 从指定位置读取数据到给定跨度。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">读取范围越界。</exception>
        public int Read(long filePosition, Span<byte> span)
        {
            CheckSpan(span);
            if (filePosition + span.Length > Info.Length)
                throw new ArgumentOutOfRangeException(nameof(span), "读取范围超出文件长度。");
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");

            LastOperateTime = DateTime.Now;
            return ExecuteRead(filePosition, span);
        }

        /// <summary>
        /// 从文件起始位置读取数据到给定内存块。
        /// </summary>
        public int Read(Memory<byte> memory)
        {
            return Read(0, memory);
        }

        /// <summary>
        /// 从指定位置读取数据到给定内存块。
        /// </summary>
        public int Read(long filePosition, Memory<byte> memory)
        {
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");

            LastOperateTime = DateTime.Now;
            return ExecuteRead(filePosition, memory);
        }

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步读取整个文件。
        /// </summary>
        public ValueTask<byte[]> ReadAsync(CancellationToken token = default)
        {
            return ReadAsync(0, (int)Info.Length, token);
        }

        /// <summary>
        /// 异步从指定位置读取指定长度的数据并返回字节数组。
        /// </summary>
        public ValueTask<byte[]> ReadAsync(long filePosition, int length, CancellationToken token = default)
        {
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");

            LastOperateTime = DateTime.Now;
            return ExecuteReadAsync(filePosition, length, token);
        }

        /// <summary>
        /// 异步从指定位置读取指定长度的数据，尽量使用数组池。
        /// </summary>
        public ValueTask<byte[]> ReadForArrayPoolAsync(long filePosition, int length, CancellationToken token = default)
        {
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");

            LastOperateTime = DateTime.Now;
            return ExecuteReadForArrayPoolAsync(filePosition, length, token);
        }

        /// <summary>
        /// 异步读取到目标数组。
        /// </summary>
        /// <returns>实际读取的字节数。</returns>
        public ValueTask<int> ReadAsync(long filePosition, int length, byte[] bytes, int bytesStart, CancellationToken token = default)
        {
            CheckBytes(bytes);
            if (bytesStart < 0 || bytesStart >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesStart));
            if (filePosition + length > Info.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "读取范围超出文件长度。");
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");

            LastOperateTime = DateTime.Now;
            return ExecuteReadAsync(filePosition, bytes, bytesStart, length, token);
        }

        /// <summary>
        /// 异步读取整个文件，尽量使用数组池（返回数组用后可归还）。
        /// </summary>
        public ValueTask<byte[]> ReadForArrayPoolAsync(out int length, CancellationToken token = default)
        {
            length = (int)Info.Length;
            return ReadForArrayPoolAsync(0, length, token);
        }

        /// <summary>
        /// 异步从文件起始位置读取到内存块。
        /// </summary>
        public ValueTask<int> ReadAsync(Memory<byte> memory, CancellationToken token = default)
        {
            return ReadAsync(0, memory, token);
        }

        /// <summary>
        /// 异步从指定位置读取到内存块。
        /// </summary>
        public ValueTask<int> ReadAsync(long filePosition, Memory<byte> memory, CancellationToken token = default)
        {
            CheckMemory(memory);
            if (!CanRead)
                throw new InvalidOperationException("文件不支持读取操作。");

            LastOperateTime = DateTime.Now;
            return ExecuteReadAsync(filePosition, memory, token);
        }

        #endregion ReadAsync

        #region Write

        /// <summary>
        /// 将整个字节数组写入文件起始位置。
        /// </summary>
        public void Write(byte[] bytes)
        {
            Write(0, bytes);
        }

        /// <summary>
        /// 将字节数组写入到指定文件偏移。
        /// </summary>
        public void Write(long filePosition, byte[] bytes)
        {
            Write(filePosition, bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 将字节数组的指定区间写入到指定文件偏移。
        /// </summary>
        public void Write(long filePosition, byte[] bytes, int bytesPosition, int bytesLength)
        {
            CheckBytes(bytes);
            if (bytesPosition < 0 || bytesPosition >= bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesPosition));
            if (!CanWrite)
                throw new InvalidOperationException("文件不支持写入操作。");

            LastOperateTime = DateTime.Now;
            EnsureCapacityForWrite(filePosition, bytesLength);
            ExecuteWrite(filePosition, bytes, bytesPosition, bytesLength);
        }

        /// <summary>
        /// 将 ExtenderBinaryWriter 的已提交数据写入文件起始位置。
        /// </summary>
        public void Write(ExtenderBinaryWriter writer)
        {
            Write(0, writer);
        }

        /// <summary>
        /// 将 ExtenderBinaryWriter 的已提交数据写入到指定文件偏移。
        /// </summary>
        public void Write(long filePosition, ExtenderBinaryWriter writer)
        {
            CheckWriter(writer);
            if (!CanWrite)
                throw new InvalidOperationException("文件不支持写入操作。");

            writer.Commit();
            EnsureCapacityForWrite(filePosition, writer.BytesCommitted);
            ExecuteWrite(filePosition, writer);
            LastOperateTime = DateTime.Now;
        }

        /// <summary>
        /// 将 ExtenderBinaryReader 中剩余未读数据写入文件起始位置。
        /// </summary>
        public void Write(ExtenderBinaryReader reader)
        {
            Write(0, reader);
        }

        /// <summary>
        /// 将 ExtenderBinaryReader 中剩余未读数据写入到指定文件偏移。
        /// </summary>
        public void Write(long filePosition, ExtenderBinaryReader reader)
        {
            CheckReader(reader);
            if (!CanWrite)
                throw new InvalidOperationException("文件不支持写入操作。");

            EnsureCapacityForWrite(filePosition, reader.Remaining);
            ExecuteWrite(filePosition, reader);
            LastOperateTime = DateTime.Now;
        }

        /// <summary>
        /// 将只读跨度写入文件起始位置。
        /// </summary>
        public void Write(ReadOnlySpan<byte> span)
        {
            Write(0, span);
        }

        /// <summary>
        /// 将只读跨度写入到指定文件偏移。
        /// </summary>
        public void Write(long filePosition, ReadOnlySpan<byte> span)
        {
            CheckSpan(span);
            if (!CanWrite)
                throw new InvalidOperationException("文件不支持写入操作。");


            EnsureCapacityForWrite(filePosition, span.Length);
            ExecuteWrite(filePosition, span);
            LastOperateTime = DateTime.Now;
        }

        /// <summary>
        /// 将只读内存写入文件起始位置。
        /// </summary>
        public void Write(ReadOnlyMemory<byte> memory)
        {
            Write(0, memory);
        }

        /// <summary>
        /// 将只读内存写入到指定文件偏移。
        /// </summary>
        public void Write(long filePosition, ReadOnlyMemory<byte> memory)
        {
            CheckMemory(memory);
            if (!CanWrite)
                throw new InvalidOperationException("文件不支持写入操作。");

            EnsureCapacityForWrite(filePosition, memory.Length);
            ExecuteWrite(filePosition, memory);
            LastOperateTime = DateTime.Now;
        }

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步将整个字节数组写入文件起始位置。
        /// </summary>
        public ValueTask WriteAsync(byte[] bytes, CancellationToken token = default)
        {
            return WriteAsync(0, bytes, token);
        }

        /// <summary>
        /// 异步将字节数组写入到指定文件偏移。
        /// </summary>
        public ValueTask WriteAsync(long filePosition, byte[] bytes, CancellationToken token = default)
        {
            return WriteAsync(filePosition, bytes, 0, bytes.Length, token);
        }

        /// <summary>
        /// 异步将字节数组的指定区间写入到指定文件偏移。
        /// </summary>
        public ValueTask WriteAsync(long filePosition, byte[] bytes, int bytesPosition, int bytesLength, CancellationToken token = default)
        {
            CheckBytes(bytes);
            if (!CanWrite)
                throw new InvalidOperationException("文件不支持写入操作。");

            LastOperateTime = DateTime.Now;
            EnsureCapacityForWrite(filePosition, bytesLength);
            return ExecuteWriteAsync(filePosition, bytes, bytesPosition, bytesLength, token);
        }

        /// <summary>
        /// 异步将只读内存写入文件起始位置。
        /// </summary>
        public ValueTask WriteAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
        {
            return WriteAsync(0, memory, token);
        }

        /// <summary>
        /// 异步将只读内存写入到指定文件偏移。
        /// </summary>
        public ValueTask WriteAsync(long filePosition, ReadOnlyMemory<byte> memory, CancellationToken token = default)
        {
            CheckMemory(memory);
            if (!CanWrite)
                throw new InvalidOperationException("文件不支持写入操作。");

            LastOperateTime = DateTime.Now;
            EnsureCapacityForWrite(filePosition, memory.Length);
            return ExecuteWriteAsync(filePosition, memory, token);
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
        /// 校验写入器状态合法且有数据。
        /// </summary>
        private void CheckWriter(ExtenderBinaryWriter writer)
        {
            if (writer.IsEmpty)
            {
                throw new ArgumentNullException(nameof(writer));
            }
        }

        /// <summary>
        /// 校验读取器状态合法且有数据。
        /// </summary>
        private void CheckReader(ExtenderBinaryReader reader)
        {
            if (reader.IsEmpty)
            {
                throw new ArgumentNullException(nameof(reader));
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

        #endregion Check

        #region Execute

        #region ExecuteWrite

        /// <summary>
        /// 派生类同步写入实现：将 bytes[bytesPosition..bytesPosition+bytesLength) 写入到 filePosition。
        /// </summary>
        protected abstract void ExecuteWrite(long filePosition, byte[] bytes, int bytesPosition, int bytesLength);

        /// <summary>
        /// 派生类同步写入实现：将读取器剩余数据写入到 filePosition。
        /// </summary>
        protected abstract void ExecuteWrite(long filePosition, ExtenderBinaryReader reader);

        /// <summary>
        /// 派生类同步写入实现：将 span 写入到 filePosition。
        /// </summary>
        protected abstract void ExecuteWrite(long filePosition, ReadOnlySpan<byte> span);

        /// <summary>
        /// 派生类同步写入实现：将 memory 写入到 filePosition。
        /// </summary>
        protected abstract void ExecuteWrite(long filePosition, ReadOnlyMemory<byte> memory);

        /// <summary>
        /// 派生类异步写入实现：将 bytes 指定区间写入到 filePosition。
        /// </summary>
        protected abstract ValueTask ExecuteWriteAsync(long filePosition, byte[] bytes, int bytesPosition, int bytesLength, CancellationToken token);

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
        /// 派生类同步读取（数组池）实现：从 filePosition 读取 length 字节并返回数组（通常来自 ArrayPool）。
        /// </summary>
        protected abstract byte[] ExecuteReadForArrayPool(long filePosition, int length);

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

        /// <summary>
        /// 派生类异步读取（数组池）实现：从 filePosition 读取 length 字节并返回数组（通常来自 ArrayPool）。
        /// </summary>
        protected abstract ValueTask<byte[]> ExecuteReadForArrayPoolAsync(long filePosition, int length, CancellationToken token);

        #endregion ExecuteRead

        #endregion Execute

        /// <summary>
        /// 确保写入 [filePosition, filePosition + count) 区间前容量足够；不足则按策略扩容。
        /// </summary>
        private void EnsureCapacityForWrite(long filePosition, long count)
        {
            ThrowIfDisposed();

            long end = filePosition + count;
            if (end <= Capacity) return;

            // 简单按需扩至 end；当策略为对齐预分配时，向上对齐到分配粒度
            long target = end;
            if (AllocationStrategy == AllocationStrategy.PreallocateAligned)
                target = AlignUp(target, AllocationGranularity);

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

        /// <summary>
        /// 释放底层文件流等托管资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            Stream.Dispose();
        }
    }
}