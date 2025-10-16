using System.Buffers;
using System.Net;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;


namespace ExtenderApp.Common.Networks.UDP
{
    /// <summary>
    /// UDP连接类，继承自Linker类，并实现IUdpLinker接口。
    /// </summary>
    internal class UdpLinker : Linker, IUdpLinker
    {
        protected override void ExecuteSend(ref ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
