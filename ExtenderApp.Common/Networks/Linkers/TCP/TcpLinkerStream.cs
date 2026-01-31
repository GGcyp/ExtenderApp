using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 将 <see cref="ITcpLinker"/> 适配为 <see cref="Stream"/>，以便在其上创建 <see cref="System.Net.Security.SslStream"/> 或用于按 <see cref="Stream"/> 接口进行读写。
    /// 
    /// 说明：
    /// - 本实现以可用性为主，适合将异步的 Linker 收发封装为 <see cref="Stream"/>；
    /// - 读操作从底层 Linker 接收数据并复制到调用方缓冲；写操作将调用方缓冲数据一次性复制后通过 Linker.SendAsync 发送；
    /// - 为简洁实现，每次 Write 会分配一个临时数组，必要时可优化为零拷贝或分段发送；
    /// - 注意：不要在同一底层连接上同时混用此 Stream 的读写与 Linker 的裸 ReceivePrivate/SendAsync，否则可能破坏数据边界或 TLS 协议。
    /// </summary>
    public sealed class TcpLinkerStream : Stream
    {
        private readonly ITcpLinker _linker;
        private bool _disposed;

        /// <summary>
        /// 使用指定的 <see cref="ITcpLinker"/> 创建一个新的 <see cref="TcpLinkerStream"/> 实例。
        /// </summary>
        /// <param name="linker">用于网络收发的 TCP 链接器，不得为 null。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="linker"/> 为 null 时抛出。</exception>
        public TcpLinkerStream(ITcpLinker linker)
        {
            _linker = linker ?? throw new ArgumentNullException(nameof(linker));
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <summary>
        /// 此适配流的 Flush 是空操作（无缓冲写入在写方法中立即发送）。
        /// </summary>
        public override void Flush() { /* no-op */ }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();

        /// <summary>
        /// 异步从底层 <see cref="ITcpLinker"/> 读取数据并拷贝到 <paramref name="buffer"/>。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="offset">写入目标缓冲区的起始偏移。</param>
        /// <param name="count">最多读取字节数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>实际读取的字节数；0 表示已断开或 EOF。</returns>
        /// <exception cref="ObjectDisposedException">当流已释放时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 offset/count 越界时抛出。</exception>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TcpLinkerStream));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            var result = await _linker.ReceiveAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
            int read = result.Value.BytesTransferred;
            if (read <= 0) return 0; // EOF 或已断开
            return System.Math.Min(read, count);
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty || buffer.Length == 0) return 0;

            var result = await _linker.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            int read = result.Value.BytesTransferred;
            if (read <= 0) return 0; // EOF 或已断开
            return System.Math.Min(read, buffer.Length);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
            => WriteAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();

        /// <summary>
        /// 异步将指定缓冲区的数据发送到底层 <see cref="ITcpLinker"/>。
        /// </summary>
        /// <param name="buffer">源缓冲区。</param>
        /// <param name="offset">源缓冲区的起始偏移。</param>
        /// <param name="count">要发送的字节数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示异步发送操作的任务。</returns>
        /// <exception cref="ObjectDisposedException">当流已释放时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 offset/count 越界时抛出。</exception>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TcpLinkerStream));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            await _linker.SendAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// 释放流并标记为已释放。不会关闭底层 <see cref="ITcpLinker"/>（由调用者管理其生命周期）。
        /// </summary>
        /// <param name="disposing">指示是否来自托管调用。</param>
        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
