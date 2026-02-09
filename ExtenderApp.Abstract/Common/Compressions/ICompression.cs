using System.Buffers;
using ExtenderApp.Contracts;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 提供数据压缩与解压的抽象接口。
    /// </summary>
    public interface ICompression
    {
        /// <summary>
        /// 尝试将输入数据压缩为新的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="input">要压缩的输入数据（只读字节片段）。</param>
        /// <param name="block">输出参数：压缩后的数据块。仅当方法返回 <c>true</c> 时该值有效；调用方负责在不再使用时释放或处置该块（若类型需要处置）。</param>
        /// <returns>
        /// 如果成功完成压缩并通过 <paramref name="block"/> 返回压缩结果则返回 <c>true</c>；否则返回 <c>false</c>（例如数据不可识别、压缩不可用或发生可恢复错误）。
        /// </returns>
        /// <remarks>
        /// - 实现应尽量避免在常见可恢复情况下抛出异常，而是通过返回 <c>false</c> 表示失败；严重或不可恢复错误可抛出异常。
        /// - 当数据较小时或不适合压缩时，实现可选择将原始数据以副本形式返回（仍返回 <c>true</c>）。
        /// - 返回的 <see cref="ByteBlock"/> 的生命周期由调用方管理。
        /// </remarks>
        bool TryCompress(ReadOnlySpan<byte> input, out ByteBlock block);

        /// <summary>
        /// 尝试将输入只读内存压缩为新的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="input">要压缩的输入数据（只读内存）。</param>
        /// <param name="block">输出参数：压缩后的数据块。仅当方法返回 <c>true</c> 时该值有效；调用方负责在不再使用时释放或处置该块（若类型需要处置）。</param>
        /// <returns>见 <see cref="TryCompress(ReadOnlySpan{byte}, out ByteBlock)"/>。</returns>
        bool TryCompress(ReadOnlyMemory<byte> input, out ByteBlock block);

        /// <summary>
        /// 尝试将输入顺序缓冲（可能由多段组成）的可读区按指定压缩类型压缩为新的 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="input">要压缩的输入序列（读取其可读区）。</param>
        /// <param name="buffer">输出参数：压缩结果缓冲。仅当方法返回 <c>true</c> 时该值有效；调用方负责在不再使用时释放或处置该缓冲。</param>
        /// <param name="compression">要使用的压缩类型。实现应支持枚举中常见的压缩策略并在文档中说明对不同类型的行为。</param>
        /// <returns>
        /// 若识别输入并成功完成压缩则返回 <c>true</c>，并通过 <paramref name="buffer"/> 返回压缩结果；否则返回 <c>false</c>。
        /// </returns>
        /// <remarks>
        /// - 实现应正确处理多段 <see cref="ReadOnlySequence{T}"/>，并尽量避免不必要的内存拷贝；对于大型或多段输入，可选择分块压缩以降低峰值内存占用。
        /// - 返回的 <see cref="ByteBuffer"/> 的生命周期由调用方负责管理。
        /// </remarks>
        bool TryCompress(ReadOnlySequence<byte> input, out ByteBuffer buffer, CompressionType compression = CompressionType.BlockArray);

        /// <summary>
        /// 尝试将压缩数据解压为新的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="input">输入：压缩数据（只读字节片段）。</param>
        /// <param name="block">输出：解压后的数据块，仅当返回 <c>true</c> 时有效；调用方负责在不再使用时释放或处置该块。</param>
        /// <returns>
        /// 如果识别并成功解压压缩数据则返回 <c>true</c>，否则返回 <c>false</c>。
        /// </returns>
        /// <remarks>
        /// - 方法应能识别实现所支持的压缩帧/格式（例如 LZ4 单块或数组帧）并据此解压；对无法识别或完整性校验失败的输入应返回 <c>false</c>（除非遇到不可恢复的内部错误）。
        /// - 解压实现应在可能的情况下避免额外拷贝并合理预分配缓冲以减少 GC 与分配开销。
        /// </remarks>
        bool TryDecompress(ReadOnlySpan<byte> input, out ByteBlock block);

        /// <summary>
        /// 尝试将压缩的只读内存解压为新的 <see cref="ByteBlock"/>。
        /// </summary>
        /// <param name="input">输入：压缩数据（只读内存）。</param>
        /// <param name="block">输出：解压后的数据块，仅当返回 <c>true</c> 时有效；调用方负责在不再使用时释放或处置该块。</param>
        /// <returns>见 <see cref="TryDecompress(ReadOnlySpan{byte}, out ByteBlock)"/>。</returns>
        bool TryDecompress(ReadOnlyMemory<byte> input, out ByteBlock block);

        /// <summary>
        /// 尝试将压缩顺序缓冲的可读区按指定压缩类型解压到新的 <see cref="ByteBuffer"/>。
        /// </summary>
        /// <param name="input">输入：压缩的顺序缓冲（读取其可读区）。</param>
        /// <param name="buffer">输出：解压结果缓冲，仅当返回 <c>true</c> 时有效；调用方负责在不再使用时释放或处置该缓冲。</param>
        /// <returns>
        /// 如果识别并成功解压压缩数据则返回 <c>true</c>，否则返回 <c>false</c>。
        /// </returns>
        /// <remarks>
        /// - 实现应正确处理多段输入并尽量避免中间拷贝；若输入使用了分块协议（如 BlockArray），实现应能逐段解压并将结果拼接到输出缓冲。
        /// - 返回的 <see cref="ByteBuffer"/> 生命周期由调用方管理，务必在使用后调用 <c>Dispose</c>（若类型支持）。
        /// </remarks>
        bool TryDecompress(ReadOnlySequence<byte> input, out ByteBuffer buffer);
    }
}