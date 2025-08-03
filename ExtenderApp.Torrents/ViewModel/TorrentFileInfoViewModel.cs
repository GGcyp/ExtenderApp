using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using MonoTorrent;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentFileInfoViewModel : ExtenderAppViewModel<TorrentFileInfoView, TorrentModel>
    {
        #region 命令

        public NoValueCommand StartCommand { get; set; }
        public NoValueCommand SelectAllCommand { get; set; }

        #endregion

        public TorrentFileInfoViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            StartCommand = new(SatrtTorrent, CanExecute);
            SelectAllCommand = new(SelecrAll, CanExecute);
        }

        /// <summary>
        /// 开始下载选中的种子
        /// </summary>
        private void SatrtTorrent()
        {
            var info = Model.SelectedTorrent;
            if (info.IsDownloading)
                return;

            Task.Run(async () =>
            {
                info.SelectedFileCount = 0;
                info.SelectedFileLength = 0;
                foreach (var node in info.Files)
                {
                    node.UpdateDownloadState();

                    info.SelectedFileCount += node.GetSelectedFileCount();
                    info.SelectedFileLength += node.GetSelectedFileLength();
                }
                await Model.SatrtTorrentAsync(info);
                info.IsDownloading = true;
            });
            InfoHashes infoHashes = info.Torrent == null ? info.MagnetLink.InfoHashes : info.Torrent.InfoHashes;
            Info($"开始下载；种子名字：{info.Name}，种子哈希值：{infoHashes.V1OrV2.ToHex()}");
        }

        /// <summary>
        /// 选择全部文件
        /// </summary>
        private void SelecrAll()
        {
            var selectedTorrent = Model.SelectedTorrent;
            var list = selectedTorrent.Files;
            bool selecrAll = !selectedTorrent.SelecrAll;
            for (int i = 0; i < list.Count; i++)
            {
                var node = list[i];
                node.DisplayNeedDownload = selecrAll;
                node.AllNeedDownload(selecrAll);
            }
            selectedTorrent.SelecrAll = selecrAll;
        }

        /// <summary>
        /// 判断是否可以执行
        /// </summary>
        /// <returns>如果可以执行，则返回true；否则返回false</returns>
        private bool CanExecute()
        {
            return Model.SelectedTorrent != null;
        }
    }
}
