using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.MainViews.ViewModels
{
    public class SettingsViewModel : ExtenderAppViewModel
    {
        public IView? CurrentPluginSettingsView { get; set; }

        public RelayCommand<TextBlock> ScrollToTopCommand { get; set; }

        public SettingsViewModel()
        {
            ScrollToTopCommand = new(ScrollToTop);
        }

        public void SetMainViewSettings(IMainViewSettings mainViewSettings)
        {
            //IList list = View.navigationBar.Children;

            //list.Add(mainViewSettings.CreateSettingsNavigationButton(View.basicSettings));
            //list.Add(mainViewSettings.CreateSettingsNavigationButton(View.pluginSettings));
        }

        public void InitMainViewSettings()
        {
            //var collection = View.navigationBar.Children;
            //for (int i = 0; i < collection.Count; i++)
            //{
            //    if (collection[i] is Button button)
            //    {
            //        button.Command = ScrollToTopCommand;
            //    }
            //}
        }

        public void ScrollToTop(TextBlock block)
        {
            if (block == null)
                return;

            // 获取 ScrollViewer 内容面板（StackPanel）
            //var panel = View.settingsPanel; // 不是 Block.Parent，也不是 Block.Parent.Parent

            //var scrollViewer = View.settingsBar;
            //if (panel == null || scrollViewer == null)
            //    return;

            //panel.UpdateLayout();
            //scrollViewer.UpdateLayout();

            //// 计算 Block 相对于内容面板的 Y 坐标
            //var transform = Block.TransformToAncestor(panel);
            //var point = transform.Transform(new Point(0, 0));

            //// 滚动到该控件的顶部
            //scrollViewer.ScrollToVerticalOffset(point.Y);
        }
    }
}