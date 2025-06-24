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

        public LinkOperateData(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, Socket? socket)
        {
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
            Socket = socket ?? new Socket(AddressFamily, SocketType, ProtocolType);
        }
    }
}
