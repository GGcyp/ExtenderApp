using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Service;

namespace ExtenderApp.MainView
{
    public class MainViewModel : BaseViewModel<DisplayDetailsStore>
    {
        public DisplayDetailsStore DisplayDetailsStore { get; }

        public MainViewModel(DisplayDetailsStore store, IServiceStore serviceStore) : base(store, serviceStore)
        {
            DisplayDetailsStore = store;
        }

        public void NavigateTo(DisplayDetails details, IView view)
        {
            _serviceStore.NavigationService.NavigateTo(details.ViewType, null);
        }
    }
}
