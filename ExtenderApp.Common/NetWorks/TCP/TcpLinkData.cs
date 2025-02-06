

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// TCP链接数据类
    /// </summary>
    internal class TcpLinkData
    {
        /// <summary>
        /// 获取或设置TCP链接状态
        /// </summary>
        public TcpLinkState State { get; private set; }

        /// <summary>
        /// 获取或设置数据
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// 获取容量
        /// </summary>
        public int Capacity => Data.Length;

        /// <summary>
        /// 初始化TcpLinkData实例
        /// </summary>
        /// <param name="bufferSize">缓冲区大小，默认为1024</param>
        public TcpLinkData(int bufferSize = 1024)
        {
            Data = new byte[bufferSize];
        }
    }
}
