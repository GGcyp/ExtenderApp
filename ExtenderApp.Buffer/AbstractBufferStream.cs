using System.Buffers;

namespace ExtenderApp.Buffer
{
    /// <summary>
    /// 对 AbstractBuffer<byte> 的 Stream 适配器，允许将 AbstractBuffer 用作 Stream 进行读写。
    /// </summary>
    public class AbstractBufferStream : Stream
    {
        private readonly AbstractBuffer<byte> _buffer;
        private long _position;
        private bool _disposed;

        public AbstractBufferStream() : this(AbstractBuffer.GetBlock<byte>())
        {
        }

        public AbstractBufferStream(AbstractBuffer<byte> buffer)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _position = 0;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length
        {
            get
            {
                ThrowIfDisposed();
                return _buffer.Committed;
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfDisposed();
                return _position;
            }
            set => throw new NotSupportedException("AbstractBufferStream does not support setting Position.");
        }

        public override void Flush()
        {
            // underlying buffer is in-memory; nothing to flush
            ThrowIfDisposed();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            long available = _buffer.Committed - _position;
            if (available <= 0) return 0;

            int toRead = (int)Math.Min(available, count);
            var destSpan = new Span<byte>(buffer, offset, toRead);
            CopyFromSequence(_buffer.CommittedSequence, _position, destSpan);
            _position += toRead;
            return toRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek is not supported.");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("SetLength is not supported.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (offset < 0 || count < 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException();

            // append to underlying buffer
            _buffer.Write(new ReadOnlySpan<byte>(buffer, offset, count));
            // after write, position moves to end of committed data
            _position = _buffer.Committed;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Try to release the buffer if possible
                    try
                    {
                        _buffer.UnfreezeWrite();
                        _buffer.TryRelease();
                    }
                    catch
                    {
                        // ignore
                    }
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(AbstractBufferStream));
        }

        private static void CopyFromSequence(ReadOnlySequence<byte> sequence, long position, Span<byte> destination)
        {
            if (destination.Length == 0) return;
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (sequence.IsEmpty)
            {
                destination.Clear();
                return;
            }

            long skip = position;
            int written = 0;
            foreach (var mem in sequence)
            {
                var span = mem.Span;
                if (skip >= span.Length)
                {
                    skip -= span.Length;
                    continue;
                }

                var src = span.Slice((int)skip);
                int take = Math.Min(src.Length, destination.Length - written);
                src.Slice(0, take).CopyTo(destination.Slice(written, take));
                written += take;
                skip = 0;
                if (written == destination.Length) break;
            }

            if (written < destination.Length)
            {
                // zero the rest
                destination.Slice(written).Clear();
            }
        }
    }
}