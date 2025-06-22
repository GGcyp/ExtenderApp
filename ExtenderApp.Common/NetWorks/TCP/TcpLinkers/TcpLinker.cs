using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;

namespace ExtenderApp.Common
{
    public class TcpLinker : Linker, ITcpLinker
    {
        private const int DEFALUT_DATA_LENGTH = 4 * 1024;

        protected override int PacketLength => DEFALUT_DATA_LENGTH;

        protected override LinkOperateData CreateLinkOperateData()
        {
            return new LinkOperateData(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
