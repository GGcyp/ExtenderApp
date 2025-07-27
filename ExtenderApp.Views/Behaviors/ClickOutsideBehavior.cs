using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace ExtenderApp.Views.Behaviors
{
    /// <summary>
    /// 点击外部行为类，用于处理ListBox控件的点击事件
    /// </summary>
    public class ClickOutsideBehavior : Behavior<ListBox>
    {
        /// <summary>
        /// 当行为附加到元素时调用
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 获取所在窗口并订阅预览鼠标按下事件
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.PreviewMouseDown += OnWindowPreviewMouseDown;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // 清理事件订阅避免内存泄漏
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.PreviewMouseDown -= OnWindowPreviewMouseDown;
            }
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsElementUnderMouse(AssociatedObject))
            {
                AssociatedObject.SelectedItem = null;
            }
        }

        /// <summary>
        /// 判断元素是否在鼠标下方
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns>元素是否在鼠标下方</returns>
        private bool IsElementUnderMouse(UIElement element)
        {
            if (element == null)
                return false;

            // 获取元素的实际尺寸
            Size elementSize;
            if (element is FrameworkElement frameworkElement)
            {
                // FrameworkElement使用ActualWidth/ActualHeight
                elementSize = new Size(frameworkElement.ActualWidth, frameworkElement.ActualHeight);
            }
            else
            {
                // 纯UIElement使用RenderSize
                elementSize = element.RenderSize;
            }

            // 获取鼠标在元素坐标系中的位置
            Point pos = Mouse.GetPosition(element);

            // 判断鼠标是否在元素范围内
            return pos.X >= 0 && pos.Y >= 0 &&
                   pos.X <= elementSize.Width &&
                   pos.Y <= elementSize.Height;
        }
    }
}
