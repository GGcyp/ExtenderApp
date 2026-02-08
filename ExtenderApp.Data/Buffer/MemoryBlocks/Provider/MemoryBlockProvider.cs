namespace ExtenderApp.Data.Buffer.MemoryBlocks
{
    /// <summary>
    /// 为 <see cref="MemoryBlock{T}"/> 提供获取与释放的抽象基类。
    /// </summary>
    /// <typeparam name="T">序列段中元素的类型。</typeparam>
    public abstract class MemoryBlockProvider<T> : BufferProvider<T, MemoryBlock<T>>
    {
        public static MemoryBlockProvider<T> Shared = ArrayPoolBlockProvider<T>.Default;
    }
}