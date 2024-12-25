using System.Net.Sockets;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Network
{
    internal class NetworkClient_TCP : NetworkClient
    {
        private readonly TcpClient _tcpClient;

        public NetworkClient_TCP()
        {
            _tcpClient = new TcpClient();
        }

        public override async Task<object> SendAsync(NetworkRequest request)
        {
            if (request is null || request is not TcpRequest tcpRequest)
                throw new ArgumentNullException(nameof(request));

            if (tcpRequest.IPEndPoint is null)
                throw new InvalidOperationException(nameof(tcpRequest));

            await _tcpClient.ConnectAsync(tcpRequest.IPEndPoint);

            NetworkStream stream = _tcpClient.GetStream();

            return stream;
        }
    }
}
