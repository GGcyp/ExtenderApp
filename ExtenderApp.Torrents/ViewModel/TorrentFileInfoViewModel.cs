using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.Torrents.Models;
using ExtenderApp.ViewModels;
using ExtenderApp.Views;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents
{
    public class TorrentFileInfoViewModel : ExtenderAppViewModel<TorrentFileInfoView, TorrentModel>
    {
        #region 属性

        public TorrentInfo SelectedTorrent
        {
            get => Model.SelectedTorrent;
            set => Model.SelectedTorrent = value;
        }

        #endregion

        #region 命令

        public NoValueCommand StartCommand { get; set; }
        public NoValueCommand SelectAllCommand { get; set; }

        #endregion

        public TorrentFileInfoViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            StartCommand = new(SatrtTorrent, CanExecute);
            SelectAllCommand = new(SelecrAll, CanExecute);
        }

        private void SatrtTorrent()
        {
            Task.Run(SatrtTorrentAsync).ConfigureAwait(false);
        }

        private async Task SatrtTorrentAsync()
        {
            var selectedTorrent = Model.SelectedTorrent;

            TorrentManager? manager = null;
            if (selectedTorrent.Torrent != null)
            {
                manager = await Model.Engine.AddAsync(selectedTorrent.Torrent, Model.SaveDirectory);
            }
            else if (selectedTorrent.MagnetLink != null)
            {
                manager = await Model.Engine.AddAsync(selectedTorrent.MagnetLink, Model.SaveDirectory);
            }
            else
            {
                Error($"当前种子或磁力链接还未加载：{selectedTorrent.Torrent}", new ArgumentNullException());
                return;
            }

            selectedTorrent.Set(manager);
            Info($"开始下载；种子名字：{manager.Name}，种子哈希值：{manager.InfoHashes.V1OrV2.ToHex()}");
            await manager.StartAsync();
            await manager.LocalPeerAnnounceAsync();
        }

        private void SelecrAll()
        {
            var selectedTorrent = Model.SelectedTorrent;
            var list = selectedTorrent.Files;
            bool selecrAll = !selectedTorrent.SelecrAll;
            for (int i = 0; i < list.Count; i++)
            {
                var node = list[i];
                node.IsNeedDownload = selecrAll;
            }
            selectedTorrent.SelecrAll = selecrAll;
        }

        private bool CanExecute()
        {
            return Model.SelectedTorrent != null;
        }
    }
}
