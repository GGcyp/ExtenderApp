using System.Buffers;
using ExtenderApp.Buffer;

namespace ExtenderApp.Abstract
{
    public static class CompressionExtenssions
    {
        #region TryCompress

        /// <summary>
        /// 尝试将调用方提供的输入缓冲中的已提交压缩数据解压为输出缓冲。
        /// </summary>
        /// <param name="input">输入缓冲，包含压缩数据（读取区将被用于解压）。</param>
        /// <param name="output">输出参数：若返回 <c>true</c> 则包含解压后的缓冲；调用方负责释放该缓冲。</param>
        /// <returns>若解压成功则返回 <c>true</c> 并通过 <paramref name="output"/> 返回结果；否则返回 <c>false</c>。</returns>
        public static bool TryCompress(this ICompression compression, ReadOnlySequence<byte> input, out AbstractBuffer<byte> output)
        {
            var buffer = SequenceBuffer<byte>.GetBuffer(input);
            var result = compression.TryCompress(buffer, out output);
            buffer.TryRelease();
            return result;
        }

        /// <summary>
        /// 尝试将调用方提供的输入缓冲中的数据压缩为输出缓冲。
        /// </summary>
        /// <param name="compression">压缩服务实例。</param>
        /// <param name="input">输入缓冲，包含待压缩的数据。</param>
        /// <param name="output">输出参数：若返回 <c>true</c> 则包含压缩后的缓冲；调用方负责释放该缓冲。</param>
        /// <returns>若压缩成功则返回 <c>true</c> 并通过 <paramref name="output"/> 返回结果；否则返回 <c>false</c>。</returns>
        public static bool TryCompress(this ICompression compression, ReadOnlyMemory<byte> input, out AbstractBuffer<byte> output)
        {
            return compression.TryCompress(input.Span, out output);
        }

        #endregion TryCompress

        #region TryDecompress

        /// <summary>
        /// 尝试将调用方提供的输入缓冲中的已提交压缩数据解压为输出缓冲。
        /// </summary>
        /// <param name="compression">压缩服务实例。</param>
        /// <param name="input">输入缓冲，包含压缩数据（读取区将被用于解压）。</param>
        /// <param name="output">输出参数：若返回 <c>true</c> 则包含解压后的缓冲；调用方负责释放该缓冲。</param>
        /// <returns>若解压成功则返回 <c>true</c> 并通过 <paramref name="output"/> 返回结果；否则返回 <c>false</c>。</returns>
        public static bool TryDecompress(this ICompression compression, ReadOnlySequence<byte> input, out AbstractBuffer<byte> output)
        {
            var buffer = SequenceBuffer<byte>.GetBuffer(input);
            var result = compression.TryDecompress(buffer, out output);
            buffer.TryRelease();
            return result;
        }

        /// <summary>
        /// 尝试将调用方提供的输入缓冲中的数据解压为输出缓冲。
        /// </summary>
        /// <param name="compression">压缩服务实例。</param>
        /// <param name="input">输入缓冲，包含待解压的数据。</param>
        /// <param name="output">输出参数：若返回 <c>true</c> 则包含解压后的缓冲；调用方负责释放该缓冲。</param>
        /// <returns>若解压成功则返回 <c>true</c> 并通过 <paramref name="output"/> 返回结果；否则返回 <c>false</c>。</returns>
        public static bool TryDecompress(this ICompression compression, ReadOnlyMemory<byte> input, out AbstractBuffer<byte> output)
        {
            return compression.TryDecompress(input.Span, out output);
        }

        #endregion TryDecompress
    }
}