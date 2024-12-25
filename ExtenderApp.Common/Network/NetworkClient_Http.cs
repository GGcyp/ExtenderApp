using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Network
{
    internal class NetworkClient_Http : NetworkClient
    {
        private readonly HttpClient _httpClient;

        public NetworkClient_Http()
        {
            _httpClient = new();
        }

        public override async Task<object> SendAsync(NetworkRequest request)
        {
            if (request is not HttpRequest httpRequest)
                throw new ArgumentException(nameof(request));

            if (httpRequest.HttpRequestMessage is null)
                throw new ArgumentNullException(nameof(request));

            var responseMessage = await _httpClient.SendAsync(httpRequest.HttpRequestMessage);

            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception("未连接成功");

            var stream = await responseMessage.Content.ReadAsStreamAsync();

            return stream;
        }
    }
}
