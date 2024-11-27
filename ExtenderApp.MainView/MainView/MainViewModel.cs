using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.Service;

namespace ExtenderApp.MainView
{
    public class MainViewModel : BaseViewModel
    {
        public DisplayDetailsStore DisplayDetailsStore { get; }

        public MainViewModel(DisplayDetailsStore store, IServiceStore serviceStore) : base(serviceStore)
        {
            DisplayDetailsStore = store;
        }

        public void NavigateTo(DisplayDetails details)
        {
            NavigateTo(details.ViewType);
        }
    }
}
