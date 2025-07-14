using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    internal class HttpLinker : Linker, IHttpLinker
    {
        public HttpLinker(ResourceLimiter resourceLimit) : base(resourceLimit)
        {
        }

        public HttpLinker(Socket socket, ResourceLimiter resourceLimit) : base(socket, resourceLimit)
        {
        }

        public HttpLinker(AddressFamily addressFamily, ResourceLimiter resourceLimit) : base(addressFamily, resourceLimit)
        {
        }

        protected override int PacketLength => throw new NotImplementedException();

        protected override LinkOperateData CreateLinkOperateData(Socket socket)
        {
            throw new NotImplementedException();
        }

        protected override LinkOperateData CreateLinkOperateData(AddressFamily addressFamily)
        {
            throw new NotImplementedException();
        }
    }
}
