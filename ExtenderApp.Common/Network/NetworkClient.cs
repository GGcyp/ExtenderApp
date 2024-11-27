using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Network
{
    internal class NetworkClient : INetworkClient
    {
        private readonly HttpClient _httpClient;

        public NetworkClient()
        {
            _httpClient = new();
        }

        public async ValueTask<NetworkResponseMessage> SendAsync(NetworkRequestMessage message, HttpCompletionOption option)
        {
            var header = message.ToHttpRequestMessage();

            var responseMessage = await _httpClient.SendAsync(header, option);

            return responseMessage.ToNetworkResponseMessage();
        }
    }
}
