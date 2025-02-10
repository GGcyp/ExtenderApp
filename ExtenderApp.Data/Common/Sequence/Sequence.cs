using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace ExtenderApp.Data.File
{
    /// <summary>
    /// 泛型序列类，实现了IBufferWriter<T>和IDisposable接口。
    /// </summary>
    /// <typeparam name="T">序列中元素的类型。</typeparam>
    public class Sequence<T> : IBufferWriter<T>, IDisposable
    {
        /// <summary>
        /// 自动增长的最大大小。
        /// </summary>
        private const int MaximumAutoGrowSize = 32 * 1024;

        /// <summary>
        /// 从数组池中租用的默认长度。
        /// </summary>
        private static readonly int DefaultLengthFromArrayPool = 1 + (4095 / Unsafe.SizeOf<T>());

        /// <summary>
        /// 空的只读序列。
        /// </summary>
        private static readonly ReadOnlySequence<T> Empty = new ReadOnlySequence<T>(SequenceSegment.Empty, 0, SequenceSegment.Empty, 0);

        /// <summary>
        /// 序列段对象池。
        /// </summary>
        private static readonly Stack<SequenceSegment> SegmentPool = new Stack<SequenceSegment>();

        /// <summary>
        /// 内存池。
        /// </summary>
        private readonly MemoryPool<T>? _memoryPool;

        /// <summary>
        /// 数组池。
        /// </summary>
        private readonly ArrayPool<T>? _arrayPool;

        /// <summary>
        /// 序列中的第一个段。
        /// </summary>
        private SequenceSegment? first;

        /// <summary>
        /// 序列中的最后一个段。
        /// </summary>
        private SequenceSegment? last;

        /// <summary>
        /// 获取或设置序列的最小跨度长度。
        /// </summary>
        public int MinimumSpanLength { get; set; }

        /// <summary>
        /// 获取或设置是否自动增加最小跨度长度。
        /// </summary>
        public bool AutoIncreaseMinimumSpanLength { get; set; }

        /// <summary>
        /// 获取此序列的只读版本。
        /// </summary>
        public ReadOnlySequence<T> AsReadOnlySequence => this;

        /// <summary>
        /// 获取序列的长度。
        /// </summary>
        public long Length => AsReadOnlySequence.Length;

        /// <summary>
        /// 使用默认的内存池初始化Sequence类的新实例。
        /// </summary>
        public Sequence() : this(ArrayPool<T>.Create())
        {
        }

        /// <summary>
        /// 使用指定的内存池初始化Sequence类的新实例。
        /// </summary>
        /// <param name="memoryPool">内存池。</param>
        public Sequence(MemoryPool<T> memoryPool)
        {
            if (memoryPool == null)
                throw new ArgumentNullException(nameof(memoryPool));

            _memoryPool = memoryPool;
            MinimumSpanLength = 0;
            AutoIncreaseMinimumSpanLength = true;
        }

        /// <summary>
        /// 使用指定的数组池初始化Sequence类的新实例。
        /// </summary>
        /// <param name="arrayPool">数组池。</param>
        public Sequence(ArrayPool<T> arrayPool)
        {
            if (arrayPool == null)
                throw new ArgumentNullException(nameof(arrayPool));

            _arrayPool = arrayPool;
            MinimumSpanLength = 0;
            AutoIncreaseMinimumSpanLength = true;
        }

        /// <summary>
        /// 推进序列的位置。
        /// </summary>
        /// <param name="count">推进的数量。</param>
        public void Advance(int count)
        {
            if (last is null)
                throw new InvalidOperationException("在获取内存之前不能进行推进操作");

            last!.Advance(count);
            ConsiderMinimumSizeIncrease();
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose() => Reset();

        /// <summary>
        /// 重置序列。
        /// </summary>
        public void Reset()
        {
            var current = first;
            while (current != null)
            {
                current = RecycleAndGetNext(current);
            }

            first = null;
            last = null;
        }

        /// <summary>
        /// 获取指定大小的内存。
        /// </summary>
        /// <param name="sizeHint">所需内存的大小提示。</param>
        /// <returns>指定大小的内存。</returns>
        public Memory<T> GetMemory(int sizeHint = 0) => GetSegment(sizeHint).AvailableMemory;

        /// <summary>
        /// 获取指定大小的跨度。
        /// </summary>
        /// <param name="sizeHint">所需跨度的大小提示。</param>
        /// <returns>指定大小的跨度。</returns>
        public Span<T> GetSpan(int sizeHint = 0) => GetSegment(sizeHint).RemainingSpan;

        /// <summary>
        /// 将只读内存附加到序列中。
        /// </summary>
        /// <param name="memory">要附加的只读内存。</param>
        public void Append(ReadOnlyMemory<T> memory)
        {
            if (memory.Length > 0)
            {
                var segment = SegmentPool.Count > 0 ? SegmentPool.Pop() : new SequenceSegment();
                segment.AssignForeign(memory);
                Append(segment);
            }
        }

        /// <summary>
        /// 获取一个段，该段具有指定大小的可用内存。
        /// </summary>
        /// <param name="sizeHint">所需内存的大小提示。</param>
        /// <returns>具有指定大小可用内存的段。</returns>
        private SequenceSegment GetSegment(int sizeHint)
        {
            //if (sizeHint < 0)
            //    throw new ArgumentOutOfRangeException(nameof(sizeHint));

            //if (sizeHint == 0)
            //    return last!;

            //int minBufferSize = -1;
            //if (last == null || last.WritableBytes < sizeHint)
            //{
            //    minBufferSize = System.Math.Max(MinimumSpanLength, sizeHint);
            //}

            //minBufferSize = minBufferSize == -1 ? DefaultLengthFromArrayPool : minBufferSize;
            //var segment = SegmentPool.Count > 0 ? SegmentPool.Pop() : new SequenceSegment();
            //if (_arrayPool != null)
            //{
            //    segment.Assign(_arrayPool.Rent(minBufferSize));
            //}
            //else
            //{
            //    segment.Assign(_memoryPool!.Rent(minBufferSize));
            //}

            //Append(segment);

            //return segment;

            //原先设计
            int? minBufferSize = null;
            if (sizeHint == 0)
            {
                if (last == null || last.WritableBytes == 0)
                {
                    minBufferSize = -1;
                }
            }
            else
            {
                if (last == null || last.WritableBytes < sizeHint)
                {
                    minBufferSize = System.Math.Max(MinimumSpanLength, sizeHint);
                }
            }

            if (minBufferSize.HasValue)
            {
                var segment = SegmentPool.Count > 0 ? SegmentPool.Pop() : new SequenceSegment();
                if (_arrayPool != null)
                {
                    segment.Assign(_arrayPool.Rent(minBufferSize.Value == -1 ? DefaultLengthFromArrayPool : minBufferSize.Value));
                }
                else
                {
                    segment.Assign(_memoryPool!.Rent(minBufferSize.Value));
                }

                Append(segment);
            }

            return last!;
        }

        /// <summary>
        /// 将段附加到序列中。
        /// </summary>
        /// <param name="segment">要附加的段。</param>
        private void Append(SequenceSegment segment)
        {
            if (last == null)
            {
                first = segment;
                last = segment;
                return;
            }

            if (last.Length > 0)
            {
                last.SetNext(segment);
                last = segment;
                return;
            }


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
            RecycleAndGetNext(last);
            last = segment;
        }

        /// <summary>
        /// 回收段并将其从序列中移除。
        /// </summary>
        /// <param name="segment">要回收的段。</param>
        /// <returns>回收后的下一个段。</returns>
        private SequenceSegment? RecycleAndGetNext(SequenceSegment segment)
        {
            var recycledSegment = segment;
            var nextSegment = segment.Next;
            recycledSegment.ResetMemory(_arrayPool);
            SegmentPool.Push(recycledSegment);
            return nextSegment;
        }

        /// <summary>
        /// 考虑是否增加最小跨度长度。
        /// </summary>
        private void ConsiderMinimumSizeIncrease()
        {
            if (AutoIncreaseMinimumSpanLength && MinimumSpanLength < MaximumAutoGrowSize)
            {
                int autoSize = System.Math.Min(MaximumAutoGrowSize, (int)System.Math.Min(int.MaxValue, Length / 2));
                if (MinimumSpanLength < autoSize)
                {
                    MinimumSpanLength = autoSize;
                }
            }
        }

        /// <summary>
        /// 将Sequence<T>隐式转换为ReadOnlySequence<T>。
        /// </summary>
        /// <param name="sequence">要转换的Sequence<T>对象。</param>
        /// <returns>转换后的ReadOnlySequence<T>对象。</returns>
        public static implicit operator ReadOnlySequence<T>(Sequence<T> sequence)
        {
            return sequence.first is SequenceSegment first && sequence.last is SequenceSegment last
                ? new ReadOnlySequence<T>(first, first.Start, last, last.End)
                : Empty;
        }


        /// <summary>
        /// 表示一个序列段，用于处理序列中的一段数据。
        /// </summary>
        /// <typeparam name="T">序列中元素的类型。</typeparam>
        private class SequenceSegment : ReadOnlySequenceSegment<T>
        {
            /// <summary>
            /// 获取一个空的 <see cref="SequenceSegment"/> 实例。
            /// </summary>
            internal static readonly SequenceSegment Empty = new SequenceSegment();

            /// <summary>
            /// 获取一个值，指示序列中的元素类型是否可能包含引用。
            /// </summary>
            private static readonly bool MayContainReferences = !typeof(T).GetTypeInfo().IsPrimitive;

            /// <summary>
            /// 获取或设置用于存储序列段数据的数组。
            /// </summary>
            private T[]? array;

            /// <summary>
            /// 获取序列段的起始索引。
            /// </summary>
            internal int Start { get; private set; }

            /// <summary>
            /// 获取序列段的结束索引。
            /// </summary>
            internal int End { get; private set; }

            /// <summary>
            /// 获取剩余的内存部分。
            /// </summary>
            internal Memory<T> RemainingMemory => AvailableMemory.Slice(End);

            /// <summary>
            /// 获取剩余的跨度部分。
            /// </summary>
            internal Span<T> RemainingSpan => AvailableMemory.Span.Slice(End);

            /// <summary>
            /// 获取或设置内存的所有者。
            /// </summary>
            internal IMemoryOwner<T>? MemoryOwner { get; private set; }

            /// <summary>
            /// 获取可用的内存。
            /// </summary>
            internal Memory<T> AvailableMemory => array ?? MemoryOwner?.Memory ?? default;

            /// <summary>
            /// 获取序列段的长度。
            /// </summary>
            internal int Length => End - Start;

            /// <summary>
            /// 获取可写入的字节数。
            /// </summary>
            internal int WritableBytes => AvailableMemory.Length - End;

            /// <summary>
            /// 获取或设置下一个序列段。
            /// </summary>
            internal new SequenceSegment? Next
            {
                get => (SequenceSegment?)base.Next;
                set => base.Next = value;
            }

            /// <summary>
            /// 获取一个值，指示序列段是否使用外部内存。
            /// </summary>
            internal bool IsForeignMemory => array == null && MemoryOwner == null;

            /// <summary>
            /// 将内存所有者分配给序列段。
            /// </summary>
            /// <param name="memoryOwner">内存所有者。</param>
            internal void Assign(IMemoryOwner<T> memoryOwner)
            {
                MemoryOwner = memoryOwner;
                Memory = memoryOwner.Memory;
            }

            /// <summary>
            /// 将数组分配给序列段。
            /// </summary>
            /// <param name="array">数组。</param>
            internal void Assign(T[] array)
            {
                this.array = array;
                Memory = array;
            }

            /// <summary>
            /// 将外部内存分配给序列段。
            /// </summary>
            /// <param name="memory">外部内存。</param>
            internal void AssignForeign(ReadOnlyMemory<T> memory)
            {
                Memory = memory;
                End = memory.Length;
            }

            /// <summary>
            /// 重置序列段的内存。
            /// </summary>
            /// <param name="arrayPool">数组池。</param>
            internal void ResetMemory(ArrayPool<T>? arrayPool)
            {
                ClearReferences(Start, End - Start);
                Memory = default;
                Next = null;
                RunningIndex = 0;
                Start = 0;
                End = 0;
                if (array != null)
                {
                    arrayPool!.Return(array);
                    array = null;
                }
                else
                {
                    MemoryOwner?.Dispose();
                    MemoryOwner = null;
                }
            }

            /// <summary>
            /// 设置下一个序列段。
            /// </summary>
            /// <param name="segment">下一个序列段。</param>
            internal void SetNext(SequenceSegment segment)
            {
                Next = segment;
                segment.RunningIndex = RunningIndex + Start + Length;

                if (!IsForeignMemory)
                {
                    Memory = AvailableMemory.Slice(0, Start + Length);
                }
            }

            /// <summary>
            /// 将序列段的结束索引向前移动指定的数量。
            /// </summary>
            /// <param name="count">要移动的数量。</param>
            internal void Advance(int count)
            {
                if (count < 0 || End + count >= Memory.Length)
                    throw new ArgumentOutOfRangeException(nameof(count), "count 必须是非负数，且移动后的结束索引不能超过内存长度。");

                End += count;
            }

            /// <summary>
            /// 将序列段的起始索引设置为指定的偏移量。
            /// </summary>
            /// <param name="offset">偏移量。</param>
            internal void AdvanceTo(int offset)
            {
                ClearReferences(Start, offset - Start);
                Start = offset;
            }

            /// <summary>
            /// 清除指定范围内的引用。
            /// </summary>
            /// <param name="startIndex">起始索引。</param>
            /// <param name="length">长度。</param>
            private void ClearReferences(int startIndex, int length)
            {
                if (MayContainReferences)
                {
                    AvailableMemory.Span.Slice(startIndex, length).Clear();
                }
            }
        }
    }
}
