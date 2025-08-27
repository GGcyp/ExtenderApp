using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;

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
            View.Window.Closed += (s, e) =>
            {
                CurrentTorrentInfo.UpdateDownloadState(false);
                MainWindowTopmost();
            };
        }

        private void SatrtDownload()
        {
            Task.Run(async () =>
            {
                await Model.SatrtTorrentAsync(CurrentTorrentInfo!);
                DispatcherInvoke(() =>
                {
                    View.Window?.Close();
                    Model.DowloadTorrentCollection!.Add(CurrentTorrentInfo!);
                    Model.SelectedTorrent = CurrentTorrentInfo;
                    SaveModel();
                });
                MainWindowTopmost();
            });
        }
    }
}
