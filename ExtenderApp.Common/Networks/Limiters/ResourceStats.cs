

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 资源使用统计类
    /// </summary>
    public struct ResourceStats
    {
        /// <summary>
        /// 获取或设置当前内存使用量（以字节为单位）
        /// </summary>
        public long CurrentMemoryUsage { get; set; }

        /// <summary>
        /// 获取或设置每秒发送的字节数
        /// </summary>
        public long SendBytesPerSecond { get; set; }

        /// <summary>
        /// 获取或设置每秒接收的字节数
        /// </summary>
        public long ReceiveBytesPerSecond { get; set; }

        /// <summary>
        /// 获取或设置时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"内存使用情况: {Utility.BytesToMegabytes(CurrentMemoryUsage)} M, " +
                   $"发送情况: {Utility.BytesToMegabytes(SendBytesPerSecond)} M/s, " +
                   $"接收情况: {Utility.BytesToMegabytes(ReceiveBytesPerSecond)} M/s, " +
                   $"时间戳: {Timestamp}";
        }
    }
}
