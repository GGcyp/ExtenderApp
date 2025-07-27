using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Views.Themes;

namespace ExtenderApp.Torrents.Themes
{
    public class TorrentTreeView : TreeView
    {
        static TorrentTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TorrentTreeView), new FrameworkPropertyMetadata(typeof(TorrentTreeView)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TorrentTreeItem();
        }
    }
}
