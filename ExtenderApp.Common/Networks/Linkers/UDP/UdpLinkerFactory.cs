using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// Upd链接器工厂类，用于创建UDP链接器实例。
    /// </summary>
    internal class UdpLinkerFactory : LinkerFactory<IUdpLinker>
    {
        public override SocketType SocketType => SocketType.Dgram;
        public override ProtocolType ProtocolType => ProtocolType.Udp;

        protected override IUdpLinker CreateLinkerInternal(Socket socket)
        {
            return new UdpLinker(socket);
        }
    }
}