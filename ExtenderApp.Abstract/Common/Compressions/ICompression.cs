using System.Buffers;
using ExtenderApp.Buffer;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供数据压缩与解压的抽象接口。
    /// </summary>
    /// <remarks>
    /// 实现应尽量避免不必要的数据拷贝并在可能时直接将结果写入目标缓冲。
    /// 所有方法均应在失败时尽量返回 <c>false</c>，而非抛出异常，除非遇到严重错误。
    /// </remarks>
    public interface ICompression
    {
        /// <summary>
        /// 将调用方提供的输入字节跨度压缩为一个新的缓冲对象输出。
        /// </summary>
        /// <param name="span">要压缩的输入数据（只读跨度）。</param>
        /// <param name="output">当方法返回 <c>true</c> 时，包含压缩结果的输出缓冲；调用方负责释放该缓冲的生命周期。</param>
        /// <returns>若压缩成功则返回 <c>true</c> 并通过 <paramref name="output"/> 返回结果；否则返回 <c>false</c>。</returns>
        /// <remarks>
        /// 实现应尽量避免分配，优先在目标缓冲中直接写入压缩数据以减少拷贝开销。
        /// </remarks>
        bool TryCompress(ReadOnlySpan<byte> span, out AbstractBuffer<byte> output);

        /// <summary>
        /// 将调用方提供的输入缓冲中的已提交数据压缩为输出缓冲。
        /// </summary>
        /// <param name="input">输入缓冲，包含待压缩的数据（读取区将被用于压缩）。</param>
        /// <param name="output">输出参数：若返回 <c>true</c> 则包含压缩后的缓冲；调用方负责释放该缓冲。</param>
        /// <param name="compression">指定使用的压缩类型或模式，默认为 <see cref="CompressionType.Block"/>。</param>
        /// <returns>若压缩成功则返回 <c>true</c> 并通过 <paramref name="output"/> 返回结果；否则返回 <c>false</c>。</returns>
        bool TryCompress(AbstractBuffer<byte> input, out AbstractBuffer<byte> output, CompressionType compression = CompressionType.Block);

        /// <summary>
        /// 将调用方提供的输入字节跨度解压为一个新的缓冲对象输出。
        /// </summary>
        /// <param name="span">包含压缩数据的输入字节跨度。</param>
        /// <param name="output">当方法返回 <c>true</c> 时，包含解压结果的输出缓冲；调用方负责释放该缓冲的生命周期。</param>
        /// <returns>若解压成功则返回 <c>true</c> 并通过 <paramref name="output"/> 返回结果；否则返回 <c>false</c>。</returns>
        bool TryDecompress(ReadOnlySpan<byte> span, out AbstractBuffer<byte> output);

        /// <summary>
        /// 将调用方提供的输入缓冲中的已提交压缩数据解压为输出缓冲。
        /// </summary>
        /// <param name="input">输入缓冲，包含压缩数据（读取区将被用于解压）。</param>
        /// <param name="output">输出参数：若返回 <c>true</c> 则包含解压后的缓冲；调用方负责释放该缓冲。</param>
        /// <returns>若解压成功则返回 <c>true</c> 并通过 <paramref name="output"/> 返回结果；否则返回 <c>false</c>。</returns>
        bool TryDecompress(AbstractBuffer<byte> input, out AbstractBuffer<byte> output);
    }
}