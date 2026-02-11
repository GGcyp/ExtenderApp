using System.Buffers;

namespace ExtenderApp.Buffer.Sequence
{
    /// <summary>
    /// 空序列缓冲段，实现了一个不包含任何数据的缓冲段。
    /// </summary>
    internal class EmptySequenceBufferSegment<T> : SequenceBufferSegment<T>
    {
        protected internal override Memory<T> Memory => Memory<T>.Empty;

        protected internal override long Committed => 0;

        protected internal override int Available => 0;

        protected internal override ReadOnlyMemory<T> CommittedMemory => Memory<T>.Empty;

        public override MemoryHandle Pin(int elementIndex)
        {
            return default;
        }

        public override SequenceBufferSegment<T> Slice(int start, int length)
        {
            return Empty;
        }

        public override void Unpin()
        {
        }

        protected override void AdvanceProtected(int count)
        {
            throw new NotImplementedException();
        }

        protected override Memory<T> GetMemotyProtected(int sizeHint = 0)
        {
            return Memory<T>.Empty;
        }

        protected override Span<T> GetSpanProtected(int sizeHint = 0)
        {
            return Span<T>.Empty;
        }
    }
}