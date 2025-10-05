namespace ExtenderApp.Data
{
    /// <summary>
    /// 文件空间分配策略，控制扩容/预分配的方式（用于大文件写入、内存映射等场景）。
    /// </summary>
    /// <remarks>
    /// - PreallocateAligned：按对齐的区块实际预分配磁盘空间，扩容时写零或等价操作，空间立即占用，吞吐更稳定。<br/>
    /// - Sparse：标记为稀疏文件，未写入区域不占磁盘，扩容极快但真实占用在写入时发生，可能于写时因磁盘不足失败。<br/>
    /// - 跨平台提示：Windows/NTFS 原生支持稀疏；Linux/macOS 若需打洞需平台特定 API（否则表现近似普通 SetLength）。
    /// </remarks>
    public enum AllocationStrategy
    {
        /// <summary>
        /// 默认分配，按照系统默认行为进行扩容，可能不预分配空间，性能不可预期。
        /// </summary>
        None,

        /// <summary>
        /// 按分配粒度（如 64KB、1MB 等）对齐进行预分配，实际占用磁盘；性能可预期，适合顺序/大块写入（默认）。
        /// </summary>
        PreallocateAligned,

        /// <summary>
        /// 稀疏文件：快速扩容，未写入区域为“空洞”不占空间；适合超大文件与随机远端写入。
        /// 需文件系统支持（Windows NTFS 佳）；写入时才实际占用磁盘。
        /// </summary>
        Sparse
    }
}
