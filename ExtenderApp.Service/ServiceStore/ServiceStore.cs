using ExtenderApp.Abstract;

namespace ExtenderApp.Services
{
    /// <summary>
    /// 服务存储类，实现了IServiceStore接口
    /// </summary>s
    internal class ServiceStore : IServiceStore
    {
        public IDispatcherService DispatcherService { get; }

        public INavigationService NavigationService { get; }

        public ITemporarilyService TemporarilyService { get; }

        public ILogingService LogingService { get; }

        public INetWorkService NetWorkService { get; }

        public IModService ModService { get; }

        public ILocalDataService LocalDataService { get; }

        public ServiceStore(IDispatcherService dispatcherService,
            INavigationService navigationService, ITemporarilyService temporarilyStore,
            ILogingService logingService, INetWorkService netWorkService, IModService modService,
            ILocalDataService localDataService)
        {
            DispatcherService = dispatcherService;
            NavigationService = navigationService;
            TemporarilyService = temporarilyStore;
            LogingService = logingService;
            NetWorkService = netWorkService;
            ModService = modService;
            LocalDataService = localDataService;
        }
    }
}
