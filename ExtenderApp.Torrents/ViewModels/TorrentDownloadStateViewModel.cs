using System.Windows;
using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;
using ExtenderApp.Views.Commands;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentDownloadStateViewModel : ExtenderAppViewModel<TorrentDownloadStateView, TorrentModel>
    {
        #region Command

        public NoValueCommand OpenSaveFolderCommand { get; set; }
        public NoValueCommand CopyMagnetLinkCommand { get; set; }

        #endregion

        public TorrentDownloadStateViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            OpenSaveFolderCommand = new(OpenSaveFolder);
            CopyMagnetLinkCommand = new(CopyTextToClipboard);
        }

        /// <summary>
        /// 将文本复制到系统剪贴板
        /// </summary>
        private void CopyTextToClipboard()
        {
            var magnetLink = Model.SelectedTorrent?.TorrentMagnetLink;
            // 检查文本是否有效
            if (string.IsNullOrEmpty(Model.SelectedTorrent?.TorrentMagnetLink))
                return;

            DispatcherInvoke(() =>
            {
                // 将文本复制到剪贴板
                Clipboard.SetText(Model.SelectedTorrent?.TorrentMagnetLink);
            });
        }

        private void OpenSaveFolder()
        {
            if (Model.SelectedTorrent == null)
                return;

            OpenFolder(Model.SelectedTorrent.SavePath);
        }
    }
}
