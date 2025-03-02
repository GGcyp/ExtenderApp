

using System.Net.Sockets;

namespace ExtenderApp.Common.NetWorks
{
    /// <summary>
    /// TCP链接数据类
    /// </summary>
    public class TcpLinkData : LinkOperateData
    {
        public TcpLinkData(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
        }
    }
}
