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

        public ILogService LoggerService { get; }

        public ServiceStore(IDispatcherService dispatcherService, INavigationService navigationService, ITemporarilyService temporarilyStore, IRefreshService refreshService, ILogService loggerService)
        {
            DispatcherService = dispatcherService;
            NavigationService = navigationService;
            TemporarilyService = temporarilyStore;
            RefreshService = refreshService;
            LoggerService = loggerService;
        }
    }
}
