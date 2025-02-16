using System.Net;


namespace ExtenderApp.Common
{
    /// <summary>
    /// 表示TCP连接信息的结构体。
    /// </summary>
    public struct TcpLinkInfo : IEquatable<TcpLinkInfo>
    {
        public const int DefaultBufferSize = 10240;

        public static TcpLinkInfo Empty = new TcpLinkInfo();

        /// <summary>
        /// 获取或设置远程IP地址和端口。
        /// </summary>
        public IPEndPoint IP { get; set; }

        /// <summary>
        /// 获取或设置接收超时时间（毫秒）。
        /// </summary>
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// 获取或设置接收缓冲区大小（字节）。
        /// </summary>
        public int ReceiveBufferSize { get; set; }

        /// <summary>
        /// 获取或设置发送缓冲区大小（字节）。
        /// </summary>
        public int SendBufferSize { get; set; }

        /// <summary>
        /// 获取或设置发送超时时间（毫秒）。
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// 使用指定的远程IP地址和端口初始化<see cref="TcpLinkInfo"/>结构的新实例。
        /// </summary>
        /// <param name="iP">远程IP地址和端口。</param>
        public TcpLinkInfo(IPEndPoint iP, int receiveBufferSize = DefaultBufferSize)
        {
            IP = iP;
            ReceiveBufferSize = receiveBufferSize;
            ReceiveTimeout = -1;
            ReceiveBufferSize = -1;
            SendBufferSize = -1;
        }

        public bool Equals(TcpLinkInfo other)
        {
            return IP.Equals(other.IP);
        }

        public static bool operator ==(TcpLinkInfo left, TcpLinkInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TcpLinkInfo left, TcpLinkInfo right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is TcpLinkInfo && Equals((TcpLinkInfo)obj);
        }

        public override int GetHashCode()
        {
            return IP.GetHashCode();
        }
    }
}
