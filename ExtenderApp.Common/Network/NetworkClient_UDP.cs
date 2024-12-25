using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Network
{
    internal class NetworkClient_UDP : NetworkClient
    {
        private readonly UdpClient _udpClient;

        public NetworkClient_UDP()
        {
            _udpClient = new UdpClient();
        }
    }
}
