using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.MainViews.Models;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    public class PluginViewModle : ExtenderAppViewModel<MainModel>
    {
        public RelayCommand<PluginDetails> OpenPluginCommand { get; set; }

        public PluginViewModle(MainModel model, IServiceStore serviceStore) : base(model, serviceStore)
        {
            OpenPluginCommand = new(OpenPlugin);
        }

        public void OpenPlugin(PluginDetails details)
        {
        }
    }
}