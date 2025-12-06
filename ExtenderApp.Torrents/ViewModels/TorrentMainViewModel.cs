using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Data;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using MonoTorrent.Logging;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentMainViewModel : ExtenderAppViewModel<TorrentMainView, TorrentModel>
    {
        private readonly ScheduledTask _task;
        private readonly Dictionary<Type, IView> _dict;

        #region Command

        #region List

        public NoValueCommand ToDownloadListCommand { get; set; }

        public NoValueCommand ToDownloadCompletedListdCommand { get; set; }

        public NoValueCommand ToSeedListCommand { get; set; }

        public NoValueCommand ToRecyclebinListCommand { get; set; }

        #endregion

        public NoValueCommand ShowAddTorrentViewCommand { get; set; }

        public NoValueCommand ToFileInfoCommand { get; set; }

        public NoValueCommand ToTorrentDetailsCommand { get; set; }

        #endregion

        public TorrentMainViewModel(TorrentLongingFactory factory, IServiceStore serviceStore) : base(serviceStore)
        {
#if DEBUG
            LoggerFactory.Register(factory.CreateTorrentLonging);
#endif
            if (MainWindow != null)
            {
                MainWindow.MinWidth = 800;
                MainWindow.MinHeight = 600;
            }

            ToDownloadListCommand = CreateTorrentListCommand<TorrentDownloadListView>();
            ToDownloadCompletedListdCommand = CreateTorrentListCommand<TorrentDownloadCompletedListView>();
            ToRecyclebinListCommand = CreateTorrentListCommand<TorrentRecyclebinListView>();
            ToFileInfoCommand = CreateTorrentDetailsCommand<TorrentDownloadFileInfoView>();
            ToTorrentDetailsCommand = CreateTorrentDetailsCommand<TorrentDownloadStateView>();
            ShowAddTorrentViewCommand = new(ShowAddTorrentView);

            LoadModel();

            Model.TorrentListView = NavigateTo<TorrentDownloadListView>();
            Model.TorrentDetailsView = NavigateTo<TorrentDownloadFileInfoView>();

            _dict = new();
            _task = new();
            TimeSpan outTime = TimeSpan.FromSeconds(1);
            _task.StartCycle(o => DispatcherInvoke(() =>
            {
                Model.UpdateTorrentInfo();
            }), outTime, outTime);
        }

        private NoValueCommand CreateTorrentListCommand<T>()
            where T : class, IView
        {
            return new NoValueCommand(() =>
            {
                var type = typeof(T);
                if (_dict.TryGetValue(type, out var view))
                {
                    Model.TorrentListView = view;
                    return;
                }

                view = NavigateTo<T>();
                _dict.Add(type, view);
                Model.TorrentListView = view;
            });
        }

        private NoValueCommand CreateTorrentDetailsCommand<T>()
            where T : class, IView
        {
            return new NoValueCommand(() =>
            {
                var type = typeof(T);
                if (_dict.TryGetValue(type, out var view))
                {
                    Model.TorrentDetailsView = view;
                    return;
                }

                view = NavigateTo<T>();
                _dict.Add(type, view);
                Model.TorrentDetailsView = view;
            });
        }

        public void ShowAddTorrentView()
        {
            var window = NavigateToWindow<TorrentAddView>();
            var view = window.CurrentView as TorrentAddView;
            window.Height = 300;
            window.Width = 300;
            window.Owner = MainWindow;
            window.WindowStartupLocation = 2;
            window.Show();
        }
    }
}
