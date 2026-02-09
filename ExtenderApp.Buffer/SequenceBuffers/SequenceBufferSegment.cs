using System.Buffers;

namespace ExtenderApp.Buffer.Sequence
{
    /// <summary>
    /// 表示序列中单个缓冲段的抽象基类，封装段的已提交长度、可用空间以及与相邻段的链表关系。
    /// </summary>
    /// <remarks>继承自 <see cref="ReadOnlySequenceSegment{T}"/>，用于在基于段的序列实现中维护 RunningIndex、前驱/后继关系并暴露段内内存与计数信息。 派生类需要提供具体的内存访问、已提交长度、可写容量以及推进/重置的实现。</remarks>
    public abstract class SequenceBufferSegment<T> : ReadOnlySequenceSegment<T>, IPinnable
    {
        public static readonly SequenceBufferSegment<T> Empty = new EmptySequenceBufferSegment<T>();

        protected internal SequenceBufferSegmentProvider<T>? SegmentProvider;

        /// <summary>
        /// 当前段在整个序列中的起始索引（相对于序列起点）。
        /// </summary>
        internal long SegmentStart { get; set; }

        /// <summary>
        /// 当前段在序列中的结束索引（不包含），等于 <see cref="SegmentStart"/> + <see cref="Committed"/>.
        /// </summary>
        internal long SegmentEnd => SegmentStart + Committed;

        /// <summary>
        /// 当前段所持有的可读/已提交内存片（派生类提供具体实现）。
        /// </summary>
        protected internal abstract new Memory<T> Memory { get; }

        /// <summary>
        /// 已提交（已写入）内存片，长度等于 <see cref="Committed"/>。派生类提供具体实现以反映当前段中已写入的数据范围。
        /// </summary>
        protected internal abstract ReadOnlyMemory<T> CommittedMemory { get; }

        /// <summary>
        /// 当前段中已提交（已写入）元素的数量。
        /// </summary>
        protected internal abstract long Committed { get; }

        /// <summary>
        /// 当前段尚可写入的元素数量（剩余可用空间）。
        /// </summary>
        protected internal abstract int Available { get; }

        /// <summary>
        /// 获取当前段在序列中的前一个段（如果存在），否则为 null。
        /// </summary>
        internal SequenceBufferSegment<T>? Prev { get; set; }

        /// <summary>
        /// 获取当前段在序列中的下一个段（如果存在），否则为 null。 派生类通过 <see cref="SetNext"/> 方法设置后续段以维护链表关系和运行索引更新。
        /// </summary>
        internal new SequenceBufferSegment<T>? Next
        {
            get => base.Next as SequenceBufferSegment<T>;
            set => base.Next = value;
        }

        /// <summary>
        /// 将指定段设置为当前段的下一个段，并自动更新后续段的 RunningIndex 以保持索引连续性。调用方应确保传入的段实例正确配置并且不与当前链表中的其他段冲突（如重复使用同一实例）。如果调用方已经在其他上下文中更新了 RunningIndex，可以将 <paramref
        /// name="needUpdateIndex"/> 设置为 false 以避免重复计算。
        /// </summary>
        /// <param name="segment">要设置为下一个段的实例（可以为 null 表示尾部）。</param>
        /// <param name="needUpdateIndex">指示是否需要更新后续段的 RunningIndex（默认为 true）。如果调用方已经在其他上下文中更新了 RunningIndex，可以将此参数设置为 false 以避免重复计算。</param>
        internal void SetNext(SequenceBufferSegment<T>? segment, bool needUpdateIndex = true)
        {
            Next = segment;
            if (segment != null)
            {
                segment.Prev = this;
                if (needUpdateIndex) segment.UpdateRunningIndex();
            }
        }

        /// <summary>
        /// 获取一个至少包含 <paramref name="sizeHint"/> 个元素的可写 <see cref="Memory{T}"/>。
        /// </summary>
        /// <param name="sizeHint">建议的最小可用大小（可为 0，表示不作特殊建议）。</param>
        /// <returns>用于写入的 <see cref="Memory{T}"/>。</returns>
        internal Memory<T> GetMemory(int sizeHint = 0)
        {
            return GetMemotyProtected(sizeHint);
        }

        /// <summary>
        /// 获取一个至少包含 <paramref name="sizeHint"/> 个元素的可写 <see cref="Span{T}"/>。
        /// </summary>
        /// <param name="sizeHint">建议的最小可用大小（可为 0，表示不作特殊建议）。</param>
        /// <returns>用于写入的 <see cref="Span{T}"/>。</returns>
        internal Span<T> GetSpan(int sizeHint = 0)
        {
            return GetSpanProtected(sizeHint);
        }

        /// <summary>
        /// 重新计算并更新自身及后续所有段的 <see cref="RunningIndex"/>，以确保每段的运行索引反映序列中的真实位置。
        /// </summary>
        protected internal void UpdateRunningIndex()
        {
            RunningIndex = Prev != null ? Prev.RunningIndex + Prev.Committed : 0;
            Next?.UpdateRunningIndex();
        }

        /// <summary>
        /// 将当前段的已提交长度向前推进指定数量（派生类应在此方法中更新内部已写入计数并触发必要的通知）。
        /// </summary>
        /// <param name="count">推进的元素数量（必须为非负值）。</param>
        protected internal abstract void Advance(int count);

        /// <summary>
        /// 将当前段重置为初始状态，清除前后段引用并将运行索引重置为 0。调用方应确保在调用此方法后不再使用该段实例，或在重新配置后再次使用。 派生类在 <see cref="ReleaseProtected"/> 中实现具体的资源释放或状态重置逻辑（如清空内存、重置计数等）。
        /// </summary>
        public void Release()
        {
            if (Prev != null)
                Prev.Next = null;
            Prev = null;

            var next = Next;
            while (next != null)
            {
                next.Prev = null;
                next.Next = null;
                next.RunningIndex = 0;
                next.Release();

                next = next.Next;
            }
            Next = null;
            RunningIndex = 0;
            SegmentProvider?.ReleaseSegment(this);
        }

        #region 抽象成员 - 派生类必须实现

        /// <summary>
        /// 派生类提供具体的可写内存获取实现。实现应返回一个从当前写入位置开始、至少包含 <paramref name="sizeHint"/> 个元素的 <see cref="Memory{T}"/>。
        /// </summary>
        /// <param name="sizeHint">期望的最小可写元素数（可为 0 表示不作特殊建议）。</param>
        /// <returns>可用于写入的 <see cref="Memory{T}"/>（从当前写入位置到可用末尾或扩容后的空间）。</returns>
        protected abstract Memory<T> GetMemotyProtected(int sizeHint = 0);

        /// <summary>
        /// 派生类提供具体的可写跨度获取实现。实现应返回一个从当前写入位置开始、至少包含 <paramref name="sizeHint"/> 个元素的 <see cref="Span{T}"/>。
        /// </summary>
        /// <param name="sizeHint">期望的最小可写元素数（可为 0 表示不作特殊建议）。</param>
        /// <returns>可用于写入的 <see cref="Span{T}"/>（从当前写入位置到可用末尾或扩容后的空间）。</returns>
        protected abstract Span<T> GetSpanProtected(int sizeHint = 0);

        /// <summary>
        /// 从指定元素索引处固定当前段的内存，返回一个 <see cref="MemoryHandle"/> 用于访问固定的内存位置。派生类应实现具体的固定逻辑（如调用 GCHandle.Alloc 或其他机制），并确保在不再需要访问时调用 <see cref="Unpin"/> 释放固定资源。
        /// </summary>
        /// <param name="elementIndex">指定索引</param>
        /// <returns><see cref="MemoryHandle"/> 实例</returns>
        public abstract MemoryHandle Pin(int elementIndex);

        /// <summary>
        /// 释放之前通过 <see cref="Pin(int)"/> 固定的内存资源。派生类应实现具体的释放逻辑（如调用 GCHandle.Free 或其他机制），以确保固定的内存能够被垃圾回收器正确管理。
        /// </summary>
        public abstract void Unpin();

        #endregion 抽象成员 - 派生类必须实现
    }
}