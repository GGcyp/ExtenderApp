using ExtenderApp.Data.Buffer.MemoryBlocks;

namespace ExtenderApp.Data.Buffer
{
    public partial class MemoryBlock<T>
    {
        public static MemoryBlock<T> GetBuffer(int initialCapacity = 32)
            => MemoryBlockProvider<T>.Shared.GetBuffer(initialCapacity);

        public static MemoryBlock<T> GetBuffer(T[] array)
            => FixedArrayBlockProvider<T>.Default.GetBuffer(array);

        public static MemoryBlock<T> GetBuffer(T[] array, int start, int length)
            => FixedArrayBlockProvider<T>.Default.GetBuffer(array, start, length);

        public static MemoryBlock<T> GetBuffer(ArraySegment<T> segment)
            => FixedArrayBlockProvider<T>.Default.GetBuffer(segment);
    }
}