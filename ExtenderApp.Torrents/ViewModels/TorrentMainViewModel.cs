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

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentMainViewModel : ExtenderAppViewModel<TorrentMainView, TorrentModel>
    {
        private readonly ScheduledTask _task;

        #region Command

        public NoValueCommand AddTorrentCommand { get; set; }

        public NoValueCommand ToDownloadCommand { get; set; }

        public NoValueCommand ToDownloadCompletedListdCommand { get; set; }

        public NoValueCommand ToFileInfoCommand { get; set; }

        public NoValueCommand ToTorrentDetailsCommand { get; set; }

        #endregion

        public TorrentMainViewModel(IMainWindow window, IServiceStore serviceStore) : base(serviceStore)
        {
            ToDownloadCommand = CreateTorrentListCommand<TorrentDownloadListView>();
            ToDownloadCompletedListdCommand = CreateTorrentListCommand<TorrentDownloadCompletedListView>();

            ToFileInfoCommand = CreateTorrentDetailsCommand<TorrentFileInfoView>();
            ToTorrentDetailsCommand = new(() =>
            {
                Model.TorrentDetailsView = null;
            });

            AddTorrentCommand = new(AddTorrent);

            Model.DowloadTorrentCollection = new();
            Model.DowloadCompletedTorrentCollection = new();
            Model.TorrentListView = NavigateTo<TorrentDownloadListView>();
            Model.TorrentDetailsView = NavigateTo<TorrentFileInfoView>();

            window.MinWidth = 800;
            window.MinHeight = 600;
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

        public void AddTorrent()
        {
            // 创建文件选择对话框实例
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 设置文件筛选器，仅显示 .torrent 文件
            openFileDialog.Filter = "Torrent 文件 (*.torrent)|*.torrent";
            // 打开文件选择对话框并获取用户选择结果
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // 用户选择了文件，获取文件路径
                string filePath = openFileDialog.FileName;
                LoadTorrent(filePath);
            }
        }

        public void LoadTorrent(string torrentPath)
        {
            Task.Run(() => LoadTorrentAsync(torrentPath));
        }

        public async Task LoadTorrentAsync(string torrentPath)
        {
            var torrent = await Torrent.LoadAsync(torrentPath);
            var info = new TorrentInfo(torrent);
            _serviceStore.DispatcherService.Invoke(() =>
            {
                Model.DowloadTorrentCollection.Add(info);
            });
        }
    }
}
