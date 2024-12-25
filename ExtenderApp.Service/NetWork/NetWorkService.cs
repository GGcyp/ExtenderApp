using System.Diagnostics;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.ObjectPools;
using ExtenderApp.Data;

namespace ExtenderApp.Service.NetWork
{
    internal class NetWorkService : INetWorkService
    {
        private readonly INetWorkProider _proider;
        private readonly ILogingService _logingService;

        public NetWorkService(INetWorkProider proider, ILogingService logingService)
        {
            _proider = proider;
            _logingService = logingService;
            SendAsync<NetworkStream>(new HttpRequest(new HttpRequestMessage(HttpMethod.Get, "https://www.baidu.com/")), s => Debug.Print("成功"), ConnectionsType.Http);
        }

        public async void SendAsync<T>(NetworkRequest request, Action<T> callback, ConnectionsType type)
        {
            try
            {
                var client = _proider.GetNetworkClient(type);
                var result = await client.SendAsync(request);
                callback?.Invoke((T)result);
            }
            catch (Exception ex)
            {
                _logingService.Error("网络请求发生了错误", nameof(INetWorkService), ex);
            }
        }
    }
}
