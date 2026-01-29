using System.Buffers;

namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个泛型先进先出 (FIFO) 的循环队列（也称为环形缓冲区）。
    /// </summary>
    /// <typeparam name="T">指定队列中元素的类型。</typeparam>
    public class CircularQueue<T> : DisposableObject
    {
        private readonly ArrayPool<T>? _pool;

        private T[] buffer;

        /// <summary>
        /// 队列头索引。
        /// </summary>
        private int _head;

        /// <summary>
        /// 队列尾索引。
        /// </summary>
        private int _tail;

        /// <summary>
        /// 获取队列中包含的元素数量。
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 获取队列可以容纳的元素总数。
        /// </summary>
        public int Capacity => buffer.Length;

        /// <summary>
        /// 使用空缓冲区初始化一个 <see cref="CircularQueue{T}"/> 的新实例。
        /// 此构造函数主要用于序列化或占位场景，实际使用应调用带容量的构造函数。
        /// </summary>
        public CircularQueue()
        {
            _pool = null;
            buffer = Array.Empty<T>();
            _head = 0;
            _tail = 0;
            Count = 0;
        }

        /// <summary>
        /// 使用指定容量初始化一个 <see cref="CircularQueue{T}"/> 的新实例，使用共享的 <see cref="ArrayPool{T}"/>.
        /// </summary>
        /// <param name="capacity">队列可以存储的初始元素数。</param>
        public CircularQueue(int capacity)
            : this(capacity, ArrayPool<T>.Shared)
        {
        }

        /// <summary>
        /// 初始化 <see cref="CircularQueue{T}"/> 类的新实例，该实例为空且具有指定的初始容量。
        /// </summary>
        /// <param name="capacity">队列可以存储的初始元素数。</param>
        /// <param name="pool">用于租用和归还底层数组的 <see cref="ArrayPool{T}"/> 实例。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="capacity"/> 为负数时引发。</exception>
        public CircularQueue(int capacity, ArrayPool<T> pool)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            buffer = _pool.Rent(capacity);
            _head = 0;
            _tail = 0;
            Count = 0;
        }

        /// <summary>
        /// 将一个或多个元素添加到队列的末尾。如果可用空间不足，则只添加适合的元素。
        /// 当存在数组池（通过构造函数传入）时，会尝试扩充底层缓冲区以容纳更多元素；否则按可用空间截断输入。
        /// </summary>
        /// <param name="values">要添加到队列的元素所在的只读内存区域。</param>
        /// <returns>成功添加到队列中的元素数量。</returns>
        public int Enqueue(ReadOnlySpan<T> values)
        {
            ThrowIfDisposed();
            if (values.IsEmpty)
                return 0;

            int desired = values.Length;
            int available = Capacity - Count;

            // 如果没有足够空间并且我们有 ArrayPool，则尝试扩容以容纳全部输入
            if (desired > available && _pool is not null)
            {
                EnsureCapacity(Count + desired);
                available = Capacity - Count;
            }

            int resultLength = desired;
            if (desired > available)
            {
                // 无法完全容纳，按可用空间截断（没有池时会进入此分支）
                values = values.Slice(0, available);
                resultLength = available;
            }

            int length = values.Length;
            if (length == 0)
                return 0;

            // 第一段复制：从 _tail 到数组末尾
            int part1Length = Math.Min(length, Capacity - _tail);
            values.Slice(0, part1Length).CopyTo(buffer.AsSpan(_tail, part1Length));

            // 如果数据环绕，则进行第二段复制
            if (part1Length < length)
            {
                int part2Length = length - part1Length;
                values.Slice(part1Length, part2Length).CopyTo(buffer.AsSpan(0, part2Length));
            }

            _tail = (_tail + length) % Capacity;
            Count += length;
            return resultLength;
        }

        /// <summary>
        /// 从队列的开头移除一个或多个元素，并将其复制到目标内存区域。
        /// </summary>
        /// <param name="destination">用于接收从队列中移除的元素的内存区域。</param>
        /// <returns>成功从队列中移除并复制到 <paramref name="destination"/> 的元素数量。</returns>
        public int Dequeue(Span<T> destination)
        {
            ThrowIfDisposed();
            if (destination.IsEmpty)
                return 0;

            int resultLength = destination.Length;
            if (destination.Length > Count)
            {
                destination = destination.Slice(0, Count);
                resultLength = Count;
            }

            int length = destination.Length;
            if (length == 0)
                return 0;

            // 第一段复制：从 _head 到数组末尾
            int part1Length = Math.Min(length, Capacity - _head);
            buffer.AsSpan(_head, part1Length).CopyTo(destination.Slice(0, part1Length));

            // 如果数据环绕，则进行第二段复制
            if (part1Length < length)
            {
                int part2Length = length - part1Length;
                buffer.AsSpan(0, part2Length).CopyTo(destination.Slice(part1Length, part2Length));
            }

            _head = (_head + length) % Capacity;
            Count -= length;
            return resultLength;
        }

        /// <summary>
        /// 从队列中移除所有对象并清空底层缓冲区。
        /// </summary>
        public void Clear()
        {
            _head = 0;
            _tail = 0;
            Count = 0;
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 确保底层缓冲区至少具有 <paramref name="min"/> 容量。
        /// 仅在构造时提供了 <see cref="ArrayPool{T}"/> 时才会实际扩充；否则不做任何操作。
        /// </summary>
        /// <param name="min">所需的最小容量（元素数量）。</param>
        private void EnsureCapacity(int min)
        {
            if (_pool is null)
                return;

            if (Capacity >= min)
                return;

            int newCapacity = Math.Max(Capacity == 0 ? 4 : Capacity * 2, min);
            var newBuffer = _pool.Rent(newCapacity);

            if (Count > 0)
            {
                int first = Math.Min(Count, Capacity - _head);
                buffer.AsSpan(_head, first).CopyTo(newBuffer.AsSpan(0, first));

                if (first < Count)
                {
                    buffer.AsSpan(0, Count - first).CopyTo(newBuffer.AsSpan(first, Count - first));
                }
            }

            var old = buffer;
            buffer = newBuffer;
            _head = 0;
            _tail = Count;

            // 仅在老数组不是 Array.Empty 并且 pool 非空时归还
            if (!object.ReferenceEquals(old, Array.Empty<T>()))
            {
                try
                {
                    _pool.Return(old, clearArray: true);
                }
                catch
                {
                    // 为防止在归还时抛出异常影响逻辑，吞掉异常。
                    // 一般情况下只要旧数组是通过 Rent 获取的，Return 不应抛出。
                }
            }
        }

        /// <summary>
        /// 释放托管资源：将租用的数组归还给 <see cref="ArrayPool{T}"/>（如果存在）。
        /// </summary>
        protected override void DisposeManagedResources()
        {
            if (_pool is not null && !object.ReferenceEquals(buffer, Array.Empty<T>()))
            {
                _pool.Return(buffer, clearArray: true);
            }
        }
    }
}