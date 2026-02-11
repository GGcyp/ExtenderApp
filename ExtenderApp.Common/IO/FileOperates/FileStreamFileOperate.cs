using System;
using System.Buffers;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

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

        #region Read

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


        protected override async ValueTask<byte[]> ExecuteReadAsync(long filePosition, int length, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                byte[] bytes = new byte[length];
                await RandomAccess.ReadAsync(Stream.SafeFileHandle, bytes, filePosition, token);
                return bytes;
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override async ValueTask<long> ExecuteReadAsync(long filePosition, long length, AbstractBuffer<byte> buffer, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                long remaining = length;
                long totalRead = 0;
                while (remaining > 0)
                {
                    int readLength = (int)Math.Min(remaining, buffer.Available);
                    if (readLength == 0)
                    {
                        break;
                    }
                    int bytesRead = await RandomAccess.ReadAsync(Stream.SafeFileHandle, buffer.GetMemory(readLength), filePosition, token);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    buffer.Advance(bytesRead);
                    filePosition += bytesRead;
                    remaining -= bytesRead;
                    totalRead += bytesRead;
                }
                return totalRead;
            }
            finally
            {
                _slim.Release();
            }
        }

        #endregion

        #region Write

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

        protected override long ExecuteWrite(long filePosition, AbstractBuffer<byte> buffer)
        {
            _slim.Wait();
            try
            {
                ReadOnlySequence<byte> sequence = buffer.CommittedSequence;
                SequencePosition position = sequence.Start;
                while (sequence.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                {
                    RandomAccess.Write(Stream.SafeFileHandle, memory.Span, filePosition);
                    filePosition += memory.Length;
                }
                return buffer.Committed;
            }
            finally
            {
                _slim.Release();
            }
        }

        protected override async ValueTask<long> ExecuteWriteAsync(long filePosition, AbstractBuffer<byte> buffer, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                ReadOnlySequence<byte> sequence = buffer.CommittedSequence;
                SequencePosition position = sequence.Start;
                while (sequence.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                {
                    await RandomAccess.WriteAsync(Stream.SafeFileHandle, memory, filePosition);
                    filePosition += memory.Length;
                }
                return buffer.Committed;
            }
            finally
            {
                _slim.Release();
            }
        }

        #endregion
    }
}