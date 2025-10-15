

namespace ExtenderApp.Data
{
    /// <summary>
    /// 压缩类型枚举
    /// </summary>
    /// <remarks>
    /// 选择指引（经验法则）：
    /// - 数据很小或压缩收益低：选 None（库在小于约 64 字节时也会直接透传）。
    /// - 单段（连续内存）中/大体量数据：选 Lz4Block（单帧、开销小、压缩比更好）。
    /// - 多段序列、需要流式/分块处理或降低峰值内存：选 Lz4BlockArray（每段独立压缩，可边解边写，压缩率略低）。
    /// </remarks>
    public enum CompressionType
    {
        /// <summary>
        /// 无压缩。
        /// 适用：数据很小（≈&lt;64B）、已是压缩/高熵数据（如随机数、图片视频等）、实时/CPU 敏感路径、对体积不敏感。
        /// 优点：零 CPU 开销、零协议开销；缺点：体积不变。
        /// </summary>
        None,

        /// <summary>
        /// LZ4 块压缩（单帧）。
        /// 适用：单对象或连续内存的中/大数据（磁盘/网络传输场景），追求较优压缩比与吞吐的平衡。
        /// 协议：Ext(99, compressedLength) + uncompressedLength:int + compressedBytes。
        /// 特点：一次性压缩/解压，协议开销小（头+长度），压缩比通常优于分块方案。
        /// </summary>
        Lz4Block,

        /// <summary>
        /// LZ4 块数组压缩（多帧/分块）。
        /// 适用：输入为多段 ReadOnlySequence&lt;byte&gt;、超大内容需分块、希望边解压边写入或控制峰值内存。
        /// 协议：[Array(n+1), Ext(98, extHeaderSize), (length:int)xN, (bin32+compressed)xN]。
        /// 特点：每段独立压缩，便于流式处理与内存控制；缺点：每段独立字典，压缩比略低且协议开销更高。
        /// </summary>
        Lz4BlockArray,
    }
}
