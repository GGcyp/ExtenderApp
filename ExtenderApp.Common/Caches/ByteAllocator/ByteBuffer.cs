using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Caches.ByteAllocator
{
    /// <summary>
    /// 字节缓冲区抽象基类，实现了基础的读写逻辑、位置管理和容量校验。
    /// </summary>
    public abstract class ByteBuffer : DisposableObject, IByteBuffer
    {
        /// <summary>
        /// 获取当前已写入且未被清理的数据末尾位置。
        /// </summary>
        public long WrittenCount { get; protected set; }

        /// <summary>
        /// 获取当前已读取的数据位置。
        /// </summary>
        public long Consumed { get; protected set; }

        /// <summary>
        /// 获取缓冲区中尚未读取的数据长度。
        /// </summary>
        public long Remaining => WrittenCount - Consumed;

        /// <inheritdoc/>
        public long WritableBytes => Capacity > WritableBytes ? Capacity - WrittenCount : 0;

        #region 子类实现成员

        /// <inheritdoc/>
        public abstract long Capacity { get; }

        /// <inheritdoc/>
        public abstract bool CanExpand { get; }

        /// <inheritdoc/>
        public abstract ReadOnlySpan<byte> CommittedSpan { get; }

        /// <inheritdoc/>
        public abstract ReadOnlyMemory<byte> CommittedMemory { get; }

        /// <inheritdoc/>
        public abstract ArraySegment<byte> UnreadSegment { get; }

        /// <inheritdoc/>
        public abstract Memory<byte> GetMemory(int sizeHint = 0);

        /// <inheritdoc/>
        public abstract Span<byte> GetSpan(int sizeHint = 0);

        #endregion 子类实现成员

        #region Write

        /// <summary>
        /// 确保缓冲区具有足够的空间容纳指定大小的数据。
        /// </summary>
        /// <param name="sizeHint">需要的最小空间大小。</param>
        /// <exception cref="InvalidOperationException">当空间不足且无法扩展时抛出。</exception>
        protected virtual void EnsureCapacity(int sizeHint)
        {
            if (WrittenCount + sizeHint <= Capacity) return;

            if (!CanExpand)
                throw new InvalidOperationException("缓冲区空间不足且不支持自动扩展。");

            Expand(sizeHint);
        }

        /// <summary>
        /// 执行缓冲区的扩容逻辑，由具体子类实现。
        /// </summary>
        /// <param name="sizeHint">需要的最小空间大小。</param>
        protected abstract void Expand(int sizeHint);

        /// <inheritdoc/>
        public void Write(byte value)
        {
            ThrowIfDisposed();
            EnsureCapacity(1);
            var span = GetSpan(1);
            span[0] = value;
            Advance(1);
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<byte> source)
        {
            ThrowIfDisposed();
            if (source.IsEmpty) return;

            int length = source.Length;
            EnsureCapacity(length);
            var span = GetSpan(length);
            source.CopyTo(span);
            Advance(length);
        }

        /// <inheritdoc/>
        public void Write(ReadOnlyMemory<byte> source)
        {
            Write(source.Span);
        }

        /// <inheritdoc/>
        public void Write(ArraySegment<byte> segment)
        {
            Write(segment.AsSpan());
        }

        /// <inheritdoc/>
        public void Advance(int count)
        {
            ThrowIfDisposed();
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (WrittenCount + count > Capacity)
                throw new InvalidOperationException("推进写入位置超出了缓冲区总容量。");

            WrittenCount += count;
        }

        #endregion Write

        #region Read

        /// <inheritdoc/>
        public void Read(scoped Span<byte> buffer)
        {
            ThrowIfDisposed();
            if (buffer.IsEmpty) return;
            if (buffer.Length > Remaining)
                throw new InvalidOperationException("请求读取的数据长度超过了当前可用数据量。");

            CommittedSpan.CopyTo(buffer);
            Advance(buffer.Length);
        }

        /// <inheritdoc/>
        public void Read(Memory<byte> buffer)
        {
            Read(buffer.Span);
        }

        /// <inheritdoc/>
        public void Advance(long count)
        {
            ThrowIfDisposed();
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (Consumed + count > WrittenCount)
                throw new InvalidOperationException("推进读取位置不能超过已写入的位置。");

            Consumed += count;
        }

        /// <inheritdoc/>
        public void Rewind(long count)
        {
            ThrowIfDisposed();
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (Consumed - count < 0)
                throw new InvalidOperationException("回退位置不能小于 0。");

            Consumed -= count;
        }

        /// <inheritdoc/>
        public void Seek(long position)
        {
            ThrowIfDisposed();
            if (position < 0 || position > WrittenCount)
                throw new ArgumentOutOfRangeException(nameof(position), "定位位置超出了已写入数据的范围。");

            Consumed = position;
        }

        /// <inheritdoc/>
        public bool TryPeek(out byte value)
        {
            ThrowIfDisposed();
            if (Remaining > 0)
            {
                value = CommittedSpan[0];
                return true;
            }
            value = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryRead(out byte value)
        {
            if (TryPeek(out value))
            {
                Advance(1);
                return true;
            }
            return false;
        }

        #endregion Read

        #region Util

        /// <summary>
        /// 重置缓冲区状态，清空已写入和已读取计数（不清除实际内存数据）。
        /// </summary>
        public virtual void Clear()
        {
            WrittenCount = 0;
            Consumed = 0;
        }

        #endregion Util
    }
}