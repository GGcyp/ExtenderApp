using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;
using ExtenderApp.MainViews.Models;
using ExtenderApp.MainViews.Views;

namespace ExtenderApp.MainViews.ViewModels
{
    public class MainViewModel : ExtenderAppViewModel<MainView, MainModel>
    {
        public MainViewModel(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
            Model.CurrentView = NavigateTo<PluginView>();
        }
    }
}
