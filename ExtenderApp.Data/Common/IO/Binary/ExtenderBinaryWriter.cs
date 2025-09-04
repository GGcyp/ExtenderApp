using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtenderApp.Data;


namespace ExtenderApp.Data
{
    /// <summary>
    /// 二进制写入
    /// </summary>
    public ref struct ExtenderBinaryWriter
    {
        /// <summary>
        /// 获取或设置输出缓冲区。
        /// </summary>
        public IBufferWriter<byte>? Output { get; private set; }

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

        ///// <summary>
        ///// 获取只读字节序列。
        ///// </summary>
        ///// <returns>返回只读字节序列。</returns>
        //public ReadOnlySequence<byte> AsReadOnlySequence => Rental.Value.AsReadOnlySequence;

        /// <summary>
        /// 序列池。
        /// </summary>
        internal SequencePool<byte>? SequencePool;

        /// <summary>
        /// 序列池的租赁。
        /// </summary>
        public SequencePool<byte>.Rental Rental;

        public bool IsEmpty => Output == null && _segment == null;

        /// <summary>
        /// 使用指定的 <see cref="SequencePool{byte}.Rental"/> 对象初始化 <see cref="ExtenderBinaryWriter"/> 类的新实例。
        /// </summary>
        /// <param name="rental">要使用的 <see cref="SequencePool{byte}.Rental"/> 对象。</param>
        /// <remarks>
        /// 该构造函数会调用带有 <see cref="byte"/> 数组参数的构造函数，并将 <see cref="Rental"/> 属性设置为提供的 <paramref name="rental"/> 对象。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExtenderBinaryWriter(SequencePool<byte>.Rental rental) : this(rental.Value)
        {
            Rental = rental;
        }

        /// <summary>
        /// 使用指定的IBufferWriter<byte>初始化BufferWriter实例。
        /// </summary>
        /// <param name="output">IBufferWriter<byte>输出缓冲区。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExtenderBinaryWriter(IBufferWriter<byte> output)
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
        public ExtenderBinaryWriter(SequencePool<byte> sequencePool, byte[] array)
        {
            _buffered = 0;
            BytesCommitted = 0;

            SequencePool = sequencePool ?? throw new ArgumentNullException(nameof(sequencePool));
            Rental = default;
            Output = default;

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
            if (_buffered > 0)
            {
                MigrateToSequence();

                BytesCommitted += _buffered;
                Output.Advance(_buffered);
                _buffered = 0;
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
        /// 将 <see cref="ReadOnlySequence{byte}"/> 类型的数据写入到当前实例中。
        /// </summary>
        /// <param name="rawMessagePackBlock">要写入的数据。</param>
        /// <remarks>
        /// 使用 <see cref="MethodImplOptions.AggressiveInlining"/> 优化方法内联，以提高性能。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in ReadOnlySequence<byte> rawMessagePackBlock)
        {
            foreach (ReadOnlyMemory<byte> segment in rawMessagePackBlock)
            {
                Write(segment.Span);
            }
        }

        /// <summary>
        /// 将一个字节写入缓冲区。
        /// </summary>
        /// <param name="source">要写入的字节。</param>
        /// <remarks>
        /// 使用 [MethodImpl(MethodImplOptions.AggressiveInlining)] 属性来指示编译器尽可能内联此方法，
        /// 以提高性能。
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte source)
        {
            var span = GetSpan(1);
            span[0] = source;
            Advance(1);
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
        /// 将缓冲区中的数据刷新到数组中，并返回该数组。
        /// </summary>
        /// <returns>包含缓冲区中数据的字节数组。</returns>
        /// <exception cref="NotSupportedException">如果当前实例不支持此操作，则抛出此异常。</exception>
        public byte[] FlushAndGetArray()
        {
            if (TryGetUncommittedSpan(out ReadOnlySpan<byte> span))
            {
                return span.ToArray();
            }
            else
            {
                if (Rental.Value == null)
                {
                    throw new NotSupportedException("此实例未初始化以支持此操作。");
                }

                Commit();
                byte[] result = Rental.Value.AsReadOnlySequence.ToArray();
                Rental.Dispose();
                return result;
            }
        }

        public void CopyTo(byte[] bytes)
        {
            if (Rental.Value == null)
            {
                throw new NotSupportedException("此实例未初始化以支持此操作。");
            }
            Commit();
            if (bytes.Length < BytesCommitted)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "目标数组长度不足以容纳所有数据。");
            }

            foreach (var segment in Rental.Value.AsReadOnlySequence)
            {
                segment.Span.CopyTo(bytes);
                bytes = bytes.AsSpan(segment.Length).ToArray();
            }
        }

        ///// <summary>
        ///// 刷新并清空当前对象
        ///// </summary>
        //public void Flush()
        //{
        //    Commit();
        //    Rental.Dispose();
        //}

        /// <summary>
        /// 将数据写入多缓冲区。
        /// </summary>
        /// <param name="source">数据</param>
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
            else if (Output != null)
            {
                var realSpan = Output.GetSpan(_buffered);
                _segment.AsSpan(0, _buffered).CopyTo(realSpan);
            }
        }

        public void Dispose()
        {
            if (SequencePool != null)
            {
                Rental.Dispose();
            }
            else if (Output != null)
            {
                Output = null;
            }
            _segment = default;
            Span = default;
            _buffered = 0;
            BytesCommitted = 0;
        }
    }
}
