using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Services
{
    /// <summary>
    /// ScopeServiceStore 类，继承自 ServiceStore 类，实现了 IModServiceStore 接口。
    /// </summary>
    internal class PluginServiceStore : ServiceStore, IPuginServiceStore
    {
        public PluginDetails PuginDetails { get; }

        public PluginServiceStore(PluginDetails modDetails,
            IDispatcherService dispatcherService,
            INavigationService navigationService,
            ICacheService cacheStore,
            ILogingService logingService,
            IPluginService modService,
            ILocalDataService localDataService,
            IPathService pathService,
            IServiceProvider serviceProvider) :
            base(dispatcherService,
                navigationService,
                cacheStore,
                logingService,
                modService,
                localDataService,
                pathService,
                serviceProvider)
        {
            PuginDetails = modDetails;
        }
    }
}
