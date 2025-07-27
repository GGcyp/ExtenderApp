using System.ComponentModel;
using ExtenderApp.Data;
using MonoTorrent;
using MonoTorrent.Client;

namespace ExtenderApp.Torrents.Models
{
    /// <summary>
    /// 表示一个Torrent文件信息节点，继承自FileNode泛型类。
    /// </summary>
    /// <typeparam name="TorrentFileInfoNode">泛型类型参数，指定节点类型为TorrentFileInfoNode。</typeparam>
    public class TorrentFileInfoNode : FileNode<TorrentFileInfoNode>, INotifyPropertyChanged
    {
        /// <summary>
        /// 获取或设置下载进度。
        /// </summary>
        public int Progress { get; set; }

        private bool isNeedDownload;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 获取或设置是否需要下载。
        /// </summary>
        public bool IsNeedDownload
        {
            get => isNeedDownload;
            set
            {
                if (isNeedDownload == value)
                    return;
                isNeedDownload = value;
                AllNeedDownload(isNeedDownload);

                if (!IsNeedDownload)
                    SetFilePriority(Priority.DoNotDownload);
            }
        }

        /// <summary>
        /// 获取或设置深度值
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// 获取或设置TorrentManagerFile属性
        /// </summary>
        /// <value>TorrentManagerFile属性的值</value>
        public ITorrentManagerFile? TorrentManagerFile { get; set; }

        public TorrentManager? TorrentManager { get; set; }

        public Priority Priority
        {
            get
            {
                if (TorrentManagerFile == null)
                    return IsNeedDownload ? Priority.Normal : Priority.DoNotDownload;

                return TorrentManagerFile.Priority;
            }
            set
            {
                if (TorrentManager == null || TorrentManagerFile == null)
                    return;

                SetFilePriority(value);
            }
        }

        /// <summary>
        /// 设置所有子节点是否需要下载
        /// </summary>
        /// <param name="isNeedDownload">是否需要下载</param>
        public void AllNeedDownload(bool isNeedDownload)
        {
            LoopAllChildNodes(n => n.IsNeedDownload = isNeedDownload);
        }

        public void TorrentFileInfoChanged()
        {
            Priority priority = Priority.Normal;
            if (!IsNeedDownload)
            {
                Priority = Priority.DoNotDownload;
                return;
            }

            SetFilePriority(priority);
        }

        private void SetFilePriority(Priority priority)
        {
            if (TorrentManagerFile == null || TorrentManager == null)
                return;

            Task.Run(() => TorrentManager.SetFilePriorityAsync(TorrentManagerFile, priority)).ConfigureAwait(false);
        }
    }
}
