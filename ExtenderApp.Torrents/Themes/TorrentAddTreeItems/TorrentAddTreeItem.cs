using System.Windows;
using System.Windows.Controls;

namespace ExtenderApp.Torrents.Themes
{
    /// <summary>
    /// TorrentAddTreeItem 类表示一个用于添加种子的树形项目。
    /// </summary>
    public class TorrentAddTreeItem : TreeViewItem
    {
        /// <summary>
        /// 静态构造函数，用于设置 TorrentAddTreeItem 的默认样式键。
        /// </summary>
        static TorrentAddTreeItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TorrentAddTreeItem), new FrameworkPropertyMetadata(typeof(TorrentAddTreeItem)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TorrentAddTreeItem();
        }
    }
}
