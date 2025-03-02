using System.Net.Sockets;
using ExtenderApp.Common.ConcurrentOperates;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Common.NetWorks
{
    public class LinkOperateData : ConcurrentOperateData
    {
        public AddressFamily AddressFamily { get; set; }
        public SocketType SocketType { get; set; }
        public ProtocolType ProtocolType { get; set; }

        public Socket? Socket { get; set; }

        public LinkOperateData(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            AddressFamily = addressFamily;
            SocketType = socketType;
            ProtocolType = protocolType;
        }

        public LinkOperateData(Socket socket)
        {
            Socket = socket;
            AddressFamily = socket.AddressFamily;
            ProtocolType = socket.ProtocolType;
            SocketType = socket.SocketType;
        }

        public override bool TryReset()
        {
            Socket!.Close();
            Socket.Dispose();
            Socket = null;
            return base.TryReset();
        }
    }
}
