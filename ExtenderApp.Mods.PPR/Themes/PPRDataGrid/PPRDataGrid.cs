using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Common.ObjectPools;

namespace ExtenderApp.Mod.PPR
{
    /// <summary>
    /// PPR数据显示控件
    /// </summary>
    public class PPRDataGrid : TreeView
    {
        private static ObjectPool<PPRInventoryTreeViewItem> pool = ObjectPool.Create<PPRInventoryTreeViewItem>();

        public static readonly DependencyProperty TitlesProperty = DependencyProperty.Register(
            nameof(Titles),
            typeof(PPRTitles),
            typeof(PPRDataGrid)
        );

        static PPRDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PPRDataGrid),
                new FrameworkPropertyMetadata(typeof(PPRDataGrid))
            );
        }

        public PPRDataGrid()
        {

        }

        public PPRTitles Titles
        {
            get => (PPRTitles)GetValue(TitlesProperty);
            set => SetValue(TitlesProperty, value);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var item = pool.Get();
            item.Titles = Titles;
            return item;
        } 
    }
}
