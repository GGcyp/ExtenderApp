using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks;
using ExtenderApp.Data;

namespace ExtenderApp.Common
{
    /// <summary>
    /// TcpLinker 类表示一个基于 TCP 协议的链接器。
    /// </summary>
    internal class TcpLinker : Linker, ITcpLinker
    {
        protected override void ExecuteSend(ref ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
