using System.Buffers;
using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    internal interface ICompression
    {
        void Serialize(ReadOnlySpan<byte> span, out ByteBuffer compressedBuffer, CompressionType compression);

        /// <summary>
        /// 将输入顺序缓冲的可读区按指定压缩类型压缩到新的顺序缓冲。
        /// </summary>
        /// <param name="sequence">输入顺序缓冲（读取其可读区）。</param>
        /// <param name="buffer">输出：压缩结果缓冲。</param>
        /// <param name="compression">压缩类型。</param>
        void Serialize(ReadOnlySequence<byte> sequence, out ByteBuffer buffer, CompressionType compression);
    }
}
