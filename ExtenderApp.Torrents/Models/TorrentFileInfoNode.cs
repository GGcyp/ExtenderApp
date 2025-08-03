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
        public double Progress { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool displyNeedDownload;
        /// <summary>
        /// 获取或设置一个布尔值，指示是否需要显示下载提示。
        /// </summary>
        public bool DisplayNeedDownload
        {
            get => displyNeedDownload;
            set
            {
                displyNeedDownload = value;
                if (IsFile)
                    return;

                AllNeedDownload(displyNeedDownload);
            }
        }

        /// <summary>
        /// 获取或设置一个布尔值，指示是否需要下载。
        /// </summary>
        public bool NeedDownloading { get; set; }

        /// <summary>
        /// 获取或设置深度值
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// 获取或设置TorrentManagerFile属性
        /// </summary>
        /// <value>TorrentManagerFile属性的值</value>
        public ITorrentManagerFile? TorrentManagerFile { get; set; }

        /// <summary>
        /// 获取或设置优先级。
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// 设置所有子节点是否需要下载
        /// </summary>
        /// <param name="isNeedDownload">是否需要下载</param>
        public void AllNeedDownload(bool isNeedDownload)
        {
            LoopAllChildNodes(n => n.DisplayNeedDownload = isNeedDownload);
        }

        /// <summary>
        /// 更新进度。
        /// </summary>
        public void UpdetaProgress()
        {
            if (TorrentManagerFile != null)
            {
                Progress = TorrentManagerFile.BitField.PercentComplete;
            }

            LoopAllChildNodes(n =>
            {
                if (!IsFile || !NeedDownloading || n.TorrentManagerFile == null)
                    return;
                n.Progress = n.TorrentManagerFile.BitField.PercentComplete;
            });
        }

        /// <summary>
        /// 更新种子文件信息变化时的优先级。
        /// </summary>
        public void TorrentFileInfoChanged()
        {
            Priority priority = Priority.Normal;
            if (!NeedDownloading)
            {
                Priority = Priority.DoNotDownload;
                return;
            }
            else
            {
                Priority = Priority.High;
            }
        }

        /// <summary>
        /// 更新下载状态
        /// </summary>
        /// <remarks>
        /// 该方法将 IsDownloading 属性设置为 IsNeedDownload 属性的值，
        /// 然后遍历集合中的每个元素，调用它们的 UpdateDownloadState 方法。
        /// </remarks>
        public void UpdateDownloadState()
        {
            NeedDownloading = DisplayNeedDownload;

            for (int i = 0; i < Count; i++)
            {
                this[i].UpdateDownloadState();
            }
        }

        /// <summary>
        /// 取消更新下载状态
        /// </summary>
        /// <remarks>
        /// 该方法将 IsDownloading 属性设置为 IsNeedDownload 属性的值，
        /// 然后遍历集合中的每个元素，调用它们的 UpdateDownloadState 方法。
        /// </remarks>
        public void UnUpdateDownloadState()
        {
            DisplayNeedDownload = NeedDownloading;

            for (int i = 0; i < Count; i++)
            {
                this[i].UnUpdateDownloadState();
            }
        }


        /// <summary>
        /// 获取需要下载的文件数量
        /// </summary>
        /// <returns>返回需要下载的文件数量</returns>
        public int GetSelectedFileCount()
        {
            if (IsFile)
            {
                if (NeedDownloading)
                    return 1;
                return 0;
            }

            int count = 0;
            for (int i = 0; i < Count; i++)
            {
                count += this[i].GetSelectedFileCount();
            }
            return count;
        }

        /// <summary>
        /// 获取需要下载的文件总长度
        /// </summary>
        /// <returns>返回需要下载的文件总长度</returns>
        public long GetSelectedFileLength()
        {
            if (IsFile)
            {
                if (NeedDownloading)
                    return Length;
                return 0;
            }

            long length = 0;
            for (int i = 0; i < Count; i++)
            {
                length += this[i].GetSelectedFileLength();
            }
            return length;
        }
    }
}
