using System.Buffers;
using ExtenderApp.Contracts;

namespace ExtenderApp.Common.IO
{
    /// <summary>
    /// 文件流文件操作类，使用FileStream进行文件操作，适用于需要频繁读写文件的场景。
    /// </summary>
    public sealed class FileStreamFileOperate : FileOperate
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

        ///<inheritdoc/>
        protected override sealed void ChangeCapacity(long length)
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

        ///<inheritdoc/>
        protected override sealed byte[] ExecuteRead(long filePosition, int length)
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

        ///<inheritdoc/>
        protected override sealed int ExecuteRead(long filePosition, Span<byte> span)
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

        ///<inheritdoc/>
        protected override sealed async ValueTask<byte[]> ExecuteReadAsync(long filePosition, int length, CancellationToken token)
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

        ///<inheritdoc/>
        protected override sealed async ValueTask<long> ExecuteReadAsync(long filePosition, Memory<byte> memory, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                return await RandomAccess.ReadAsync(Stream.SafeFileHandle, memory, filePosition, token);
            }
            finally
            {
                _slim.Release();
            }
        }

        #endregion Read

        #region Write

        ///<inheritdoc/>
        protected override sealed long ExecuteWrite(long filePosition, ReadOnlySpan<byte> span)
        {
            _slim.Wait();
            try
            {
                RandomAccess.Write(Stream.SafeFileHandle, span, filePosition);
                return span.Length;
            }
            finally
            {
                _slim.Release();
            }
        }

        ///<inheritdoc/>
        protected override sealed long ExecuteWrite(long filePosition, ReadOnlySequence<byte> sequence)
        {
            _slim.Wait();
            try
            {
                SequencePosition position = sequence.Start;
                while (sequence.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                {
                    RandomAccess.Write(Stream.SafeFileHandle, memory.Span, filePosition);
                    filePosition += memory.Length;
                }
                return sequence.Length;
            }
            finally
            {
                _slim.Release();
            }
        }

        ///<inheritdoc/>
        protected override sealed async ValueTask<long> ExecuteWriteAsync(long filePosition, ReadOnlySequence<byte> sequence, CancellationToken token)
        {
            _slim.Wait();
            try
            {
                SequencePosition position = sequence.Start;
                while (sequence.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                {
                    await RandomAccess.WriteAsync(Stream.SafeFileHandle, memory, filePosition);
                    filePosition += memory.Length;
                }
                return sequence.Length;
            }
            finally
            {
                _slim.Release();
            }
        }

        #endregion Write
    }
}