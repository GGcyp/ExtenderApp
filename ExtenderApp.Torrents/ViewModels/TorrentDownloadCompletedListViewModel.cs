using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentDownloadCompletedListViewModel : ExtenderAppViewModel<TorrentDownloadCompletedListView, TorrentModel>
    {
        public TorrentDownloadCompletedListViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }
    }
}
