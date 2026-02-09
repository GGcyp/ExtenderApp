namespace ExtenderApp.Contracts
{
    /// <summary>
    /// 文件访问/映射模式（互斥枚举）。
    /// </summary>
    public enum ExtenderFileAccess : byte
    {
        /// <summary>
        /// 可读可写。
        /// </summary>
        ReadWrite = 0,

        /// <summary>
        /// 只读。
        /// </summary>
        Read = 1,

        /// <summary>
        /// 只写（部分底层 API 可能不支持仅写入的映射）。
        /// </summary>
        Write = 2,

        /// <summary>
        /// 写时复制：写入仅对当前进程可见，不会回写到底层文件。
        /// </summary>
        CopyOnWrite = 4,

        /// <summary>
        /// 可读且可执行。
        /// </summary>
        ReadExecute = 8,

        /// <summary>
        /// 可读可写且可执行。
        /// </summary>
        ReadWriteExecute = 16,
    }
}
