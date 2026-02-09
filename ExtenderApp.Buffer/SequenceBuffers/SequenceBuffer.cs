using System.Buffers;
using ExtenderApp.Buffer.Sequence;

namespace ExtenderApp.Buffer
{
    /// <summary>
    /// 基础序列实现，使用段提供者管理多个段并实现 <see cref="IBufferWriter{T}"/>。 提供在首部/末尾/中间插入段以及按位置查找段的功能，并支持基于 <see cref="AutomaticCompactMemory"/> 的紧凑操作。
    /// </summary>
    /// <typeparam name="T">序列中元素的类型。</typeparam>
    public class SequenceBuffer<T> : AbstractBuffer<T>
    {
        /// <summary>
        /// 自动增长的最大大小。
        /// </summary>
        private const int MaximumAutoGrowSize = 32 * 1024;

        public static readonly SequenceBuffer<T> Empty = new SequenceBuffer<T>(null!);

        private static readonly ReadOnlySequence<T> EmptySequence = new ReadOnlySequence<T>(SequenceBufferSegment<T>.Empty, 0, SequenceBufferSegment<T>.Empty, 0);

        private readonly SequenceBufferSegmentProvider<T> _segmentProvider;

        /// <summary>
        /// 获取或设置序列的最小跨度（已提交）长度，供自动增长策略使用。
        /// </summary>
        private int minimumSpanCommitted;

        internal SequenceBufferProvider<T> OwnerProvider;

        /// <summary>
        /// 获取或设置是否自动增加最小跨度长度。
        /// </summary>
        public bool AutoIncreaseMinimumSpanCommitted { get; set; }

        /// <summary>
        /// 是否在可能时自动压缩/回收空闲内存（由具体实现解释）。
        /// </summary>
        public bool AutomaticCompactMemory { get; set; }

        /// <summary>
        /// 返回表示已提交数据的只读序列。
        /// </summary>
        public override ReadOnlySequence<T> CommittedSequence => this;

        /// <summary>
        /// 当前序列中已提交（已写入）元素的总数。
        /// </summary>
        public override long Committed
        {
            get
            {
                if (First == null)
                    return 0;

                First.UpdateRunningIndex();
                if (Last == null)
                    return First.Committed;

                return Last.RunningIndex + Last.Committed;
            }
        }

        /// <summary>
        /// 序列可见的总容量（已提交 + 最后一段剩余可写空间）。
        /// </summary>
        public override long Capacity
        {
            get
            {
                if (First == null)
                    return 0;

                First.UpdateRunningIndex();
                if (Last == null)
                    return First.Committed + First.Available;

                return Last.RunningIndex + Last.Committed + Last.Available;
            }
        }

        /// <summary>
        /// 当前可直接写入的元素数量（最后一段的剩余可写空间）。
        /// </summary>
        public override int Available
        {
            get
            {
                if (Last == null)
                    return 0;

                return Last.Available;
            }
        }

        internal SequenceBufferSegment<T>? First;

        internal SequenceBufferSegment<T>? Last;

        public SequenceBuffer() : this(SequenceBufferSegmentProvider<T>.Shared)
        {
        }

        /// <summary>
        /// 使用指定的段提供者创建序列实例。
        /// </summary>
        /// <param name="segmentProvider">用于创建或复用序列段的提供者，不能为空。</param>
        public SequenceBuffer(SequenceBufferSegmentProvider<T> segmentProvider)
        {
            OwnerProvider = default!;
            _segmentProvider = segmentProvider;
        }

        /// <summary>
        /// 获取具有至少 <paramref name="sizeHint"/> 可写空间的内存段，返回值从当前写入位置开始。
        /// </summary>
        /// <param name="sizeHint">所需的最小可写元素数，0 表示不限。</param>
        /// <returns>用于写入的 <see cref="Memory{T}"/>。</returns>
        public override Memory<T> GetMemory(int sizeHint = 0) => GetSegment(sizeHint).GetMemory(sizeHint);

        /// <summary>
        /// 获取具有至少 <paramref name="sizeHint"/> 可写空间的跨度，返回值从当前写入位置开始。
        /// </summary>
        /// <param name="sizeHint">所需的最小可写元素数，0 表示不限。</param>
        /// <returns>用于写入的 <see cref="Span{T}"/>。</returns>
        public override Span<T> GetSpan(int sizeHint = 0) => GetSegment(sizeHint).GetSpan(sizeHint);

        /// <summary>
        /// 将当前写入位置向前推进 <paramref name="count"/> 个元素。
        /// </summary>
        /// <param name="count">推进的元素数量（必须为非负值，且不得超过当前段的可写范围）。</param>
        /// <exception cref="InvalidOperationException">若在未获取内存前调用该方法将抛出。</exception>
        public override void Advance(int count)
        {
            if (Last is null)
                throw new InvalidOperationException("在获取内存之前不能进行推进操作");

            Last.Advance(count);
            ConsiderMinimumSizeIncrease();
        }

        private SequenceBufferSegment<T> GetSegment(int sizeHint)
        {
            int minBufferSize = sizeHint;
            SequenceBufferSegment<T>? segment = Last;
            if (sizeHint == 0 && Last != null && Last.Available != 0)
            {
                return Last;
            }

            if (Last?.Available < sizeHint)
            {
                minBufferSize = System.Math.Max(minimumSpanCommitted, sizeHint);
            }

            return CreateSegment(minBufferSize);
        }

        private SequenceBufferSegment<T> CreateSegment(int sizeHint)
        {
            // 使用 provider 获取新的段实例（实现可以从池中租用或新建）。
            var Segment = _segmentProvider.GetSegment(sizeHint);
            Append(Segment);
            return Segment;
        }

        #region Operations for segment management

        /// <summary>
        /// 将指定段追加到序列末尾。若当前最后一段已部分消费，则直接链入；否则替换空段并释放原段链。
        /// </summary>
        /// <param name="segment">要追加的段，不能为空。</param>
        public void Append(SequenceBufferSegment<T> segment)
        {
            if (Last == null)
            {
                First = segment;
                Last = segment;
                return;
            }

            // 若 Last 已被消费过一部分，直接在其后追加新段
            if (Last.Committed > 0)
            {
                Last.SetNext(segment);
                Last = segment;
                return;
            }

            // Last 是空（或未消费）且需要替换为新段 —— 与以前实现行为一致：
            var current = First;
            if (First != Last)
            {
                while (current!.Next != Last)
                {
                    current = current.Next;
                }
            }
            else
            {
                First = segment;
            }

            current!.SetNext(segment);
            Last.Release();
            Last = segment;
        }

        private void Prepend(SequenceBufferSegment<T> segment)
        {
            if (First == null)
            {
                First = segment;
                Last = segment;
                return;
            }

            // 将新段作为头部，并连接原先的头部为下一个
            segment.SetNext(First);
            First = segment;
        }

        /// <summary>
        /// 在指定的现有段之后插入一个新段，并在必要时更新尾部引用。
        /// </summary>
        /// <param name="existing">现有段，不能为空且必须属于当前序列。</param>
        /// <param name="segment">要插入的新段，不能为空。</param>
        private void InsertAfter(SequenceBufferSegment<T> existing, SequenceBufferSegment<T> segment)
        {
            segment.SetNext(existing.Next, false);
            existing.SetNext(segment);

            if (existing == Last)
                Last = segment;
        }

        /// <summary>
        /// 在指定的现有段之前插入一个新段（保证链表完整性）。
        /// </summary>
        /// <param name="existing">现有段，不能为空且必须属于当前序列。</param>
        /// <param name="segment">要插入的新段，不能为空。</param>
        private void InsertBefore(SequenceBufferSegment<T> existing, SequenceBufferSegment<T> segment)
        {
            if (existing.Prev == null)
            {
                Prepend(segment);
                return;
            }
            else
            {
                existing.Prev.SetNext(segment, false);
                segment.SetNext(existing);
            }
        }

        /// <summary>
        /// 在序列中根据绝对位置（从 0 开始）插入段，插入点位于包含该位置的段之前。 若 position 指向段首，则插入在该段之前；position 等于 <see cref="Committed"/> 等价于 Append。
        /// </summary>
        /// <param name="position">绝对位置（0..Committed）。</param>
        /// <param name="segment">要插入的新段，不能为空。</param>
        public void InsertAtPosition(long position, SequenceBufferSegment<T> segment)
        {
            ArgumentNullException.ThrowIfNull(segment, nameof(segment));
            if (position < 0 || position > Committed)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (First == null)
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

            if (position == Committed)
            {
                Append(segment);
                return;
            }

            if (!TryGetSegmentByPosition(position, out SequenceBufferSegment<T> target, out var offset))
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
        /// 根据序列中的绝对位置定位到包含该位置的段并返回段内偏移（相对于段的起点）。
        /// </summary>
        /// <param name="position">绝对位置（从 0 开始）。</param>
        /// <param name="segment">找到的段（若返回 false 则为 default）。</param>
        /// <param name="offset">在段内的偏移（相对于段的 MemoryOwner 的起点）。</param>
        /// <returns>若找到则返回 true，否则返回 false。</returns>
        public bool TryGetSegmentByPosition(long position, out SequenceBufferSegment<T> segment, out int offset)
        {
            segment = default!;
            offset = 0;

            if (position < 0 || position >= Committed)
                return false;

            var current = First;
            while (current != null)
            {
                long segStart = current.RunningIndex;
                long segLen = current.Committed;
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

        protected override void UpdateCommittedProtected(Span<T> span, long committedPosition)
        {
            // 将指定的已写入数据 span 按绝对位置 committedPosition 写回到对应段的已提交区域。 先确保段的 RunningIndex 是最新的，然后定位到起始段并逐段复制。
            if (span.IsEmpty)
                return;

            if (First == null)
                throw new InvalidOperationException("序列为空，无法更新提交内容。");

            // 保证 RunningIndex 与段位置同步
            First.UpdateRunningIndex();

            // 先定位起始段与偏移
            if (!TryGetSegmentByPosition(committedPosition, out var segment, out var segOffset))
                throw new InvalidOperationException("无法定位到提交位置所在的段。");

            int remaining = span.Length;
            int srcIndex = 0;

            // 逐段复制数据到各段的已提交内存
            while (remaining > 0 && segment != null)
            {
                // 当前段内从 segOffset 到已提交末尾可写入的长度
                int segCommitted = (int)segment.Committed;
                int can = segCommitted - segOffset;
                if (can > remaining) can = remaining;
                if (can > 0)
                {
                    // 复制到段的已提交内存
                    span.Slice(srcIndex, can).CopyTo(segment.Memory.Span.Slice(segOffset, can));
                    srcIndex += can;
                    remaining -= can;
                }

                // 向下一段继续，重置偏移
                segOffset = 0;
                segment = segment.Next;
            }

            if (remaining != 0)
            {
                // 理论上不会发生，因为上层已验证 committedPosition + span.Capacity <= Committed
                throw new InvalidOperationException("更新已提交内容时未能完成全部复制。");
            }
        }

        /// <summary>
        /// 根据当前提交长度和自动增长策略考虑是否增加 minimumSpanCommitted 的值，以指导未来的段创建。
        /// </summary>
        private void ConsiderMinimumSizeIncrease()
        {
            if (AutoIncreaseMinimumSpanCommitted && minimumSpanCommitted < MaximumAutoGrowSize)
            {
                int autoSize = System.Math.Min(MaximumAutoGrowSize, (int)System.Math.Min(int.MaxValue, Committed / 2));
                if (minimumSpanCommitted < autoSize)
                {
                    minimumSpanCommitted = autoSize;
                }
            }
        }

        #endregion Operations for segment management

        protected override void ReleaseProtected() => OwnerProvider.Release(this);

        public override void Clear()
        {
            First?.Release();
            First = null;
            Last = null;
            minimumSpanCommitted = 0;
        }

        public override T[] ToArray() => ((ReadOnlySequence<T>)this).ToArray();

        protected override MemoryHandle PinProtected(int elementIndex)
        {
            if (First == null)
                throw new InvalidOperationException("序列为空，无法固定元素。");

            if (!TryGetSegmentByPosition(elementIndex, out var segment, out var offset))
            {
                throw new ArgumentOutOfRangeException(nameof(elementIndex), "指定的元素索引超出序列范围。");
            }

            while (segment != null)
            {
                var next = segment.Next;
                segment.Pin(0);
                segment = next;
            }
            return default;
        }

        public override void Unpin()
        {
            base.Unpin();
            var segment = First;
            while (segment != null)
            {
                var next = segment.Next;
                segment.Unpin();
                segment = next;
            }
        }

        /// <summary>
        /// 将 <see cref="SequenceBuffer{T}"/> 隐式转换为 <see cref="ReadOnlySequence{T}"/>。
        /// </summary>
        /// <param name="sequence">源序列。</param>
        public static implicit operator ReadOnlySequence<T>(SequenceBuffer<T> sequence)
        {
            return sequence.First is SequenceBufferSegment<T> first && sequence.Last is SequenceBufferSegment<T> last
                ? new ReadOnlySequence<T>(first, 0, last, (int)last.Committed)
                : EmptySequence;
        }
    }
}