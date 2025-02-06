using System.Net;

namespace ExtenderApp.Common
{
    /// <summary>
    /// TcpLinkOperate 的扩展方法类。
    /// </summary>
    public static class TcpLinkOperateExtensions
    {
        /// <summary>
        /// 通过IP地址和端口号连接到TCP服务器。
        /// </summary>
        /// <param name="operate">TcpLinkOperate 对象。</param>
        /// <param name="ip">目标服务器的IP地址。</param>
        /// <param name="prot">目标服务器的端口号。</param>
        /// <param name="receiveBufferSize">接收缓冲区大小，默认为 TcpLinkInfo.DefaultBufferSize。</param>
        public static void Connect(this TcpLinkOperate operate, string ip, int prot, int receiveBufferSize = TcpLinkInfo.DefaultBufferSize)
        {
            operate.Connect(IPAddress.Parse(ip), prot, receiveBufferSize);
        }

        /// <summary>
        /// 通过IP地址对象和端口号连接到TCP服务器。
        /// </summary>
        /// <param name="operate">TcpLinkOperate 对象。</param>
        /// <param name="ip">目标服务器的IP地址对象。</param>
        /// <param name="prot">目标服务器的端口号。</param>
        /// <param name="receiveBufferSize">接收缓冲区大小，默认为 TcpLinkInfo.DefaultBufferSize。</param>
        public static void Connect(this TcpLinkOperate operate, IPAddress ip, int prot, int receiveBufferSize = TcpLinkInfo.DefaultBufferSize)
        {
            operate.Connect(new IPEndPoint(ip, prot), receiveBufferSize);
        }

        /// <summary>
        /// 通过IP地址端点对象连接到TCP服务器。
        /// </summary>
        /// <param name="operate">TcpLinkOperate 对象。</param>
        /// <param name="ip">目标服务器的IP地址端点对象。</param>
        /// <param name="receiveBufferSize">接收缓冲区大小，默认为 TcpLinkInfo.DefaultBufferSize。</param>
        public static void Connect(this TcpLinkOperate operate, IPEndPoint ip, int receiveBufferSize = TcpLinkInfo.DefaultBufferSize)
        {
            operate.Connect(new TcpLinkInfo(ip, receiveBufferSize));
        }

        /// <summary>
        /// 异步连接TCP服务器。
        /// </summary>
        /// <param name="operate">TCP操作对象。</param>
        /// <param name="ip">服务器IP地址。</param>
        /// <param name="prot">服务器端口。</param>
        /// <param name="receiveBufferSize">接收缓冲区大小，默认为TcpLinkInfo.DefaultBufferSize。</param>
        /// <returns>一个表示异步操作的Task对象。</returns>
        public static async Task ConnectAsync(this TcpLinkOperate operate, string ip, int prot, int receiveBufferSize = TcpLinkInfo.DefaultBufferSize)
        {
            await operate.ConnectAsync(IPAddress.Parse(ip), prot, receiveBufferSize);
        }

        /// <summary>
        /// 异步连接TCP服务器。
        /// </summary>
        /// <param name="operate">TCP操作对象。</param>
        /// <param name="ip">服务器IP地址。</param>
        /// <param name="prot">服务器端口。</param>
        /// <param name="receiveBufferSize">接收缓冲区大小，默认为TcpLinkInfo.DefaultBufferSize。</param>
        /// <returns>一个表示异步操作的Task对象。</returns>
        public static async Task ConnectAsync(this TcpLinkOperate operate, IPAddress ip, int prot, int receiveBufferSize = TcpLinkInfo.DefaultBufferSize)
        {
            await operate.ConnectAsync(new IPEndPoint(ip, prot), receiveBufferSize);
        }

        /// <summary>
        /// 异步连接TCP服务器。
        /// </summary>
        /// <param name="operate">TCP操作对象。</param>
        /// <param name="ip">服务器IP地址。</param>
        /// <param name="receiveBufferSize">接收缓冲区大小，默认为TcpLinkInfo.DefaultBufferSize。</param>
        /// <returns>一个表示异步操作的Task对象。</returns>
        public static async Task ConnectAsync(this TcpLinkOperate operate, IPEndPoint ip, int receiveBufferSize = TcpLinkInfo.DefaultBufferSize)
        {
            await operate.ConnectAsync(new TcpLinkInfo(ip, receiveBufferSize));
        }
    }
}
