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
        private bool leaveInnerLinkerOpen;
        private ITcpLinker? _linker;
        private bool disposed;

        /// <summary>
        /// 指示该流是否支持读取操作。此实现始终返回 <c>true</c>，因为底层 <see cref="ITcpLinker"/> 支持接收数据。
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// 指示该流是否支持寻址。此实现不支持寻址，始终返回 <c>false</c>。
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// 指示该流是否支持写入操作。此实现始终返回 <c>true</c>，因为底层 <see cref="ITcpLinker"/> 支持发送数据。
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// 获取流的长度。此流不支持获取长度，因此会抛出 <see cref="NotSupportedException"/>。
        /// </summary>
        /// <exception cref="NotSupportedException">始终抛出。</exception>
        public override long Length => throw new NotSupportedException();

        /// <summary>
        /// 获取或设置流中的当前位置。此操作不受支持并将抛出 <see cref="NotSupportedException"/>。
        /// </summary>
        /// <exception cref="NotSupportedException">始终抛出。</exception>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public TcpLinkerStream() : this(null, true)
        {
        }

        public TcpLinkerStream(ITcpLinker? linker) : this(linker, true)
        {
        }

        public TcpLinkerStream(ITcpLinker? linker, bool leaveInnerLinkerOpen)
        {
            _linker = linker;
            this.leaveInnerLinkerOpen = leaveInnerLinkerOpen;
        }

        /// <summary>
        /// 写入内部的 <see cref="ITcpLinker"/> 实例。此方法允许在流创建后动态设置或替换底层链接器。
        /// </summary>
        /// <param name="linker">被写入的 <see cref="ITcpLinker"/> 实例。</param>
        /// <param name="leaveInnerLinkerOpen">指示在流释放时是否保持底层链接器打开。默认为 <c>true</c>，表示流释放时不关闭链接器。</param>
        /// <exception cref="ObjectDisposedException">当流已被释放时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 <paramref name="linker"/> 为 <c>null</c> 时抛出。</exception>
        public void SetInnerTcpLinker(ITcpLinker linker, bool leaveInnerLinkerOpen = true)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(TcpLinkerStream));
            _linker = linker ?? throw new ArgumentNullException(nameof(linker));
            this.leaveInnerLinkerOpen = leaveInnerLinkerOpen;
        }

        /// <summary>
        /// 确保底层 <see cref="ITcpLinker"/> 可用。
        /// </summary>
        /// <returns>有效的 <see cref="ITcpLinker"/> 实例。</returns>
        /// <exception cref="ObjectDisposedException">当流已被释放时抛出。</exception>
        /// <exception cref="InvalidOperationException">当底层链接器未设置时抛出。</exception>
        private ITcpLinker EnsureLinker()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(TcpLinkerStream));
            return _linker ?? throw new InvalidOperationException("内部流尚未设置。");
        }

        /// <summary>
        /// 刷新流。此实现为空操作，因为写入通过底层 <see cref="ITcpLinker"/> 立即发送。
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// 从流中同步读取数据并写入到指定缓冲区。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="offset">写入缓冲区的起始偏移。</param>
        /// <param name="count">最多读取的字节数。</param>
        /// <returns>实际读取的字节数；0 表示远端已关闭连接或到达流末尾。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为 <c>null</c> 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="offset"/> 或 <paramref name="count"/> 越界时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当流已释放时抛出。</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            return EnsureLinker().Receive(buffer.AsSpan(offset, count)).Value.BytesTransferred;
        }

        /// <summary>
        /// 从流中同步读取数据并写入到指定的 <see cref="Span{Byte}"/> 缓冲区。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <returns>实际读取的字节数。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为空时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当流已释放时抛出。</exception>
        public override int Read(Span<byte> buffer)
        {
            if (buffer.IsEmpty) throw new ArgumentNullException(nameof(buffer));

            return EnsureLinker().Receive(buffer).Value.BytesTransferred;
        }

        /// <summary>
        /// 异步从底层 <see cref="ITcpLinker"/> 读取数据并拷贝到 <paramref name="buffer"/>。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="offset">写入目标缓冲区的起始偏移。</param>
        /// <param name="count">最多读取字节数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>任务，完成时返回实际读取的字节数；0 表示已断开或 EOF。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为 <c>null</c> 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="offset"/> 或 <paramref name="count"/> 越界时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当流已释放时抛出。</exception>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (disposed) throw new ObjectDisposedException(nameof(TcpLinkerStream));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            var result = await EnsureLinker().ReceiveAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
            return result.Value.BytesTransferred;
        }

        /// <summary>
        /// 异步从底层 <see cref="ITcpLinker"/> 读取数据并写入到 <see cref="Memory{Byte}"/> 缓冲区。
        /// </summary>
        /// <param name="buffer">目标缓冲区。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>任务，完成时返回实际读取的字节数。</returns>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty || buffer.Length == 0)
                return 0;

            var result = await EnsureLinker().ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            return result.Value.BytesTransferred;
        }

        /// <summary>
        /// 将指定缓冲区的数据同步写入到流中（发送到远端）。
        /// </summary>
        /// <param name="buffer">源缓冲区。</param>
        /// <param name="offset">源缓冲区的起始偏移。</param>
        /// <param name="count">写入的字节数。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为 <c>null</c> 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="offset"/> 或 <paramref name="count"/> 越界时抛出。</exception>
        /// <exception cref="ObjectDisposedException">当流已释放时抛出。</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (disposed) throw new ObjectDisposedException(nameof(TcpLinkerStream));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            EnsureLinker().Send(buffer.AsSpan(offset, count));
        }

        /// <summary>
        /// 将指定的 <see cref="ReadOnlySpan{Byte}"/> 内容同步写入到流中（发送）。
        /// </summary>
        /// <param name="buffer">源数据缓冲区。</param>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (buffer.IsEmpty || buffer.Length == 0)
                return;

            EnsureLinker().Send(buffer);
        }

        /// <summary>
        /// 异步将数据写入到流中并发送到远端。
        /// </summary>
        /// <param name="buffer">源缓冲区。</param>
        /// <param name="offset">缓冲区起始偏移。</param>
        /// <param name="count">写入的字节数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示写入操作的任务。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="buffer"/> 为 <c>null</c> 时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="offset"/> 或 <paramref name="count"/> 越界时抛出。</exception>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            await EnsureLinker().SendAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 异步将 <see cref="ReadOnlyMemory{Byte}"/> 写入到流并发送。
        /// </summary>
        /// <param name="buffer">要发送的数据。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty || buffer.Length == 0)
                return;

            await EnsureLinker().SendAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 不支持在此流上执行 <see cref="Seek(long,SeekOrigin)"/> 操作。
        /// </summary>
        /// <exception cref="NotSupportedException">始终抛出。</exception>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <summary>
        /// 不支持在此流上设置长度。
        /// </summary>
        /// <exception cref="NotSupportedException">始终抛出。</exception>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// 释放流并标记为已释放。此实现不会关闭或释放底层 <see cref="ITcpLinker"/>，其生命周期由调用方管理。
        /// </summary>
        /// <param name="disposing">指示是否来自托管代码的释放调用。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !leaveInnerLinkerOpen)
            {
                _linker?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }
    }
}