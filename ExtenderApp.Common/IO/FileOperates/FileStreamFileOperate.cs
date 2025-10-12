using System.Buffers;
using ExtenderApp.Data;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件并发操作类，继承自ConcurrentOperate泛型类
    /// </summary>
    /// <typeparam name="FileOperatePolicy">
    /// 文件操作策略类型
    /// </typeparam>
    /// <typeparam name="FileOperateData">
    /// 文件操作数据类型
    /// </typeparam>
    public class FileStreamFileOperate : FileOperate
    {
        /// <summary>
        /// 并发访问控制信号量
        /// </summary>
        private readonly SemaphoreSlim _slim;

        public FileStreamFileOperate(LocalFileInfo info) : base(info)
        {
            _slim = new(1, 1);
        }

        public FileStreamFileOperate(FileOperateInfo operateInfo) : base(operateInfo)
        {
            _slim = new(1, 1);
        }

        protected override void ChangeCapacity(long length)
        {
            _slim.Wait();
            try
            {
                Stream.SetLength(length);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override byte[] ExecuteRead(long filePosition, int length)
        {
            _slim.Wait();
            try
            {
                byte[] bytes = new byte[length];
                RandomAccess.Read(Stream.SafeFileHandle, bytes.AsSpan(0, length), filePosition);
                return bytes;
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override int ExecuteRead(long filePosition, byte[] bytes, int bytesStart, int length)
        {
            _slim.Wait();
            try
            {
                return RandomAccess.Read(Stream.SafeFileHandle, bytes.AsSpan(bytesStart, length), filePosition);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override int ExecuteRead(long filePosition, Span<byte> span)
        {
            _slim.Wait();
            try
            {
                return RandomAccess.Read(Stream.SafeFileHandle, span, filePosition);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override int ExecuteRead(long filePosition, Memory<byte> memory)
        {
            _slim.Wait();
            try
            {
                return RandomAccess.Read(Stream.SafeFileHandle, memory.Span, filePosition);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override ValueTask<byte[]> ExecuteReadAsync(long filePosition, int length, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                byte[] bytes = new byte[length];
                RandomAccess.ReadAsync(Stream.SafeFileHandle, bytes, filePosition, token);
                return new ValueTask<byte[]>(bytes);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override ValueTask<int> ExecuteReadAsync(long filePosition, byte[] bytes, int bytesStart, int length, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                return RandomAccess.ReadAsync(Stream.SafeFileHandle, bytes, filePosition, token);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override ValueTask<int> ExecuteReadAsync(long filePosition, Memory<byte> memory, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                return RandomAccess.ReadAsync(Stream.SafeFileHandle, memory, filePosition, token);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override byte[] ExecuteReadForArrayPool(long filePosition, int length)
        {
            _slim.Wait();
            try
            {
                byte[] bytes = ArrayPool<byte>.Shared.Rent(length);
                RandomAccess.Read(Stream.SafeFileHandle, bytes.AsSpan(0, length), filePosition);
                return bytes;
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override ValueTask<byte[]> ExecuteReadForArrayPoolAsync(long filePosition, int length, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                byte[] bytes = ArrayPool<byte>.Shared.Rent(length);
                RandomAccess.ReadAsync(Stream.SafeFileHandle, bytes.AsMemory(0, length), filePosition, token);
                return new ValueTask<byte[]>(bytes);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override void ExecuteWrite(long filePosition, byte[] bytes, int bytesPosition, int bytesLength)
        {
            _slim.Wait();
            try
            {
                ExecuteWrite(filePosition, bytes.AsSpan(bytesPosition, bytesLength));
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override void ExecuteWrite(long filePosition, ExtenderBinaryReader reader)
        {
            _slim.Wait();
            try
            {
                foreach (var item in reader.Sequence)
                {
                    RandomAccess.Write(Stream.SafeFileHandle, item.Span, filePosition);
                    filePosition += item.Length;
                }
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override void ExecuteWrite(long filePosition, ReadOnlySpan<byte> span)
        {
            _slim.Wait();
            try
            {
                RandomAccess.Write(Stream.SafeFileHandle, span, filePosition);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override void ExecuteWrite(long filePosition, ReadOnlyMemory<byte> memory)
        {
            _slim.Wait();
            try
            {
                RandomAccess.Write(Stream.SafeFileHandle, memory.Span, filePosition);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override ValueTask ExecuteWriteAsync(long filePosition, byte[] bytes, int bytesPosition, int bytesLength, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                return RandomAccess.WriteAsync(Stream.SafeFileHandle, bytes.AsMemory(bytesPosition, bytesLength), filePosition, token);
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override ValueTask ExecuteWriteAsync(long filePosition, ReadOnlyMemory<byte> memory, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                return RandomAccess.WriteAsync(Stream.SafeFileHandle, memory, filePosition, token);
            }
            finally
            {
                _slim.Release();
            }
        }
    }
}