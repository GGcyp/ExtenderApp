using System.Buffers;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件并发操作类，继承自ConcurrentOperate泛型类
    /// </summary>
    /// <typeparam name="FileOperatePolicy">文件操作策略类型</typeparam>
    /// <typeparam name="FileOperateData">文件操作数据类型</typeparam>
    public class FileStreamFileOperate : FileOperate
    {
        public FileStreamFileOperate(LocalFileInfo info) : base(info)
        {
        }

        public FileStreamFileOperate(FileOperateInfo operateInfo) : base(operateInfo)
        {
        }

        protected override void ChangeCapacity(long length)
        {
            Stream.SetLength(length);
        }

        protected override byte[] ExecuteRead(long filePosition, int length)
        {
            byte[] bytes = new byte[length];
            RandomAccess.Read(Stream.SafeFileHandle, bytes, filePosition);
            return bytes;
        }

        protected override int ExecuteRead(long filePosition, byte[] bytes, int bytesStart, int length)
        {
            return RandomAccess.Read(Stream.SafeFileHandle, bytes.AsSpan(bytesStart, length), filePosition);
        }

        protected override int ExecuteRead(long filePosition, Span<byte> span)
        {
            return RandomAccess.Read(Stream.SafeFileHandle, span, filePosition);
        }

        protected override int ExecuteRead(long filePosition, Memory<byte> memory)
        {
            return RandomAccess.Read(Stream.SafeFileHandle, memory.Span, filePosition);
        }

        protected override ValueTask<byte[]> ExecuteReadAsync(long filePosition, int length, CancellationToken token)
        {
            byte[] bytes = new byte[length];
            RandomAccess.ReadAsync(Stream.SafeFileHandle, bytes, filePosition, token);
            return new ValueTask<byte[]>(bytes);
        }

        protected override ValueTask<int> ExecuteReadAsync(long filePosition, byte[] bytes, int bytesStart, int length, CancellationToken token)
        {
            return RandomAccess.ReadAsync(Stream.SafeFileHandle, bytes, filePosition, token);
        }

        protected override ValueTask<int> ExecuteReadAsync(long filePosition, Memory<byte> memory, CancellationToken token)
        {
            return RandomAccess.ReadAsync(Stream.SafeFileHandle, memory, filePosition, token);
        }

        protected override byte[] ExecuteReadForArrayPool(long filePosition, int length)
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent(length);
            RandomAccess.Read(Stream.SafeFileHandle, bytes.AsSpan(0, length), filePosition);
            return bytes;
        }

        protected override ValueTask<byte[]> ExecuteReadForArrayPoolAsync(long filePosition, int length, CancellationToken token)
        {
            byte[] bytes = ArrayPool<byte>.Shared.Rent(length);
            RandomAccess.ReadAsync(Stream.SafeFileHandle, bytes.AsMemory(0, length), filePosition, token);
            return new ValueTask<byte[]>(bytes);
        }

        protected override void ExecuteWrite(long filePosition, byte[] bytes, int bytesPosition, int bytesLength)
        {
            ExecuteWrite(filePosition, bytes.AsSpan(bytesPosition, bytesLength));
        }

        protected override void ExecuteWrite(long filePosition, ExtenderBinaryReader reader)
        {
            foreach (var item in reader.Sequence)
            {
                RandomAccess.Write(Stream.SafeFileHandle, item.Span, filePosition);
            }
        }

        protected override void ExecuteWrite(long filePosition, ReadOnlySpan<byte> span)
        {
            RandomAccess.Write(Stream.SafeFileHandle, span, filePosition);
        }

        protected override void ExecuteWrite(long filePosition, ReadOnlyMemory<byte> memory)
        {
            RandomAccess.Write(Stream.SafeFileHandle, memory.Span, filePosition);
        }

        protected override ValueTask ExecuteWriteAsync(long filePosition, byte[] bytes, int bytesPosition, int bytesLength, CancellationToken token)
        {
            return RandomAccess.WriteAsync(Stream.SafeFileHandle, bytes.AsMemory(bytesPosition, bytesLength), filePosition, token);
        }

        protected override ValueTask ExecuteWriteAsync(long filePosition, ReadOnlyMemory<byte> memory, CancellationToken token)
        {
            return RandomAccess.WriteAsync(Stream.SafeFileHandle, memory, filePosition, token);
        }
    }
}
