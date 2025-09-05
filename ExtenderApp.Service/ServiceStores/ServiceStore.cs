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

        public ICacheService CacheService { get; }

        public ILogingService LogingService { get; }

        public IPluginService PluginService { get; }

        public ILocalDataService LocalDataService { get; }

        public IPathService PathService { get; }

        public IServiceProvider ServiceProvider { get; }

        public IMainWindowService MainWindowService { get; }

        public ISystemService SystemService { get; }

        public IMessageService MessageService { get; }

        public ServiceStore(IDispatcherService dispatcherService,
            INavigationService navigationService,
            ICacheService cacheStore,
            ILogingService logingService,
            IPluginService modService,
            ILocalDataService localDataService,
            IPathService pathService,
            IServiceProvider serviceProvider,
            IMainWindowService mainWindowService,
            ISystemService systemService,
            IMessageService messageService)
        {
            DispatcherService = dispatcherService;
            NavigationService = navigationService;
            CacheService = cacheStore;
            LogingService = logingService;
            PluginService = modService;
            LocalDataService = localDataService;
            PathService = pathService;
            ServiceProvider = serviceProvider;
            MainWindowService = mainWindowService;
            SystemService = systemService;
            MessageService = messageService;
        }
    }
}
