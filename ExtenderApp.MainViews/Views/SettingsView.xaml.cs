using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtenderApp.MainViews.ViewModels;
using ExtenderApp.Views;

namespace ExtenderApp.MainViews.Views
{
    /// <summary>
    /// SettingsView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsView : ExtenderAppView
    {
        private SolidColorBrush settingsNavigationLightBrushes;

        public SettingsView(SettingsViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
            settingsNavigationLightBrushes = new SolidColorBrush(Color.FromRgb(220, 225, 230));
        }

        private void SettingsBar_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 获取内容面板
            double minDistance = double.MaxValue;
            var item = GetTopTextBlock(navigationBar.Children, settingsPanel);

            //// currentBlock 即为当前可视区块
            //HighlightNavigationButton(currentBlock);
            //ViewModel<SettingsViewModel>().Info();
            item.Item1.Background = settingsNavigationLightBrushes;
            item.Item1.Foreground = Brushes.Black;
        }

        private (Button, TextBlock?) GetTopTextBlock(UIElementCollection collection, StackPanel panel)
        {
            TextBlock? textBlockResult = null;
            Button? buttonResult = null;
            double minDistance = double.MaxValue;
            foreach (UIElement child in collection)
            {
                if (child is not Button button)
                    continue;
                button.Background = Brushes.Transparent;
                button.Foreground = Brushes.White;
                if (button.CommandParameter is TextBlock block)
                {
                    // 计算区块顶部相对于内容面板的Y坐标
                    var transform = block.TransformToAncestor(panel);
                    var point = transform.Transform(new Point(0, 0));
                    double distance = Math.Abs(point.Y - settingsBar.VerticalOffset);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        textBlockResult = block;
                        buttonResult = button;
                    }
                }
            }
            return (buttonResult, textBlockResult);
        }
    }
}
