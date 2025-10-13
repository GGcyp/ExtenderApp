namespace ExtenderApp.Data
{
    /// <summary>
    /// 定义文件访问的策略，用于指导工厂/提供者选择具体的读写实现。
    /// </summary>
    /// <remarks>
    /// 选择建议：
    /// - FileStream：通用、简单、低内存占用，适合小到中等规模文件的顺序/适度随机访问与异步 IO。
    /// - MemoryMapped：适合超大文件的高频随机访问或跨进程共享场景，但需谨慎管理映射与视图生命周期。
    /// </remarks>
    public enum FileOperateType
    {
        /// <summary>
        /// 使用基于 <see cref="System.IO.FileStream"/> 的传统文件访问方式。
        /// 兼容性最佳，适合顺序读写与常规随机读写，内存占用小，API 简单。
        /// </summary>
        FileStream,

        /// <summary>
        /// 并发文件流访问，基于 <see cref="System.IO.FileStream"/>，但启用更高的并发读写支持。
        /// </summary>
        ConcurrentFileStream,

        /// <summary>
        /// 使用内存映射文件（UnreadMemory-Mapped File）访问。
        /// 提供接近内存的随机访问性能，可跨进程共享；适合大文件或频繁随机读写的场景。
        /// 需确保映射文件与视图正确释放以避免句柄/内存泄漏。
        /// </summary>
        MemoryMapped,
    }
}
