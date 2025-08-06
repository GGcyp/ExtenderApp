using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Views.Themes;

namespace ExtenderApp.Torrents.Themes
{
    /// <summary>
    /// TorrentAddTreeView 类继承自 TreeView 类，用于表示一个特定的树形视图，专门用于添加种子。
    /// </summary>

    public class TorrentAddTreeView : TreeView
    {
        static TorrentAddTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TorrentAddTreeView), new FrameworkPropertyMetadata(typeof(TorrentAddTreeView)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TorrentAddTreeItem();
        }
    }
}
