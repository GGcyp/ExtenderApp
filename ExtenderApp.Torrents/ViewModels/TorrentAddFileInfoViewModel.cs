using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using MonoTorrent;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentAddFileInfoViewModel : ExtenderAppViewModel<TorrentAddFileInfoView, TorrentModel>
    {
        public TorrentInfo? CurrentTorrentInfo { get; set; }

        #region Command

        public NoValueCommand StartDownloadCommand { get; set; }
        public NoValueCommand SelectedAllCommand { get; set; }

        #endregion

        public TorrentAddFileInfoViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            StartDownloadCommand = new(SatrtDownload);
            SelectedAllCommand = new(() =>
            {
                CurrentTorrentInfo!.SelecrAllFiles();
            });
        }

        public override void Enter(ViewInfo oldViewInfo)
        {
            var torrentAddViewModel = oldViewInfo.ViewModel as TorrentAddViewModel;
            CurrentTorrentInfo = torrentAddViewModel?.CurrentTorrentInfo;
            Model.DowloadTorrentCollection!.Add(CurrentTorrentInfo!);
            Model.SatrtTorrentAsync(CurrentTorrentInfo!);
        }

        private void SatrtDownload()
        {
            Task.Run(async () =>
            {
                await Model.SatrtTorrentAsync(CurrentTorrentInfo!);
                var manager = CurrentTorrentInfo.Manager;
                Debug(manager.TrackerManager.Tiers.Count);
                manager.PeerConnected += (sender, e) =>
                {
                    Debug(e.Peer);
                    //Debug(e.)
                };
                manager.TorrentStateChanged += (sender, e) =>
                {
                    Debug(e.NewState);
                };
            });
        }
    }
}
