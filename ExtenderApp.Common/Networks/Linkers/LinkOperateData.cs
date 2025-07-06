using System.Net.Sockets;
using ExtenderApp.Common.ConcurrentOperates;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 链接操作数据类，继承自ConcurrentOperateData类
    /// </summary>
    public class LinkOperateData : ConcurrentOperateData
    {
        /// <summary>
        /// 地址族
        /// </summary>
        public AddressFamily AddressFamily { get; set; }

        /// <summary>
        /// 套接字类型
        /// </summary>
        public SocketType SocketType { get; set; }

        /// <summary>
        /// 协议类型
        /// </summary>
        public ProtocolType ProtocolType { get; set; }

        public Socket Socket { get; set; }

        public LinkOperateData(Socket socket)
        {
            AddressFamily = socket.AddressFamily;
            ProtocolType = socket.ProtocolType;
            SocketType = socket.SocketType;
            Socket = socket ?? new Socket(AddressFamily, SocketType, ProtocolType);
        }

        public LinkOperateData(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
            Socket = new Socket(AddressFamily, SocketType, ProtocolType);
        }
    }
}
