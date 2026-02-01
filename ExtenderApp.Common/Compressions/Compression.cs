using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Compressions
{
    public abstract class Compression : DisposableObject, ICompression
    {
        public abstract bool TryCompress(ReadOnlySpan<byte> input, out ByteBlock block);

        public abstract bool TryCompress(ReadOnlySequence<byte> input, out ByteBuffer buffer, CompressionType compression = CompressionType.BlockArray);

        public abstract bool TryCompress(ReadOnlyMemory<byte> input, out ByteBlock block);

        public abstract bool TryDecompress(ReadOnlySpan<byte> input, out ByteBlock block);

        public abstract bool TryDecompress(ReadOnlySequence<byte> input, out ByteBuffer buffer);

        public abstract bool TryDecompress(ReadOnlyMemory<byte> input, out ByteBlock block);
    }
}