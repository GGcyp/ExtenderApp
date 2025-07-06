using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;

namespace ExtenderApp.Common
{
    /// <summary>
    /// TcpLinker 类表示一个基于 TCP 协议的链接器。
    /// </summary>
    internal class TcpLinker : Linker, ITcpLinker
    {
        /// <summary>
        /// 默认的数据包长度。
        /// </summary>
        private const int DEFALUT_DATA_LENGTH = 4 * 1024;

        /// <summary>
        /// 获取数据包长度。
        /// </summary>
        /// <returns>返回数据包长度。</returns>
        protected override int PacketLength => DEFALUT_DATA_LENGTH;

        public TcpLinker(AddressFamily addressFamily) : base(addressFamily)
        {

        }

        public TcpLinker(Socket socket) : base(socket)
        {

        }

        protected override LinkOperateData CreateLinkOperateData(Socket socket)
        {
            if (socket.ProtocolType != ProtocolType.Tcp)
                throw new Exception("生成套字节错误");

            return new LinkOperateData(socket);
        }

        protected override LinkOperateData CreateLinkOperateData(AddressFamily addressFamily)
        {
            return new LinkOperateData(addressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
