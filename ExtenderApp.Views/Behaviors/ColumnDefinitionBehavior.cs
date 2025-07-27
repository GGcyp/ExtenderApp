using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ExtenderApp.Views.Animation;
using Microsoft.Xaml.Behaviors;

namespace ExtenderApp.Views.Behaviors
{
    /// <summary>
    /// 控制ColumnDefinition的行为类，实现列的显示/隐藏动画效果
    /// </summary>
    public class ColumnDefinitionBehavior : Behavior<ColumnDefinition>
    {
        #region 依赖属性

        /// <summary>
        /// 列的目标宽度（显示时）
        /// </summary>
        public double TargetWidth
        {
            get { return (double)GetValue(TargetWidthProperty); }
            set { SetValue(TargetWidthProperty, value); }
        }

        public static readonly DependencyProperty TargetWidthProperty =
            DependencyProperty.Register("TargetWidth",
                typeof(double),
                typeof(ColumnDefinitionBehavior));

        /// <summary>
        /// 动画持续时间（毫秒）
        /// </summary>
        public int AnimationDuration
        {
            get { return (int)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(int), typeof(ColumnDefinitionBehavior),
                new PropertyMetadata(300));

        /// <summary>
        /// 是否显示列
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(ColumnDefinitionBehavior),
                new PropertyMetadata(false, OnIsVisibleChanged));

        #endregion

        /// <summary>
        /// 当IsVisible属性改变时触发
        /// </summary>
        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColumnDefinitionBehavior behavior && behavior.AssociatedObject != null)
            {
                bool isVisible = (bool)e.NewValue;
                behavior.ToggleColumnVisibility(isVisible);
            }
        }

        /// <summary>
        /// 切换列的可见性
        /// </summary>
        private void ToggleColumnVisibility(bool isVisible)
        {
            GridLength targetWidth = isVisible ? new GridLength(TargetWidth) : new GridLength(0);
            AnimateColumnWidth(targetWidth);
        }

        /// <summary>
        /// 列宽度动画
        /// </summary>
        private void AnimateColumnWidth(GridLength targetWidth)
        {
            // 创建宽度动画
            var widthAnimation = new GridLengthAnimation
            {
                To = targetWidth,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // 应用动画到关联的ColumnDefinition
            AssociatedObject.BeginAnimation(ColumnDefinition.WidthProperty, widthAnimation);
        }

        /// <summary>
        /// 当行为附加到元素时调用
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            // 初始化宽度
            AssociatedObject.Width = new GridLength(0);
        }
    }
}
