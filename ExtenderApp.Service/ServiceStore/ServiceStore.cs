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

        public IPluginService ModService { get; }

        public ILocalDataService LocalDataService { get; }

        public IPathService PathService { get; }

        public ServiceStore(IDispatcherService dispatcherService,
            INavigationService navigationService, ITemporarilyService temporarilyStore,
            ILogingService logingService, IPluginService modService,
            ILocalDataService localDataService, IPathService pathService)
        {
            DispatcherService = dispatcherService;
            NavigationService = navigationService;
            TemporarilyService = temporarilyStore;
            LogingService = logingService;
            ModService = modService;
            LocalDataService = localDataService;
            PathService = pathService;
        }
    }
}
