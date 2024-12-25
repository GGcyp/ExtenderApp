using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Common.Network
{
    internal class NetworkClient : INetworkClient
    {
        public NetworkClient()
        {
        }

        public virtual Task<object> SendAsync(NetworkRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
