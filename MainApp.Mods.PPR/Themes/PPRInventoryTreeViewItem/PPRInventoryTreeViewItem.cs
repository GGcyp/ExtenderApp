using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MainApp.Common.ObjectPool;

namespace MainApp.Mod.PPR
{
    public class PPRInventoryTreeViewItem : TreeViewItem
    {
        private static ObjectPool<PPRInventoryTreeViewItem> inventoryTreeViewItemPool = ObjectPool.Create<PPRInventoryTreeViewItem>();

        public static readonly DependencyProperty TitlesProperty = DependencyProperty.Register(
            nameof(Titles),
            typeof(PPRTitles),
            typeof(PPRInventoryTreeViewItem)
        );

        private bool isActive;

        public PPRTitles Titles
        {
            get => (PPRTitles)GetValue(TitlesProperty);
            set
            {
                if (isActive) return;
                SetValue(TitlesProperty, value);
                isActive = true;
            }
        }

        static PPRInventoryTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PPRInventoryTreeViewItem),
                new FrameworkPropertyMetadata(typeof(PPRInventoryTreeViewItem))
            );
        }

        public PPRInventoryTreeViewItem()
        {
            //Titles = titles;
            Unloaded += PPRInventoryTreeViewItem_Unloaded;
        }

        private void PPRInventoryTreeViewItem_Unloaded(object sender, RoutedEventArgs e)
        {
            IsExpanded = false;
            inventoryTreeViewItemPool.Release(this);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new PPRPeriodTreeViewItem();
        }
    }
}
