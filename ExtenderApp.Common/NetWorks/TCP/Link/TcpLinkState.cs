
namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// TCP连接状态枚举
    /// </summary>
    public enum TcpLinkState : byte
    {
        /// <summary>
        /// 已连接
        /// </summary>
        Started,
        /// <summary>
        /// 连接已结束
        /// </summary>
        Ended,

        /// <summary>
        /// 传输文件
        /// </summary>
        File,
        /// <summary>
        /// 传输分块文件
        /// </summary>
        BlockFile,
        /// <summary>
        /// 传输消息
        /// </summary>
        Message,
    }
}
