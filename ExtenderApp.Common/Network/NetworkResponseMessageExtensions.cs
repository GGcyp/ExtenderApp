using ExtenderApp.Data;

namespace ExtenderApp.Common.Network
{
    public static class NetworkResponseMessageExtensions
    {
        public static NetworkResponseMessage ToNetworkResponseMessage(this HttpResponseMessage message)
        {
            var result = new NetworkResponseMessage();

            result.StatusCode = message.StatusCode;
            result.Content = message.Content;
            result.Headers = message.Headers;
            result.Version = message.Version;

            return result;
        }
    }
}
