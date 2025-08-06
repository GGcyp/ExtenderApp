using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentAddFileInfoViewModel : ExtenderAppViewModel<TorrentAddFileInfoView, TorrentModel>
    {
        public TorrentInfo? CurrentTorrentInfo { get; set; }

        public TorrentAddFileInfoViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
        }

        public override void Enter(ViewInfo oldViewInfo)
        {
            var torrentAddViewModel = oldViewInfo.ViewModel as TorrentAddViewModel;
            CurrentTorrentInfo = torrentAddViewModel?.CurrentTorrentInfo;
            Model.DowloadTorrentCollection.Add(CurrentTorrentInfo);
        }

    }
}
