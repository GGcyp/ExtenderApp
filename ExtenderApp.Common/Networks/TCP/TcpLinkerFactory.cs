using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// TCP链接器工厂类，用于创建TCP链接器实例。
    /// </summary>
    internal class TcpLinkerFactory : LinkerFactory<ITcpLinker>
    {
        public override SocketType LinkerSocketType => SocketType.Stream;

        public override ProtocolType LinkerProtocolType => ProtocolType.Tcp;

        protected override ITcpLinker CreateLinkerInternal(Socket socket)
        {
            return new TcpLinker(socket);
        }
    }
}
