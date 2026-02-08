using ExtenderApp.Data.Buffer.Sequence;

namespace ExtenderApp.Data.Buffer
{
    public static class SequenceBufferExtensions
    {
        public static SequenceBuffer<T> Append<T>(this SequenceBuffer<T> sequenceBuffer, MemoryBlock<T> memoryBlock)
        {
            var segment = SequenceBufferSegmentProvider<T>.Shared.GetSegment(memoryBlock);
            sequenceBuffer.Append(segment);
            return sequenceBuffer;
        }
    }
}