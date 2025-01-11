using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Services;

namespace ExtenderApp.Service
{
    internal class ScopeServiceStore : ServiceStore, IModServiceStore
    {
        public ModDetails ModDetails { get; }

        public ScopeServiceStore(ModDetails modDetails, IDispatcherService dispatcherService, INavigationService navigationService, ITemporarilyService temporarilyStore, ILogingService logingService, INetWorkService netWorkService, IModService modService, ILocalDataService localDataService, IRefreshService refreshService) : base(dispatcherService, navigationService, temporarilyStore, logingService, netWorkService, modService, localDataService, refreshService)
        {
            ModDetails = modDetails;
        }
    }
}
