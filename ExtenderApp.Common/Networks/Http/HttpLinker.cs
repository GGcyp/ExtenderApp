using System.Net.Sockets;
using System.Text;
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

        protected override int PacketLength => (int)Utility.KilobytesToBytes(4);

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

        //public async Task<string> GetAsync(Uri uri)
        //{
        //    try
        //    {
        //        Connect(uri);

        //        string request = $"GET {uri.PathAndQuery} HTTP/1.1\r\n";
        //        request += $"Host: {uri.Host}\r\n";
        //        request += "Connection: close\r\n\r\n";

        //        byte[] requestData = Encoding.UTF8.GetBytes(request);
        //        Send(requestData);

        //        byte[] responseBuffer = new byte[4096];
        //        int bytesRead = await _networkStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
        //        string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

        //        Close();
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        //OnErrored?.Invoke(ex);
        //        return null;
        //    }
        //}
    }
}
