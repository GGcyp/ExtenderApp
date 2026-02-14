using System.Buffers;

namespace ExtenderApp.Buffer
{
    public partial class SequenceBuffer<T>
    {
        public static SequenceBuffer<T> GetBuffer()
            => DefaultSequenceBufferProvider<T>.Shared.GetBuffer();

        public static SequenceBuffer<T> GetBuffer(ReadOnlySequence<T> memories)
            => DefaultSequenceBufferProvider<T>.Default.GetBuffer(memories);
    }
}