using System.Net.Sockets;
using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Networks
{
    internal class HttpLinker : Linker, IHttpLinker
    {
        protected override void ExecuteSend(ref ByteBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
