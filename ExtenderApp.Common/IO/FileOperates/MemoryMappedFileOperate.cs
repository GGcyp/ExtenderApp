using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件并发操作类，继承自ConcurrentOperate泛型类
    /// </summary>
    /// <typeparam name="FileOperatePolicy">文件操作策略类型</typeparam>
    /// <typeparam name="FileOperateData">文件操作数据类型</typeparam>
    public class MemoryMappedFileOperate : FileOperate
    {
        private const int AllocationGranularity = 64 * 1024; // 保守对齐（Windows 常见）

        private MemoryMappedFile mmFile;
        private MemoryMappedViewAccessor mmViewAccessor;

        public MemoryMappedFileOperate(LocalFileInfo info) : base(info)
        {
        }

        public MemoryMappedFileOperate(FileOperateInfo operateInfo) : base(operateInfo)
        {
        }

        //private void CreateMemoryMapped(long capacity = 0L)
        //{
        //    DisposeMapping();
        //    if (capacity < Capacity)
        //        return;

        //    FileStream stream = OperateInfo.OpenFile();
        //    if (capacity == 0)
        //    {
        //        capacity = stream.DefaultLength == 0L ? Utility.KilobytesToBytes(4) : stream.DefaultLength;
        //    }

        //    if (AllocationStrategy != AllocationStrategy.None)
        //    {
        //        SwitchAllocationStrategy(stream, capacity);
        //    }

        //    if (stream.DefaultLength < capacity)
        //    {
        //        stream.SetLength(capacity);
        //    }

        //    Capacity = capacity;
        //    mmFile = MemoryMappedFile.CreateFromFile(stream, Info.FileName, Capacity, OperateInfo, HandleInheritability.None, false);
        //    mmViewAccessor = mmFile.CreateViewAccessor();
        //}

        #region Check

        private void CheckBytes([NotNull] byte[] bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
        }

        private void CheckMemoryMappedCanWrite()
        {
            if (!mmViewAccessor.CanWrite)
                throw new Exception(string.Format("当前文件无法写入：{0}", Info.FullPath));
        }

        private void CheckMemoryMappedCanRead()
        {
            if (!mmViewAccessor.CanRead)
                throw new Exception(string.Format("当前文件无法读取：{0}", Info.FullPath));
        }

        #endregion Check

        #region MemoryMappedOperate

        private void WriteToMemoryMapped(long filePosition, byte[] bytes, int offset, int count)
        {
            CheckMemoryMappedCanWrite();
            if (count <= 0) return;

            mmViewAccessor.WriteArray(filePosition, bytes, offset, count);
        }

        #endregion MemoryMappedOperate

        private void DisposeMapping()
        {
            if (mmViewAccessor != null)
            {
                mmViewAccessor.Dispose();
                mmViewAccessor = null!;
            }
            if (mmFile != null)
            {
                mmFile.Dispose();
                mmFile = null!;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DisposeMapping();
        }

        protected override void ExecuteWrite(long filePosition, byte[] bytes, int bytesPosition, int bytesLength)
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteWrite(long filePosition, ref ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteWrite(long filePosition, ReadOnlySpan<byte> span)
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteWrite(long filePosition, ReadOnlyMemory<byte> memory)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask ExecuteWriteAsync(long filePosition, byte[] bytes, int bytesPosition, int bytesLength, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask ExecuteWriteAsync(long filePosition, ReadOnlyMemory<byte> memory, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override byte[] ExecuteRead(long filePosition, int length)
        {
            throw new NotImplementedException();
        }

        protected override int ExecuteRead(long filePosition, byte[] bytes, int bytesStart, int length)
        {
            throw new NotImplementedException();
        }

        protected override int ExecuteRead(long filePosition, Span<byte> span)
        {
            throw new NotImplementedException();
        }

        protected override int ExecuteRead(long filePosition, Memory<byte> memory)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<byte[]> ExecuteReadAsync(long filePosition, int length, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<int> ExecuteReadAsync(long filePosition, byte[] bytes, int bytesStart, int length, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask<int> ExecuteReadAsync(long filePosition, Memory<byte> memory, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override void ChangeCapacity(long length)
        {
            throw new NotImplementedException();
        }

        protected override void ExecuteWrite(long filePosition, ref ByteBlock block)
        {
            throw new NotImplementedException();
        }

        protected override int ExecuteRead(long filePosition, int length, ref ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }

        protected override int ExecuteRead(long filePosition, int length, ref ByteBlock block)
        {
            throw new NotImplementedException();
        }
    }
}