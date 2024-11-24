using ExtenderApp.Abstract;

namespace ExtenderApp.Service
{
    internal class ServiceStore : IServiceStore
    {
        public IDispatcherService DispatcherService { get; }

        public INavigationService NavigationService {  get; }

        public ServiceStore(IDispatcherService dispatcherService, INavigationService navigationService)
        {
            DispatcherService = dispatcherService;
            NavigationService = navigationService;
        }
    }
}
