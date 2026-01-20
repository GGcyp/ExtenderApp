using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Torrents.Views;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Torrents.ViewModels
{
    public class TorrentDownloadStateViewModel : ExtenderAppViewModel<TorrentDownloadStateView, TorrentModel>
    {
        #region Command

        public RelayCommand OpenSaveFolderCommand { get; set; }

        public RelayCommand CopyMagnetLinkCommand { get; set; }

        public RelayCommand AddPeerCommand { get; set; }

        #endregion Command

        public TorrentDownloadStateViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            OpenSaveFolderCommand = new(OpenSaveFolder);
            CopyMagnetLinkCommand = new(CopyTextToClipboard);
            AddPeerCommand = new(AddPeer);
        }

        /// <summary>
        /// 将文本复制到系统剪贴板
        /// </summary>
        private void CopyTextToClipboard()
        {
            var magnetLink = Model.SelectedTorrent?.TorrentMagnetLink;
            // 检查文本是否有效
            if (string.IsNullOrEmpty(magnetLink))
                return;

            // 将文本复制到剪贴板
            ClipboardSetText(magnetLink);
        }

        private void OpenSaveFolder()
        {
            if (Model.SelectedTorrent == null)
                return;

            OpenFolder(Model.SelectedTorrent.SavePath);
        }

        private void AddPeer()
        {
        }
    }
}