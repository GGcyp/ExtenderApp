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
        public ModDetails ModDetails { get; }

        public ScopeServiceStore(ModDetails modDetails, IDispatcherService dispatcherService, INavigationService navigationService, ITemporarilyService temporarilyStore, ILogingService logingService, IModService modService, ILocalDataService localDataService, IScheduledTaskService scheduledTaskService) : base(dispatcherService, navigationService, temporarilyStore, logingService, modService, localDataService, scheduledTaskService)
        {
            ModDetails = modDetails;
        }
    }
}
