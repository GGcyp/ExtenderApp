using ExtenderApp.Buffer.Sequence;

namespace ExtenderApp.Buffer
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