using System.Buffers;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Data.Binary
{
    /// <summary>
    /// 内部引用结构体 <see cref="SequenceReader{T}"/>，用于高效地读取和遍历 <see cref="ReadOnlySequence{T}"/> 或 <see cref="ReadOnlyMemory{T}"/>。
    /// </summary>
    /// <typeparam name="T">元素的类型，必须是未管理的并且实现了 <see cref="IEquatable{T}"/> 接口。</typeparam>
    internal ref struct SequenceReader<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        /// 当前读取位置。
        /// </summary>
        private SequencePosition currentPosition;

        /// <summary>
        /// 下一个读取位置。
        /// </summary>
        private SequencePosition nextPosition;

        /// <summary>
        /// 标记是否还有更多数据可读。
        /// </summary>
        private bool moreData;

        /// <summary>
        /// 序列的总长度。
        /// </summary>
        private long length;

        /// <summary>
        /// 获取一个值，指示是否已到达序列末尾。
        /// </summary>
        public readonly bool End => !moreData;

        /// <summary>
        /// 获取当前读取的序列。
        /// </summary>
        public ReadOnlySequence<T> Sequence { get; private set; }

        /// <summary>
        /// 获取当前读取位置。
        /// </summary>
        public SequencePosition Position
            => Sequence.GetPosition(CurrentSpanIndex, currentPosition);

        /// <summary>
        /// 获取当前读取的数据片段。
        /// </summary>
        public ReadOnlySpan<T> CurrentSpan { get; private set; }

        /// <summary>
        /// 获取当前数据片段的索引。
        /// </summary>
        public int CurrentSpanIndex { get; private set; }

        /// <summary>
        /// 获取当前未读取的数据片段。
        /// </summary>
        public readonly ReadOnlySpan<T> UnreadSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CurrentSpan.Slice(CurrentSpanIndex);
        }

        /// <summary>
        /// 获取已消耗（读取）的元素数量。
        /// </summary>
        public long Consumed { get; private set; }

        /// <summary>
        /// 获取剩余未读取的元素数量。
        /// </summary>
        public long Remaining => Length - Consumed;

        /// <summary>
        /// 获取序列的总长度。
        /// </summary>
        public long Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (length < 0)
                {
                    // 使用ReadOnlySequence<T>时，初始时先将序列总长度设置为 -1，后续会在需要时进行实际计算并缓存。
                    length = Sequence.Length;
                }
                return length;
            }
        }

        /// <summary>
        /// 获取一个值，指示序列是否为空。
        /// </summary>
        public bool IsEmpty => Length == 0;

        /// <summary>
        /// 使用指定的只读序列初始化 <see cref="SequenceReader{T}"/>。
        /// </summary>
        /// <param name="sequence">要读取的只读序列。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceReader(scoped in ReadOnlySequence<T> sequence)
        {
            this.CurrentSpanIndex = 0;
            this.Consumed = 0;
            this.Sequence = sequence;
            this.currentPosition = sequence.Start;
            this.length = -1;

            ReadOnlySpan<T> first = sequence.First.Span;
            this.nextPosition = sequence.GetPosition(first.Length);
            this.CurrentSpan = first;
            this.moreData = first.Length > 0;

            if (!this.moreData && !sequence.IsSingleSegment)
            {
                this.moreData = true;
                this.GetNextSpan();
            }
        }

        /// <summary>
        /// 使用指定的只读内存初始化 <see cref="SequenceReader{T}"/>。
        /// </summary>
        /// <param name="memory">要读取的只读内存。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceReader(ReadOnlyMemory<T> memory) : this(new ReadOnlySequence<T>(memory))
        {
        }

        /// <summary>
        /// 尝试从当前位置预览下一个元素。
        /// </summary>
        /// <param name="value">如果成功预览到元素，则将其值赋给该参数；否则，该参数将被设置为默认值。</param>
        /// <returns>如果成功预览到下一个元素，则返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T value)
        {
            if (moreData)
            {
                value = CurrentSpan[CurrentSpanIndex];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// 尝试从当前位置读取下一个元素。
        /// </summary>
        /// <param name="value">如果成功读取到元素，则将其值赋给该参数；否则，该参数将被设置为默认值。</param>
        /// <returns>如果成功读取到元素，则返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead(out T value)
        {
            if (End)
            {
                value = default;
                return false;
            }

            value = CurrentSpan[CurrentSpanIndex];
            CurrentSpanIndex++;
            Consumed++;

            if (CurrentSpanIndex >= CurrentSpan.Length)
            {
                GetNextSpan();
            }

            return true;
        }

        /// <summary>
        /// 将读取位置回退指定的元素数量。
        /// </summary>
        /// <param name="count">要回退的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Rewind(long count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // 将已消耗（读取）的元素总数量（Consumed）减去要回退的数量，相当于将读取位置往回移动了相应的元素个数。
            Consumed -= count;

            // 判断当前正在读取的数据片段（CurrentSpan）内的索引（CurrentSpanIndex）是否大于等于要回退的数量，
            // 如果是，说明可以直接在当前片段内进行回退操作，将当前片段内的索引减去回退的数量（转换为 int 类型，因为 CurrentSpanIndex 是 int 类型），
            // 并将 moreData 设置为 true，表示还有数据可读（回退之后当前位置还有未读数据）。
            if (CurrentSpanIndex >= count)
            {
                CurrentSpanIndex -= (int)count;
                moreData = true;
            }
            else
            {
                // 如果当前片段内的索引不够回退指定的数量，并且当前是基于序列（usingSequence 为 true）进行读取的情况，
                // 说明需要跨越片段往回扫描，调用 RetreatToPreviousSpan 方法来处理这种复杂的回退操作，
                // 传入当前已经消耗的元素数量（Consumed）作为参数。
                // 因为要回退到之前的某个位置，可能涉及多个片段的调整，所以需要更复杂的逻辑处理。
                RetreatToPreviousSpan(Consumed);
            }
        }

        /// <summary>
        /// 回退到前一个数据片段。
        /// </summary>
        /// <param name="consumed">已消耗的元素数量。</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RetreatToPreviousSpan(long consumed)
        {
            ResetReader();
            Advance(consumed);
        }

        /// <summary>
        /// 重置读取器的状态。
        /// </summary>
        private void ResetReader()
        {
            CurrentSpanIndex = 0;
            Consumed = 0;
            currentPosition = Sequence.Start;
            nextPosition = currentPosition;

            if (Sequence.TryGet(ref nextPosition, out ReadOnlyMemory<T> memory, advance: true))
            {
                moreData = true;

                if (memory.Length == 0)
                {
                    CurrentSpan = default;
                    GetNextSpan();
                }
                else
                {
                    CurrentSpan = memory.Span;
                }
            }
            else
            {
                moreData = false;
                CurrentSpan = default;
            }
        }

        /// <summary>
        /// 获取下一个数据片段。
        /// </summary>
        private void GetNextSpan()
        {
            if (!Sequence.IsSingleSegment)
            {
                SequencePosition previousNextPosition = nextPosition;
                while (Sequence.TryGet(ref nextPosition, out ReadOnlyMemory<T> memory, advance: true))
                {
                    currentPosition = previousNextPosition;
                    if (memory.Length > 0)
                    {
                        CurrentSpan = memory.Span;
                        CurrentSpanIndex = 0;
                        return;
                    }
                    else
                    {
                        CurrentSpan = default;
                        CurrentSpanIndex = 0;
                        previousNextPosition = nextPosition;
                    }
                }
            }
            moreData = false;
        }

        /// <summary>
        /// 将读取器前进指定的元素数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(long count)
        {
            const long TooBigOrNegative = unchecked((long)0xFFFFFFFF80000000);
            if ((count & TooBigOrNegative) == 0 && CurrentSpan.Length - CurrentSpanIndex > (int)count)
            {
                CurrentSpanIndex += (int)count;
                Consumed += count;
            }
            else
            {
                AdvanceToNextSpan(count);
            }
        }

        /// <summary>
        /// 在当前数据片段内前进指定的元素数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceCurrentSpan(long count)
        {
            Consumed += count;
            CurrentSpanIndex += (int)count;
            if (CurrentSpanIndex >= CurrentSpan.Length)
            {
                GetNextSpan();
            }
        }

        /// <summary>
        /// 尝试向前移动指定的元素数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        /// <returns>如果成功前进，则返回 true；否则返回 false。</returns>
        public bool TryAdvance(long count)
        {
            if (Remaining < count)
            {
                return false;
            }
            Advance(count);
            return true;
        }

        /// <summary>
        /// 前进到下一个数据片段，并尝试读取指定的元素数量。
        /// </summary>
        /// <param name="count">要前进的元素数量。</param>
        private void AdvanceToNextSpan(long count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Consumed += count;
            while (moreData)
            {
                int remaining = CurrentSpan.Length - CurrentSpanIndex;
                if (remaining > count)
                {
                    CurrentSpanIndex += (int)count;
                    count = 0;
                    break;
                }
                CurrentSpanIndex += remaining;
                count -= remaining;
                GetNextSpan();
                if (count == 0)
                {
                    break;
                }
            }
            if (count != 0)
            {
                Consumed -= count;
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        /// <summary>
        /// 尝试将数据复制到目标跨度中。
        /// </summary>
        /// <param name="destination">目标复制目的地。</param>
        /// <returns>如果成功复制，则返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> destination)
        {
            ReadOnlySpan<T> firstSpan = UnreadSpan;
            if (firstSpan.Length >= destination.Length)
            {
                firstSpan.Slice(0, destination.Length).CopyTo(destination);
                return true;
            }

            return !Sequence.IsEmpty && TryCopyMultisegment(destination);
        }

        /// <summary>
        /// 尝试从多个数据片段中复制数据到目标跨度中。
        /// </summary>
        /// <param name="destination">目标复制目的地。</param>
        /// <returns>如果成功复制，则返回 true；否则返回 false。</returns>
        private readonly bool TryCopyMultisegment(Span<T> destination)
        {
            long length = this.length < 0 ? Sequence.Length : this.length;
            long remaining = length - Consumed;
            if (remaining < destination.Length)
            {
                return false;
            }

            ReadOnlySpan<T> firstSpan = UnreadSpan;
            firstSpan.CopyTo(destination);
            int copied = firstSpan.Length;
            SequencePosition next = nextPosition;
            while (Sequence.TryGet(ref next, out ReadOnlyMemory<T> nextSegment, true))
            {
                if (nextSegment.Length > 0)
                {
                    ReadOnlySpan<T> nextSpan = nextSegment.Span;
                    int toCopy = Math.Min(nextSpan.Length, destination.Length - copied);
                    nextSpan.Slice(0, toCopy).CopyTo(destination.Slice(copied));
                    copied += toCopy;
                    if (copied >= destination.Length)
                    {
                        break;
                    }
                }
            }
            return true;
        }
    }
}
