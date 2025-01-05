using System.Buffers;

namespace ExtenderApp.Data.File
{
    /// <summary>
    /// 序列池类，用于管理Sequence<byte>对象的重用。
    /// </summary>
    public class SequencePool
    {
        /// <summary>
        /// 序列的最小跨度长度。
        /// </summary>
        private const int MinimumSpanLength = 32 * 1024;

        /// <summary>
        /// 序列池的最大大小。
        /// </summary>
        private readonly int _maxSize;

        /// <summary>
        /// 序列池实例。
        /// </summary>
        private readonly Stack<Sequence<byte>> _pool;

        /// <summary>
        /// 数组池或内存池实例。
        /// </summary>
        private readonly object _arrayPoolOrMemoryPool;

        /// <summary>
        /// 使用默认的最大大小和数组池初始化SequencePool实例。
        /// </summary>
        public SequencePool()
            : this(Environment.ProcessorCount * 2, ArrayPool<byte>.Create(80 * 1024, 100))
        {
        }

        /// <summary>
        /// 使用指定的最大大小初始化SequencePool实例，并使用默认的数组池。
        /// </summary>
        /// <param name="maxSize">序列池的最大大小。</param>
        public SequencePool(int maxSize)
            : this(maxSize, ArrayPool<byte>.Create(80 * 1024, 100))
        {
        }

        /// <summary>
        /// 使用指定的最大大小和数组池初始化SequencePool实例。
        /// </summary>
        /// <param name="maxSize">序列池的最大大小。</param>
        /// <param name="arrayPool">数组池实例。</param>
        public SequencePool(int maxSize, ArrayPool<byte> arrayPool)
        {
            _maxSize = maxSize;
            _arrayPoolOrMemoryPool = arrayPool;
            _pool = new();
        }

        /// <summary>
        /// 使用指定的最大大小和内存池初始化SequencePool实例。
        /// </summary>
        /// <param name="maxSize">序列池的最大大小。</param>
        /// <param name="memoryPool">内存池实例。</param>
        public SequencePool(int maxSize, MemoryPool<byte> memoryPool)
        {
            _maxSize = maxSize;
            _arrayPoolOrMemoryPool = memoryPool;
            _pool = new();
        }

        /// <summary>
        /// 租用一个Sequence<byte>对象。
        /// </summary>
        /// <returns>包含租用Sequence<byte>对象的Rental实例。</returns>
        public Rental Rent()
        {
            if (_pool.Count > 0)
            {
                return new Rental(this, _pool.Pop());
            }

            var sequence = _arrayPoolOrMemoryPool is ArrayPool<byte> arrayPool
                ? new Sequence<byte>(arrayPool)
                : new Sequence<byte>((MemoryPool<byte>)_arrayPoolOrMemoryPool);

            sequence.MinimumSpanLength = MinimumSpanLength;

            return new Rental(this, sequence);
        }

        /// <summary>
        /// 释放指定的Sequence<byte>对象。
        /// </summary>
        /// <param name="value">要释放的Sequence<byte>对象。</param>
        private void Return(Sequence<byte> value)
        {
            value.Reset();
            value.MinimumSpanLength = MinimumSpanLength;
            _pool.Push(value);
        }


        /// <summary>
        /// 表示一个用于管理租用的序列的结构体。
        /// </summary>
        public struct Rental : IDisposable
        {
            /// <summary>
            /// 序列池所有者。
            /// </summary>
            private readonly SequencePool _owner;

            /// <summary>
            /// 获取租用的序列。
            /// </summary>
            public Sequence<byte> Value { get; }

            /// <summary>
            /// 初始化 <see cref="Rental"/> 结构体的新实例。
            /// </summary>
            /// <param name="owner">序列池所有者。</param>
            /// <param name="value">要租用的序列。</param>
            internal Rental(SequencePool owner, Sequence<byte> value)
            {
                _owner = owner;
                Value = value;
            }

            /// <summary>
            /// 释放租用的序列。
            /// </summary>
            public void Dispose()
            {
                _owner?.Return(Value);
            }
        }
    }
}
