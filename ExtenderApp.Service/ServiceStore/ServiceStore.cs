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

        public IRefreshService RefreshService { get; }

        public ILogingService LoggingService { get; }

        public ServiceStore(IDispatcherService dispatcherService, INavigationService navigationService, ITemporarilyService temporarilyStore, IRefreshService refreshService, ILogingService loggingService)
        {
            DispatcherService = dispatcherService;
            NavigationService = navigationService;
            TemporarilyService = temporarilyStore;
            RefreshService = refreshService;
            LoggingService = loggingService;
        }
    }
}
