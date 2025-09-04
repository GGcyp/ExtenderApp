using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentSettingsViewModel : ExtenderAppViewModel<TorrentSettingsView, TorrentModel>
    {
        public TorrentSettingsViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }
    }
}
