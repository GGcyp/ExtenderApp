using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 一个基于数组池（<see cref="ArrayPool{T}"/>）的临时帧集合，用于高效收集与批量处理 <see cref="Frame"/> 实例。
    /// </summary>
    /// <remarks>
    /// - 本类型为值类型（struct），自身不具备线程安全性；在并发场景下需由外部同步访问。  
    /// - 内部数组从 <see cref="ArrayPool{Frame}.Shared"/> 租用（仅在首次分配或扩容时）；调用 <see cref="Dispose"/> 会释放内部帧资源并将数组归还池中。  
    /// - 对集合中的每个 <see cref="Frame"/> 调用 <see cref="Frame.Dispose"/> 以释放其持有的 <see cref="ByteBlock"/>。<see cref="Clear"/> 会释放当前元素但不会归还内部数组；若需要归还数组请调用 <see cref="Dispose"/>。  
    /// - 因为 <see cref="Frame"/> 是只读值类型且 Dispose 会释放其内部缓冲，避免在多个副本上重复 Dispose 同一帧所持有的资源。  
    /// - 推荐用法：在使用结束时务必调用 <see cref="Dispose"/>（例如放入 using 或 try/finally），以避免数组与帧负载泄漏。
    /// </remarks>
    public struct PooledFrameList
    {
        private Frame[]? _array;
        private int _count;

        /// <summary>
        /// 当前集合中帧的数量。
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 指示集合是否尚未分配内部数组（空集合且未租用底层数组）。
        /// </summary>
        public bool IsEmpty => _array is null;

        /// <summary>
        /// 按索引访问集合中的帧。调用方应保证索引在 [0, <see cref="Count"/>) 范围内。
        /// </summary>
        /// <param name="index">元素索引。</param>
        /// <returns>指定位置的 <see cref="Frame"/>。</returns>
        public Frame this[int index]
        {
            get => _array![index];
            set => _array![index] = value;
        }

        /// <summary>
        /// 以只读切片形式获取当前已存储的帧集合（不分配新数组）。
        /// </summary>
        /// <returns>表示当前元素的 <see cref="ReadOnlySpan{Frame}"/>。</returns>
        public ReadOnlySpan<Frame> AsSpan()
            => _array is null ? ReadOnlySpan<Frame>.Empty : _array.AsSpan(0, _count);

        /// <summary>
        /// 将一个帧添加到集合末尾。必要时进行扩容（从数组池租用更大的数组并拷贝现有元素）。
        /// </summary>
        /// <param name="frame">要添加的帧。调用后集合接管该帧的生命周期，最终会在 <see cref="Clear"/> 或 <see cref="Dispose"/> 时调用 <see cref="Frame.Dispose"/>。</param>
        public void Add(Frame frame)
        {
            EnsureCapacity(_count + 1);
            _array![_count++] = frame;
        }

        /// <summary>
        /// 清空集合：对当前所有元素调用 <see cref="Frame.Dispose"/> 并将计数重置为 0，但不将内部数组归还数组池（保留以便重用以降低分配代价）。
        /// </summary>
        public void Clear()
        {
            if (_array is null)
                return;

            for (int i = 0; i < _count; i++)
            {
                _array![i].Dispose();
            }
            // 仅重置计数；避免清零以减少成本（需要彻底清理可改为 Array.Clear）
            _count = 0;
        }

        /// <summary>
        /// 释放集合：对当前元素逐一调用 <see cref="Frame.Dispose"/>，并将内部数组归还到数组池，同时重置内部状态。
        /// </summary>
        public void Dispose()
        {
            if (_array is not null)
            {
                for (int i = 0; i < _count; i++)
                {
                    _array[i].Dispose();
                }
                ArrayPool<Frame>.Shared.Return(_array);
                _array = null;
                _count = 0;
            }
        }

        /// <summary>
        /// 确保内部数组容量至少为指定大小；若不足则从数组池租用更大数组并搬移现有元素，归还旧数组。
        /// </summary>
        /// <param name="size">需要的最小容量。</param>
        private void EnsureCapacity(int size)
        {
            if (_array is null)
            {
                _array = ArrayPool<Frame>.Shared.Rent(System.Math.Max(4, size));
                _count = 0;
                return;
            }

            if (_array.Length >= size)
                return;

            int newCap = _array.Length * 2;
            if (newCap < size) newCap = size;

            var newArr = ArrayPool<Frame>.Shared.Rent(newCap);
            _array.AsSpan(0, _count).CopyTo(newArr);
            ArrayPool<Frame>.Shared.Return(_array, clearArray: true);
            _array = newArr;
        }
    }
}