using System.Buffers;
using System.Runtime.CompilerServices;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 内存块的抽象基类，表示一段可写入的内存区域，支持动态扩展和已写入数据的管理。
    /// </summary>
    /// <typeparam name="T">内存块中元素的类型。</typeparam>
    public abstract class MemoryBlock<T> : DisposableObject, IBufferWriter<T>
    {
        // 使用 RuntimeHelpers.IsReferenceOrContainsReferences<T>() 在运行时判定是否需要清理引用。
        private static readonly bool MayContainReferences = RuntimeHelpers.IsReferenceOrContainsReferences<T>();

        /// <summary>
        /// 获取为空段的单例实例（表示不包含任何数据的占位段）。
        /// 派生类型需提供一个空实现：<see cref="EmptyMemoryBlock{T}"/>.
        /// </summary>
        public static readonly MemoryBlock<T> Empty = new EmptyMemoryBlock<T>();

        /// <summary>
        /// 当前段内有效数据的起始索引（相对于 <see cref="AvailableMemory"/>）。
        /// 起始索引与 <see cref="end"/> 一起定义段的有效范围 [start, end)。
        /// </summary>
        private int start;

        /// <summary>
        /// 当前段内有效数据的结束索引（不含）。
        /// </summary>
        private int end;

        /// <summary>
        /// 获取当前段内已写入的内存部分（相对于底层可用内存的切片）。
        /// </summary>
        internal Memory<T> Memory => AvailableMemory.Slice(start, Consumed);

        /// <summary>
        /// 获取当前段内已写入的跨度部分（同步访问，非线程安全）。
        /// </summary>
        internal Span<T> Span => Memory.Span;

        /// <summary>
        /// 获取当前段已使用的元素数量，等于 <see cref="end"/> 减去 <see cref="start"/>。
        /// </summary>
        public int Consumed => end - start;

        /// <summary>
        /// 获取当前段剩余可写入的元素数量，等于 <see cref="AvailableMemory"/> 的长度减去 <see cref="end"/>。
        /// </summary>
        public int WritableBytes => AvailableMemory.Length - end;

        /// <summary>
        /// 获取剩余的内存部分（可写区域），供调用方写入数据。
        /// </summary>
        public Memory<T> RemainingMemory => AvailableMemory.Slice(end);

        /// <summary>
        /// 获取剩余的跨度部分（可写区域），供调用方写入数据。
        /// </summary>
        public Span<T> RemainingSpan => RemainingMemory.Span;

        /// <summary>
        /// 获取当前段内已写入的只读内存部分。
        /// </summary>
        public ReadOnlyMemory<T> ConsumedMemory => Memory;

        /// <summary>
        /// 获取当前段内已写入的只读跨度部分。
        /// </summary>
        public ReadOnlySpan<T> ConsumedSpan => Span;

        /// <summary>
        /// 检查当前内存块是否为空（即未写入任何数据）。
        /// </summary>
        public bool IsEmpty => AvailableMemory.IsEmpty || Consumed == 0;

        /// <summary>
        /// 实现 IBufferWriter{T}：返回至少包含 <paramref name="sizeHint"/> 个可写元素的 Memory。
        /// 派生类通过重写 <see cref="EnsureCapacity(int)"/> 来确保容量。
        /// </summary>
        /// <param name="sizeHint">期望的最小可写元素数（可选），当为 0 时仅保证至少 1 个可写元素（视实现而定）。</param>
        /// <returns>用于写入的可写内存片段（从当前写入位置到底层可用内存末尾）。</returns>
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return RemainingMemory;
        }

        /// <summary>
        /// 实现 IBufferWriter{T}：返回至少包含 <paramref name="sizeHint"/> 个可写元素的 Span。
        /// 派生类通过重写 <see cref="EnsureCapacity(int)"/> 来确保容量。
        /// </summary>
        /// <param name="sizeHint">期望的最小可写元素数（可选）。</param>
        /// <returns>用于写入的可写跨度（从当前写入位置到底层可用内存末尾）。</returns>
        public Span<T> GetSpan(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return RemainingSpan;
        }

        /// <summary>
        /// 当已写入范围发生变化时触发的事件。仅在同一线程/同步语境下绑定和触发才安全。
        /// 外部可订阅以在段内容改变时更新关联状态（例如序列长度缓存）。
        /// </summary>
        internal event Action? ConsumedChanged;

        /// <summary>
        /// 当前内存块被释放时触发的事件。
        /// </summary>
        internal event Action? Released;

        /// <summary>
        /// 当已写入范围变化时调用以触发 <see cref="ConsumedChanged"/> 事件。
        /// </summary>
        private void OnConsumedChanged() => ConsumedChanged?.Invoke();

        /// <summary>
        /// 当内存块被释放时调用以触发 <see cref="Released"/> 事件。
        /// </summary>
        private void OnReleased() => Released?.Invoke();

        /// <summary>
        /// 派生类应提供的底层可用内存（包含已写入与未写入部分）。
        /// 该属性不应分配新的底层存储；派生类负责提供稳定的 Memory 实现（例如来自池或 MemoryOwner）。
        /// </summary>
        protected abstract Memory<T> AvailableMemory { get; }

        #region Operations

        /// <summary>
        /// 将结束索引向前移动指定数量，表示向段中写入了更多元素。
        /// 实现了 IBufferWriter{T}.Advance 的语义检查。
        /// </summary>
        /// <param name="count">要前移的元素数量（非负）。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="count"/> 为负或移动后 <see cref="end"/> 超过底层内存长度时抛出。
        /// </exception>
        public void Advance(int count)
        {
            if (count < 0 || end + count > AvailableMemory.Length)
                throw new ArgumentOutOfRangeException(nameof(count), "count 必须是非负数，且移动后的结束索引不能超过内存长度。");

            end += count;
            OnConsumedChanged();
        }

        /// <summary>
        /// 将起始索引移动到指定偏移（丢弃起始到偏移之间的数据）。
        /// 同时会在必要时清理被丢弃范围内的引用以允许 GC 回收并防止数据泄露。
        /// </summary>
        /// <param name="offset">新的起始索引，必须位于当前 [start, end] 范围内。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="offset"/> 不在合法范围内时抛出。</exception>
        public void Seek(int offset)
        {
            if (offset < start || offset > end)
                throw new ArgumentOutOfRangeException(nameof(offset), "offset 必须在当前段的起始和结束索引之间。");

            ClearReferences(start, offset - start);
            start = offset;
            OnConsumedChanged();
        }

        /// <summary>
        /// 将结束索引向后回退指定数量（相当于从段尾移除数据）。
        /// 回退时会在被移除的范围内清除引用（如必要）。
        /// </summary>
        /// <param name="count">要回退的元素数量（非负）。</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 当 <paramref name="count"/> 为负或回退后 <see cref="end"/> 小于 <see cref="start"/> 时抛出。
        /// </exception>
        public void Rewind(int count)
        {
            if (count < 0 || end - count < start)
                throw new ArgumentOutOfRangeException(nameof(count), "count 必须是非负数，且移动后的结束索引不能小于起始索引。");

            end -= count;
            ClearReferences(end, count);
            OnConsumedChanged();
        }

        /// <summary>
        /// 将 <paramref name="span"/> 复制到当前段已写入区域的指定偏移位置（偏移相对于本段已写入区域起始）。
        /// 仅在目标范围完全位于已写入区域内时生效；不会修改已写入长度。
        /// </summary>
        /// <param name="span">要复制的来源数据。</param>
        /// <param name="consumedPosition">目标偏移（相对于本段已写入区域起始，默认 0）。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="consumedPosition"/> 或 <paramref name="span"/> 超出已写入区域时抛出。</exception>
        public void UpdateConsumed(Span<T> span, int consumedPosition = 0)
        {
            if (span.IsEmpty)
                return;

            if (consumedPosition < 0 || consumedPosition + span.Length > Consumed)
                throw new ArgumentOutOfRangeException(nameof(consumedPosition), "consumedPosition 必须在已写入范围内，且 span 能完全写入。");

            span.CopyTo(Span.Slice(consumedPosition));
        }

        /// <summary>
        /// 在指定范围内清除引用类型的元素（设置为默认值），以便被回收并防止数据泄露。
        /// 仅当元素类型为引用或包含引用字段时才执行（由 <see cref="MayContainReferences"/> 判定）。
        /// </summary>
        /// <param name="startIndex">范围起始索引（相对于段的可用内存）。</param>
        /// <param name="length">要清除的元素数量。</param>
        protected void ClearReferences(int startIndex, int length)
        {
            if (MayContainReferences)
            {
                AvailableMemory.Span.Slice(startIndex, length).Clear();
            }
        }

        /// <summary>
        /// 尝试将当前序列段已写入的数据复制到另一段序列段中。
        /// 仅在目标段有足够可写空间时才会复制全部已写入数据。
        /// </summary>
        /// <param name="segment">目标序列段。</param>
        /// <returns>如果复制成功（全部已写入数据被复制）则返回 true，否则返回 false。</returns>
        /// <remarks>
        /// 复制使用 <see cref="Memory.CopyTo(Memory{T})"/> 语义：从本段已写入部分到目标段的可写部分的直接复制。
        /// </remarks>
        public bool TryCopyTo(MemoryBlock<T> segment)
        {
            if (segment == null || segment.WritableBytes < Consumed)
                return false;

            Memory.CopyTo(segment.RemainingMemory);
            return true;
        }

        /// <summary>
        /// 清空已写入数据并在必要时清除引用，恢复到初始未使用状态。
        /// 此方法会触发 <see cref="ConsumedChanged"/> 事件。
        /// </summary>
        public void Clear()
        {
            ClearReferences(start, Consumed);
            start = 0;
            end = 0;
            OnConsumedChanged();
        }

        /// <summary>
        /// 反转已写入区域中从 <paramref name="start"/> 开始的指定长度段内的元素顺序。
        /// </summary>
        /// <param name="start">相对于已写入区域起始的起始索引。</param>
        /// <param name="length">要反转的长度。</param>
        /// <exception cref="ArgumentOutOfRangeException">当参数不定义一个有效的已写入范围时抛出。</exception>
        public void Reverse(int start, int length)
        {
            if (start < 0 || length < 0 || start + length > Consumed)
                throw new ArgumentOutOfRangeException("start 和 length 必须定义一个有效的已写入范围。");
            Span.Slice(start, length).Reverse();
        }

        /// <summary>
        /// 对整个已写入区域执行反转操作（将已写入元素顺序反转）。
        /// </summary>
        public void Reverse()
        {
            Span.Reverse();
        }

        #endregion Operations

        /// <summary>
        /// 回收段资源，将各属性重置为初始状态（供回收池使用）。
        /// 派生实现应在 <see cref="ReleaseProtected"/> 中释放底层所有权资源（例如数组归还或 MemoryOwner.Dispose）。
        /// </summary>
        public void Release()
        {
            start = 0;
            end = 0;
            ReleaseProtected();
            OnReleased();
        }

        /// <summary>
        /// 回收段资源的受保护方法，供派生类实现特定的清理逻辑（例如释放 IMemoryOwner 或归还数组）。
        /// </summary>
        protected abstract void ReleaseProtected();

        /// <summary>
        /// 确保段有足够的可写空间以容纳指定数量的元素。
        /// 派生类应在此方法内扩展或分配底层存储（若必要），并保证调用者随后从 <see cref="RemainingMemory"/> / <see cref="RemainingSpan"/> 获取到的容量至少满足 <paramref name="sizeHint"/>。
        /// </summary>
        /// <param name="sizeHint">所需的最小可写空间大小（当为 0 时实现可选择保持当前容量或最低策略）。</param>
        protected abstract void EnsureCapacity(int sizeHint);
    }
}