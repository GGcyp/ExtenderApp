namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示一个泛型先进先出 (FIFO) 的循环队列（也称为环形缓冲区）。
    /// </summary>
    /// <typeparam name="T">指定队列中元素的类型。</typeparam>
    public class CircularQueue<T>
    {
        /// <summary>
        /// 获取一个表示容量为零的空 <see cref="CircularQueue{T}"/> 的实例。
        /// </summary>
        public static CircularQueue<T> Empty = new CircularQueue<T>(0);

        private readonly T[] _buffer;

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
        public int Capacity => _buffer.Length;

        /// <summary>
        /// 初始化 <see cref="CircularQueue{T}"/> 类的新实例，该实例为空且具有指定的初始容量。
        /// </summary>
        /// <param name="capacity">队列可以存储的初始元素数。</param>
        /// <exception cref="ArgumentOutOfRangeException">当 <paramref name="capacity"/> 为负数时引发。</exception>
        public CircularQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be non-negative.");
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            Count = 0;
        }

        /// <summary>
        /// 将一个或多个元素添加到队列的末尾。如果可用空间不足，则只添加适合的元素。
        /// </summary>
        /// <param name="values">要添加到队列的元素所在的只读内存区域。</param>
        /// <returns>成功添加到队列中的元素数量。</returns>
        public int Enqueue(ReadOnlySpan<T> values)
        {
            if (values.IsEmpty)
                return 0;

            int resultLength = values.Length;
            int available = Capacity - Count;
            if (values.Length > available)
            {
                values = values.Slice(0, available);
                resultLength = available;
            }

            int length = values.Length;
            // 第一段复制：从 _tail 到数组末尾
            int part1Length = Math.Min(length, Capacity - _tail);
            values.Slice(0, part1Length).CopyTo(_buffer.AsSpan(_tail, part1Length));

            // 如果数据环绕，则进行第二段复制
            if (part1Length < length)
            {
                int part2Length = length - part1Length;
                values.Slice(part1Length, part2Length).CopyTo(_buffer.AsSpan(0, part2Length));
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
            if (destination.IsEmpty)
                return 0;

            int resultLength = destination.Length;
            if (destination.Length > Count)
            {
                destination = destination.Slice(0, Count);
                resultLength = Count;
            }

            int length = destination.Length;
            // 第一段复制：从 _head 到数组末尾
            int part1Length = Math.Min(length, Capacity - _head);
            _buffer.AsSpan(_head, part1Length).CopyTo(destination.Slice(0, part1Length));

            // 如果数据环绕，则进行第二段复制
            if (part1Length < length)
            {
                int part2Length = length - part1Length;
                _buffer.AsSpan(0, part2Length).CopyTo(destination.Slice(part1Length, part2Length));
            }

            _head = (_head + length) % Capacity;
            Count -= length;
            return resultLength;
        }

        /// <summary>
        /// 从队列中移除所有对象。
        /// </summary>
        public void Clear()
        {
            _head = 0;
            _tail = 0;
            Count = 0;
        }
    }
}