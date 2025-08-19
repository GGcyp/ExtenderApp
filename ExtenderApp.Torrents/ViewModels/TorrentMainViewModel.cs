using System.Net;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using Microsoft.Win32;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Logging;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentMainViewModel : ExtenderAppViewModel<TorrentMainView, TorrentModel>
    {
        private readonly ScheduledTask _task;

        #region Command

        #region List

        public NoValueCommand ToDownloadListCommand { get; set; }

        public NoValueCommand ToDownloadCompletedListdCommand { get; set; }

        public NoValueCommand ToRecyclebinListCommand { get; set; }

        #endregion

        public NoValueCommand AddTorrentCommand { get; set; }

        public NoValueCommand ToFileInfoCommand { get; set; }

        public NoValueCommand ToTorrentDetailsCommand { get; set; }

        #endregion

        public TorrentMainViewModel(TorrentLongingFactory factory, IServiceStore serviceStore) : base(serviceStore)
        {
            LoggerFactory.Register(factory.CreateTorrentLonging);

            ToDownloadListCommand = CreateTorrentListCommand<TorrentDownloadListView>();
            ToDownloadCompletedListdCommand = CreateTorrentListCommand<TorrentDownloadCompletedListView>();
            ToRecyclebinListCommand = CreateTorrentListCommand<TorrentRecyclebinListView>();

            ToFileInfoCommand = CreateTorrentDetailsCommand<TorrentDownloadFileInfoView>();
            ToTorrentDetailsCommand = CreateTorrentDetailsCommand<TorrentDownloadStateView>();

            AddTorrentCommand = new(ShowAddTorrentView);

            Model.DowloadTorrentCollection = new();
            Model.DowloadCompletedTorrentCollection = new();
            Model.TorrentListView = NavigateTo<TorrentDownloadListView>();
            Model.TorrentDetailsView = NavigateTo<TorrentDownloadFileInfoView>();

            CurrrentMainWindow.MinWidth = 800;
            CurrrentMainWindow.MinHeight = 600;
            Model.CreateTorrentClientEngine();
            Model.SaveDirectory = _serviceStore.PathService.CreateFolderPathForAppRootFolder("test");

            _task = new();
            TimeSpan outTime = TimeSpan.FromSeconds(1);
            _task.StartCycle(o => Model.UpdateTorrentInfo(), outTime, outTime);
        }

        private NoValueCommand CreateTorrentListCommand<T>() where T : class, IView
        {
            return new NoValueCommand(() =>
            {
                Model.TorrentListView = NavigateTo<T>();
            });
        }

        private NoValueCommand CreateTorrentDetailsCommand<T>() where T : class, IView
        {
            return new NoValueCommand(() =>
            {
                Model.TorrentDetailsView = NavigateTo<T>();
            });
        }

        public void ShowAddTorrentView()
        {
            var window = NavigateToWindow<TorrentAddView>();
            var view = window.CurrentView as TorrentAddView;
            window.Height = 300;
            window.Width = 300;
            window.Owner = CurrrentMainWindow;
            window.WindowStartupLocation = 2;
            window.Show();
        }
    }
}
