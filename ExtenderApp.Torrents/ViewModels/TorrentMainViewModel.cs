using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using MonoTorrent.Logging;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentMainViewModel : ExtenderAppViewModel<TorrentMainView, TorrentModel>
    {
        private readonly ScheduledTask _task;
        private readonly Dictionary<Type, IView> _dict;

        #region Command

        #region List

        public RelayCommand ToDownloadListCommand { get; set; }

        public RelayCommand ToDownloadCompletedListdCommand { get; set; }

        public RelayCommand ToSeedListCommand { get; set; }

        public RelayCommand ToRecyclebinListCommand { get; set; }

        #endregion List

        public RelayCommand ShowAddTorrentViewCommand { get; set; }

        public RelayCommand ToFileInfoCommand { get; set; }

        public RelayCommand ToTorrentDetailsCommand { get; set; }

        #endregion Command

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

        private RelayCommand CreateTorrentListCommand<T>()
            where T : class, IView
        {
            return new RelayCommand(() =>
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

        private RelayCommand CreateTorrentDetailsCommand<T>()
            where T : class, IView
        {
            return new RelayCommand(() =>
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