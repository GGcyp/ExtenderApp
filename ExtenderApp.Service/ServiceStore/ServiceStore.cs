using ExtenderApp.Abstract;

namespace ExtenderApp.Service
{
    /// <summary>
    /// 服务存储类，实现了IServiceStore接口
    /// </summary>s
    internal class ServiceStore : IServiceStore
    {
        public IDispatcherService DispatcherService { get; }

        public INavigationService NavigationService {  get; }

        public ITemporarilyService TemporarilyService { get; }

        public INetWorkService NetWorkService { get; }

        public ILogingService LogingService {  get; }

        public ServiceStore(IDispatcherService dispatcherService, INavigationService navigationService, ITemporarilyService temporarilyStore, INetWorkService netWorkService, ILogingService logingService)
        {
            DispatcherService = dispatcherService;
            NavigationService = navigationService;
            TemporarilyService = temporarilyStore;
            NetWorkService = netWorkService;
            LogingService = logingService;
        }
    }
}
