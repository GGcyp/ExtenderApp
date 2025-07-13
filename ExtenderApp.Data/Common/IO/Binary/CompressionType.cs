

namespace ExtenderApp.Data
{
    /// <summary>
    /// 压缩类型枚举
    /// </summary>
    public enum CompressionType
    {
        /// <summary>
        /// 无压缩
        /// </summary>
        None,

        /// <summary>
        /// LZ4 块压缩
        /// </summary>
        Lz4Block,

        /// <summary>
        /// LZ4 块数组压缩
        /// </summary>
        Lz4BlockArray,
    }
}
