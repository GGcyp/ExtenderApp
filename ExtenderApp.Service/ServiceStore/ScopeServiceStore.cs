using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Services;

namespace ExtenderApp.Service
{
    /// <summary>
    /// ScopeServiceStore 类，继承自 ServiceStore 类，实现了 IModServiceStore 接口。
    /// </summary>
    internal class ScopeServiceStore : ServiceStore, IModServiceStore
    {
        public PluginDetails ModDetails { get; }

        public ScopeServiceStore(PluginDetails modDetails, IDispatcherService dispatcherService, INavigationService navigationService, ITemporarilyService temporarilyStore, ILogingService logingService, IPluginService modService, ILocalDataService localDataService, IPathService pathService) : base(dispatcherService, navigationService, temporarilyStore, logingService, modService, localDataService, pathService)
        {
            ModDetails = modDetails;
        }
    }
}
