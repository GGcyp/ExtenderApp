

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 资源限制配置
    /// </summary>
    /// <remarks>
    /// 此类定义了资源使用的限制配置，包括最大总内存使用量、最大发送速率、最大接收速率、速率窗口和统计报告间隔。
    /// </remarks>
    public class ResourceLimitConfig
    {
        /// <summary>
        /// 最大总内存使用量（以字节为单位）
        /// </summary>
        /// <remarks>
        /// 默认为100MB
        /// </remarks>
        public long MaxTotalMemoryBytes { get; set; } = Utility.MegabytesToBytes(100); // 100MB

        /// <summary>
        /// 最大发送速率（以字节/秒为单位）
        /// </summary>
        /// <remarks>
        /// 默认为10MB/s
        /// </remarks>
        public long MaxSendRateBytesPerSecond { get; set; } = Utility.MegabytesToBytes(10); // 10MB/s

        /// <summary>
        /// 最大接收速率（以字节/秒为单位）
        /// </summary>
        /// <remarks>
        /// 默认为10MB/s
        /// </remarks>
        public long MaxReceiveRateBytesPerSecond { get; set; } = Utility.MegabytesToBytes(10); // 10MB/s

        /// <summary>
        /// 速率窗口时间跨度
        /// </summary>
        /// <remarks>
        /// 用于计算发送和接收速率的时间窗口，默认为1秒
        /// </remarks>
        public TimeSpan RateWindow { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 统计报告间隔
        /// </summary>
        /// <remarks>
        /// 发送统计报告的间隔时间，默认为1秒
        /// </remarks>
        public TimeSpan StatsReportInterval { get; set; } = TimeSpan.FromSeconds(1);
    }
}
