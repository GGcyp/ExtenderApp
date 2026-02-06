using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 基础序列实现，使用段提供者管理多个段并实现 <see cref="IBufferWriter{T}"/>。
    /// 提供在首部/末尾/中间插入段以及按位置查找段的功能，并支持基于 AutomaticCompactMemory 的紧凑操作。
    /// </summary>
    /// <typeparam name="T">序列中元素的类型。</typeparam>
    public class SequenceBase<T> : DisposableObject, IBufferWriter<T>
    {
        /// <summary>
        /// 自动增长的最大大小。
        /// </summary>
        private const int MaximumAutoGrowSize = 32 * 1024;

        private static readonly ObjectPool<SequenceSegment> _pool = ObjectPool.Create<SequenceSegment>();
        private static readonly ReadOnlySequence<T> Empty = new ReadOnlySequence<T>(SequenceSegment.Empty, 0, SequenceSegment.Empty, 0);

        private static SequenceSegment GetSegment(MemoryBlock<T> block)
        {
            var segment = _pool.Get();
            segment.SetBlock(block);
            return segment;
        }

        private static void ReleaseSegment(SequenceSegment? segment)
        {
            while (segment != null)
            {
                var next = segment.Next;
                segment.Reset();
                _pool.Release(segment);
                segment = next;
            }
        }

        private readonly MemoryBlockProviderBase<T> _blockProvider;

        /// <summary>
        /// 获取或设置序列的最小跨度长度。
        /// </summary>
        private int minimumSpanLength;

        /// <summary>
        /// 获取或设置是否自动增加最小跨度长度。
        /// </summary>
        public bool AutoIncreaseMinimumSpanLength { get; set; }

        /// <summary>
        /// 是否在可能时自动压缩/回收空闲内存（由具体实现解释）。
        /// </summary>
        public bool AutomaticCompactMemory { get; set; }

        /// <summary>
        /// 获取此序列的只读版本。
        /// </summary>
        public ReadOnlySequence<T> AsReadOnlySequence => this;

        /// <summary>
        /// 获取序列的长度（元素数量）。
        /// </summary>
        public long Length => AsReadOnlySequence.Length;

        private SequenceSegment? first;

        private SequenceSegment? last;

        public SequenceBase(MemoryBlockProviderBase<T> blockProvider)
        {
            _blockProvider = blockProvider;
        }

        /// <summary>
        /// 获取具有至少 <paramref name="sizeHint"/> 可写空间的内存段。
        /// </summary>
        /// <param name="sizeHint">所需的最小可写元素数，0 表示不限。</param>
        /// <returns>可写内存。</returns>
        public Memory<T> GetMemory(int sizeHint = 0) => GetSegment(sizeHint).Block!.RemainingMemory;

        /// <summary>
        /// 获取具有至少 <paramref name="sizeHint"/> 可写空间的跨度。
        /// </summary>
        /// <param name="sizeHint">所需的最小可写元素数，0 表示不限。</param>
        /// <returns>可写跨度。</returns>
        public Span<T> GetSpan(int sizeHint = 0) => GetSegment(sizeHint).Block!.RemainingSpan;

        /// <summary>
        /// 将当前写入位置推进 <paramref name="count"/> 个元素。
        /// </summary>
        /// <param name="count">推进的元素数量。</param>
        public void Advance(int count)
        {
            if (last is null)
                throw new InvalidOperationException("在获取内存之前不能进行推进操作");

            last.Advance(count);
            ConsiderMinimumSizeIncrease();
        }

        private SequenceSegment GetSegment(int sizeHint)
        {
            int minBufferSize = sizeHint;
            SequenceSegment? segment = last;
            if (sizeHint == 0 && last != null && last.WritableBytes != 0)
            {
                return last;
            }

            if (last?.WritableBytes < sizeHint)
            {
                minBufferSize = System.Math.Max(minimumSpanLength, sizeHint);
            }

            var sequenceSegment = _pool.Get();
            GetSegment(CreateBlock(minBufferSize));
            return sequenceSegment;
        }

        private MemoryBlock<T> CreateBlock(int sizeHint)
        {
            // 使用 provider 获取新的段实例（实现可以从池中租用或新建）。
            var block = _blockProvider.GetBlock(sizeHint);
            Append(block);
            return block;
        }

        /// <summary>
        /// 将段追加到序列末尾（常规操作）。
        /// </summary>
        /// <param name="segment">要追加的段，不能为空。</param>
        public void Append(MemoryBlock<T> block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            Append(GetSegment(block));
        }

        /// <summary>
        /// 将段插入到序列首部。
        /// </summary>
        /// <param name="segment">要插入的段，不能为空。</param>
        public void Prepend(MemoryBlock<T> block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            var segment = GetSegment(block);
            Prepend(segment);
        }

        private void Append(SequenceSegment segment)
        {
            if (last == null)
            {
                first = segment;
                last = segment;
                return;
            }

            // 若 last 已被消费过一部分，直接在其后追加新段
            if (last.Consumed > 0)
            {
                last.SetNext(segment);
                last = segment;
                return;
            }

            // last 是空（或未消费）且需要替换为新段 —— 与以前实现行为一致：
            var current = first;
            if (first != last)
            {
                while (current!.Next != last)
                {
                    current = current.Next;
                }
            }
            else
            {
                first = segment;
            }

            current!.SetNext(segment);
            ReleaseSegment(last);
            last = segment;
        }

        private void Prepend(SequenceSegment segment)
        {
            if (first == null)
            {
                first = segment;
                last = segment;
                return;
            }

            // 将新段作为头部，并连接原先的头部为下一个
            segment.SetNext(first);
            first = segment;
        }

        public void UpdateConsumedForPostion(Span<T> span, long position)
        {
            //if (span.IsEmpty)
            //    return;

            //// 计算在段内的相对偏移（相对于 start）
            //if (position < SegmentStart)
            //    throw new ArgumentOutOfRangeException(nameof(position), "position 必须不小于段起始索引。");

            //long relative = position - SegmentStart;
            //if (relative < 0 || relative + span.Length > Consumed)
            //    throw new ArgumentOutOfRangeException(nameof(span), "要更新的跨度超出当前段的已用范围。");

            //int consumedPosition = (int)relative;
            //span.CopyTo(Span.Slice(consumedPosition));
        }

        /// <summary>
        /// 在指定的现有段之后插入一个新段。
        /// </summary>
        /// <param name="existing">现有段，不能为空且必须属于当前序列。</param>
        /// <param name="segment">要插入的新段，不能为空。</param>
        private void InsertAfter(SequenceSegment existing, SequenceSegment segment)
        {
            segment.SetNext(existing.Next);
            existing.SetNext(segment);

            if (existing == last)
                last = segment;
        }

        /// <summary>
        /// 在指定的现有段之前插入一个新段。
        /// </summary>
        /// <param name="existing">现有段，不能为空且必须属于当前序列。</param>
        /// <param name="segment">要插入的新段，不能为空。</param>
        private void InsertBefore(SequenceSegment existing, SequenceSegment segment)
        {
            if (existing == first)
            {
                Prepend(segment);
                return;
            }

            // 查找 existing 的前驱并在其后插入
            var current = first;
            while (current != null && current.Next != existing)
            {
                current = current.Next;
            }

            if (current == null)
                throw new InvalidOperationException("指定的 existing 段不属于当前序列。");

            segment.SetNext(existing);
            current.SetNext(segment);
        }

        /// <summary>
        /// 在序列中根据绝对位置（从 0 开始）插入段，插入点位于包含该位置的段之前（若 position 指向段首，则插入在该段之前）。
        /// </summary>
        /// <param name="position">绝对位置（0..Length）。</param>
        /// <param name="segment">要插入的新段，不能为空。</param>
        public void InsertAtPosition(long position, MemoryBlock<T> block)
        {
            ArgumentNullException.ThrowIfNull(block, nameof(block));
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            var segment = GetSegment(block);

            if (first == null)
            {
                // 空序列等价于 Append
                Append(segment);
                return;
            }

            if (position == 0)
            {
                Prepend(segment);
                return;
            }

            if (position == Length)
            {
                Append(segment);
                return;
            }

            if (!TryGetSegmentByPosition(position, out SequenceSegment target, out var offset))
                throw new InvalidOperationException("无法定位到指定位置的段。");

            // 若 position 指向 target 段的首部（offset == 0），插在 target 之前
            if (offset == 0)
            {
                InsertBefore(target, segment);
            }
            else
            {
                // 否则插在 target 之后（留给消费/调用者决定如何拼接）
                InsertAfter(target, segment);
            }
        }

        /// <summary>
        /// 尝试根据序列中的绝对位置定位到包含该位置的段并返回段内偏移（相对于段的 Memory 的起点）。
        /// </summary>
        /// <param name="position">绝对位置（从 0 开始）。</param>
        /// <param name="segment">找到的段（若返回 false 则为 null）。</param>
        /// <param name="offset">在段内的偏移（相对于段的 Memory 的起点）。</param>
        /// <returns>若找到则返回 true，否则返回 false。</returns>
        public bool TryGetSegmentByPosition(long position, out MemoryBlock<T> segment, out int offset)
        {
            segment = MemoryBlock<T>.Empty;
            if (TryGetSegmentByPosition(position, out SequenceSegment seg, out offset))
            {
                segment = seg.Block!;
                return true;
            }
            return false;
        }

        private bool TryGetSegmentByPosition(long position, out SequenceSegment segment, out int offset)
        {
            segment = default!;
            offset = 0;

            if (position < 0 || position >= Length)
                return false;

            var current = first;
            while (current != null)
            {
                long segStart = current.RunningIndex;
                int segLen = current.Consumed;
                if (position >= segStart && position < segStart + segLen)
                {
                    segment = current;
                    offset = (int)(position - segStart);
                    return true;
                }

                current = current.Next;
            }

            return false;
        }

        private void ConsiderMinimumSizeIncrease()
        {
            if (AutoIncreaseMinimumSpanLength && minimumSpanLength < MaximumAutoGrowSize)
            {
                int autoSize = System.Math.Min(MaximumAutoGrowSize, (int)System.Math.Min(int.MaxValue, Length / 2));
                if (minimumSpanLength < autoSize)
                {
                    minimumSpanLength = autoSize;
                }
            }
        }

        /// <summary>
        /// 将 SequenceBase 隐式转换为 <see cref="ReadOnlySequence{T}"/>。
        /// </summary>
        /// <param name="sequence">源序列。</param>
        public static implicit operator ReadOnlySequence<T>(SequenceBase<T> sequence)
        {
            return sequence.first is SequenceSegment first && sequence.last is SequenceSegment last
                ? new ReadOnlySequence<T>(first, 0, last, last.Consumed)
                : Empty;
        }

        private class SequenceSegment : ReadOnlySequenceSegment<T>
        {
            internal static readonly SequenceSegment Empty = new();
            private MemoryBlock<T>? block;

            internal MemoryBlock<T>? Block
            {
                get
                {
                    if (block != null && block.IsDisposed)
                    {
                        BlockReleased();
                    }
                    return block;
                }
                private set => block = value;
            }

            internal new Memory<T> Memory => Block?.Memory ?? Memory<T>.Empty;

            internal int Consumed => Block?.Consumed ?? 0;

            /// <summary>
            /// 序列中当前段的起始索引（相对于整个序列）。
            /// </summary>
            internal long SegmentStart { get; set; }

            /// <summary>
            /// 序列中当前段的结束索引（相对于整个序列）。
            /// </summary>
            internal long SegmentEnd => SegmentStart + Consumed;

            public int WritableBytes => Block?.WritableBytes ?? 0;

            internal SequenceSegment? Prev { get; private set; }

            internal new SequenceSegment? Next
            {
                get => base.Next as SequenceSegment;
                set => base.Next = value;
            }

            /// <summary>
            /// 将 <paramref name="segment"/> 设置为当前段的下一个段，并为其设置正确的 <see cref="Prev"/> 链接。
            /// 注意：同时会更新下一个段及其后续所有段的 <see cref="RunningIndex"/> 属性。
            /// </summary>
            /// <param name="segment">要设置为下一个段的实例（不能为空）。</param>
            /// <param name="owner">当前段所属的序列实例。</param>
            internal void SetNext(SequenceSegment? segment)
            {
                Next = segment;
                if (segment != null)
                {
                    segment.Prev = this;
                    segment.UpdateRunningIndex();
                }
            }

            internal void SetBlock(MemoryBlock<T> block)
            {
                Block = block;
                Block.ConsumedChanged += UpdateRunningIndex;
                Block.Released += BlockReleased;
            }

            /// <summary>
            /// 更新自身及后续所有段的 RunningIndex 属性，以反映当前链中各段的正确位置。
            /// </summary>
            internal void UpdateRunningIndex()
            {
                RunningIndex = Prev != null ? Prev.RunningIndex + Prev.Consumed : 0;
                Next?.UpdateRunningIndex();
            }

            private void BlockReleased()
            {
            }

            internal void Advance(int count) => Block?.Advance(count);

            internal void Reset()
            {
                if (Block != null)
                {
                    Block.ConsumedChanged -= UpdateRunningIndex;
                    Block.Released -= BlockReleased;
                }
                Block = null;
                Prev = null;
                Next = null;
                RunningIndex = 0;
            }
        }
    }
}