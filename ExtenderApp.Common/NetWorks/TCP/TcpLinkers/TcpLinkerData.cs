

using System.Net.Sockets;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// TCP链接数据类
    /// </summary>
    public class TcpLinkerData : LinkerData
    {
        public TcpLinkerData(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
        }
    }
}
