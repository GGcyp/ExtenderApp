using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentRecyclebinListViewModel : ExtenderAppViewModel<TorrentRecyclebinListView, TorrentModel>
    {
        public TorrentRecyclebinListViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }
    }
}
