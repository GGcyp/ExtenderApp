using System.Buffers;

namespace ExtenderApp.Buffer
{
    public static class MemoryBlockExtensions
    {
        public static MemoryBlock<T> ToMemoryBlock<T>(this AbstractBuffer<T> buffer)
        {
            MemoryBlock<T> memoryBlock = MemoryBlock<T>.GetBuffer((int)buffer.Committed);

            if (buffer is MemoryBlock<T> mb)
            {
                memoryBlock.Write(mb.CommittedSpan);
                return memoryBlock;
            }

            ReadOnlySequence<T> memories = buffer.CommittedSequence;
            SequencePosition position = memories.Start;
            while (memories.TryGet(ref position, out var memory))
            {
                memoryBlock.Write(memory);
            }
            return memoryBlock;
        }

        public static void Write<T>(this AbstractBuffer<T> buffer, MemoryBlock<T> memoryBlock)
        {
            buffer.Write(memoryBlock.CommittedSpan);
        }
    }
}