using System.Net;

namespace ExtenderApp.Data
{
    public class TcpRequest : NetworkRequest
    {
        /// <summary>
        /// 获取或设置IP端点
        /// </summary>
        public IPEndPoint IPEndPoint { get; set; }

        public TcpRequest(string ip, int port) : this(IPAddress.Parse(ip), port)
        {

        }

        public TcpRequest(IPAddress ipAddress, int port) : this(new IPEndPoint(ipAddress, port))
        {

        }

        public TcpRequest(IPEndPoint ip)
        {
            IPEndPoint = ip;
        }
    }
}
