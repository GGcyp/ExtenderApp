using System.Buffers;

namespace ExtenderApp.Data
{
    public struct PooledFrameList
    {
        private Frame[]? _array;
        private int _count;

        public int Count => _count;
        public bool IsCreated => _array is not null;

        public Frame this[int index]
        {
            get => _array![index];
            set => _array![index] = value;
        }


        public ReadOnlySpan<Frame> AsSpan()
            => _array is null ? ReadOnlySpan<Frame>.Empty : _array.AsSpan(0, _count);

        public void Add(Frame frame)
        {
            EnsureCapacity(_count + 1);
            _array![_count++] = frame;
        }

        public void Clear()
        {
            // 仅重置计数；避免清零以减少成本（需要彻底清理可改为 Array.Clear）
            _count = 0;
        }

        public void Dispose()
        {
            if (_array is not null)
            {
                ArrayPool<Frame>.Shared.Return(_array, clearArray: true);
                _array = null;
                _count = 0;
            }
        }

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