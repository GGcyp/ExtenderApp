using System.Windows;
using System.Windows.Controls;

namespace ExtenderApp.Torrents.Themes
{
    public class TorrentTreeItem : TreeViewItem
    {
        static TorrentTreeItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TorrentTreeItem), new FrameworkPropertyMetadata(typeof(TorrentTreeItem)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TorrentTreeItem();
        }
    }
}
