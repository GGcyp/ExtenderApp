

using System.Net.Sockets;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// TCP链接数据类
    /// </summary>
    public class TcpLinkerData : LinkOperateData
    {
        public TcpLinkerData(Socket? socket) : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, socket)
        {
        }
    }
}
