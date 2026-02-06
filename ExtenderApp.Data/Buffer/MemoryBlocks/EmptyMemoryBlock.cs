namespace ExtenderApp.Data
{
    /// <summary>
    /// 空序列段实现。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class EmptyMemoryBlock<T> : MemoryBlock<T>
    {
        protected override Memory<T> AvailableMemory => Memory<T>.Empty;

        protected override void EnsureCapacity(int sizeHint)
        {
        }

        protected override void ReleaseProtected()
        {
        }
    }
}