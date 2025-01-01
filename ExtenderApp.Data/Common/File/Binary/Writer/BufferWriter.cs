using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace ExtenderApp.Data.File
{
    /// <summary>
    /// 内部引用结构体BufferWriter，用于高效地向IBufferWriter<byte>输出数据。
    /// </summary>
    internal ref struct BufferWriter
    {
        /// <summary>
        /// 获取或设置输出缓冲区。
        /// </summary>
        public IBufferWriter<byte> Output { get; private set; }

        /// <summary>
        /// 当前的数组段。
        /// </summary>
        private ArraySegment<byte> _segment;

        /// <summary>
        /// 已缓冲的字节数。
        /// </summary>
        private int _buffered;

        /// <summary>
        /// 获取当前的Span。
        /// </summary>
        public Span<byte> Span { get; private set; }

        /// <summary>
        /// 已提交的字节数。
        /// </summary>
        public long BytesCommitted { get; private set; }

        /// <summary>
        /// 序列池。
        /// </summary>
        internal SequencePool SequencePool;

        /// <summary>
        /// 序列池的租赁。
        /// </summary>
        internal SequencePool.Rental Rental;

        /// <summary>
        /// 使用指定的IBufferWriter<byte>初始化BufferWriter实例。
        /// </summary>
        /// <param name="output">IBufferWriter<byte>输出缓冲区。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BufferWriter(IBufferWriter<byte> output)
        {
            _buffered = 0;
            BytesCommitted = 0;
            Output = output ?? throw new ArgumentNullException(nameof(output));

            var memory = Output.GetMemory();
            MemoryMarshal.TryGetArray(memory, out _segment);
            Span = memory.Span;
        }

        /// <summary>
        /// 使用指定的序列池和字节数组初始化BufferWriter实例。
        /// </summary>
        /// <param name="sequencePool">序列池。</param>
        /// <param name="array">字节数组。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal BufferWriter(SequencePool sequencePool, byte[] array)
        {
            _buffered = 0;
            BytesCommitted = 0;
            if (sequencePool is null) throw new ArgumentNullException(nameof(sequencePool));
            this.SequencePool = sequencePool;
            Rental = default;
            Output = null;

            _segment = new ArraySegment<byte>(array);
            Span = _segment.AsSpan();
        }

        /// <summary>
        /// 获取指定大小的Span。
        /// </summary>
        /// <param name="sizeHint">期望的大小。</param>
        /// <returns>Span<byte>。</returns>
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            Ensure(sizeHint);
            return Span;
        }

        /// <summary>
        /// 获取指定大小的字节引用。
        /// </summary>
        /// <param name="sizeHint">期望的大小。</param>
        /// <returns>ref byte。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref byte GetPointer(int sizeHint = 0)
        {
            Ensure(sizeHint);

            if (_segment.Array != null)
            {
                return ref _segment.Array[_segment.Offset + _buffered];
            }
            else
            {
                return ref Span.GetPinnableReference();
            }
        }

        /// <summary>
        /// 提交已缓冲的数据。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Commit()
        {
            var buffered = _buffered;
            if (buffered > 0)
            {
                MigrateToSequence();

                BytesCommitted += buffered;
                _buffered = 0;
                Output.Advance(buffered);
                Span = default;
            }
        }

        /// <summary>
        /// 前进指定的字节数。
        /// </summary>
        /// <param name="count">前进的字节数。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            _buffered += count;
            Span = Span.Slice(count);
        }

        /// <summary>
        /// 将ReadOnlySpan<byte>写入缓冲区。
        /// </summary>
        /// <param name="source">要写入的数据。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> source)
        {
            if (Span.Length >= source.Length)
            {
                source.CopyTo(Span);
                Advance(source.Length);
            }
            else
            {
                WriteMultiBuffer(source);
            }
        }

        /// <summary>
        /// 确保有足够的空间写入指定大小的数据。
        /// </summary>
        /// <param name="count">期望的空间大小。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ensure(int count = 0)
        {
            if (Span.Length < count)
            {
                EnsureMore(count);
            }
        }

        /// <summary>
        /// 确保有更多的空间。
        /// </summary>
        /// <param name="count">期望的空间大小。</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnsureMore(int count = 0)
        {
            if (_buffered > 0)
            {
                Commit();
            }
            else
            {
                MigrateToSequence();
            }

            var memory = Output.GetMemory(count);
            if (memory.IsEmpty)
            {
                throw new InvalidOperationException("基础的 IBufferWriter<byte>.GetMemory(int) 方法返回了一个空内存块，这是不被允许的。" + Output.GetType().FullName);
            }
            MemoryMarshal.TryGetArray(memory, out _segment);
            Span = memory.Span;
        }

        /// <summary>
        /// 尝试获取未提交的Span。
        /// </summary>
        /// <param name="span">未提交的Span。</param>
        /// <returns>是否成功获取。</returns>
        internal bool TryGetUncommittedSpan(out ReadOnlySpan<byte> span)
        {
            if (SequencePool != null)
            {
                span = _segment.AsSpan(0, _buffered);
                return true;
            }

            span = default;
            return false;
        }

        /// <summary>
        /// 将ReadOnlySpan<byte>写入多缓冲区。
        /// </summary>
        /// <param name="source">要写入的数据。</param>
        private void WriteMultiBuffer(ReadOnlySpan<byte> source)
        {
            int copiedBytes = 0;
            int bytesLeftToCopy = source.Length;
            while (bytesLeftToCopy > 0)
            {
                if (Span.Length == 0)
                {
                    EnsureMore();
                }

                var writable = System.Math.Min(bytesLeftToCopy, Span.Length);
                source.Slice(copiedBytes, writable).CopyTo(Span);
                copiedBytes += writable;
                bytesLeftToCopy -= writable;
                Advance(writable);
            }
        }

        /// <summary>
        /// 迁移到序列。
        /// </summary>
        private void MigrateToSequence()
        {
            if (SequencePool != null)
            {
                Rental = SequencePool.Rent();
                Output = Rental.Value;
                var realSpan = Output.GetSpan(_buffered);
                _segment.AsSpan(0, _buffered).CopyTo(realSpan);
                SequencePool = null;
            }
        }
    }
}
