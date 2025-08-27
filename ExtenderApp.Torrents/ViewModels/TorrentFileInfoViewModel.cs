using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Models;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;
using MonoTorrent;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentFileInfoViewModel : ExtenderAppViewModel<TorrentDownloadFileInfoView, TorrentModel>
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
            Model.SelectedTorrent!.SelecrAllFiles();
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
