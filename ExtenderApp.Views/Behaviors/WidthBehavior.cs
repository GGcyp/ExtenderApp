using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ExtenderApp.Views.Animation;
using Microsoft.Xaml.Behaviors;

namespace ExtenderApp.Views.Behaviors
{
    /// <summary>
    /// WidthBehavior 用于为 FrameworkElement 控件实现宽度缓动动画切换。
    /// 通过绑定 IsVisible 属性控制展开/收起时的动画，TargetWidth 指定展开目标宽度，AnimationDuration 指定动画时长（毫秒）。
    /// </summary>
    public class WidthBehavior : Behavior<FrameworkElement>
    {
        #region 依赖属性

        /// <summary>
        /// 目标宽度，展开时动画到此宽度。
        /// </summary>
        public double TargetWidth
        {
            get { return (double)GetValue(TargetWidthProperty); }
            set { SetValue(TargetWidthProperty, value); }
        }

        public static readonly DependencyProperty TargetWidthProperty =
            DependencyProperty.Register("TargetWidth",
                typeof(double),
                typeof(WidthBehavior));

        /// <summary>
        /// 动画时长（毫秒），控制宽度变化的缓动速度。
        /// </summary>
        public int AnimationDuration
        {
            get { return (int)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration",
                typeof(int),
                typeof(WidthBehavior));

        /// <summary>
        /// 控制是否可见，切换时触发宽度动画。
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register(
                "IsVisible",
                typeof(bool),
                typeof(WidthBehavior),
                new PropertyMetadata(false, OnIsVisibleChanged));

        #endregion

        /// <summary>
        /// 记录收起时的宽度，用于动画回退。
        /// </summary>
        private double lastWidth;

        /// <summary>
        /// IsVisible 属性变更时触发，执行宽度动画。
        /// </summary>
        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WidthBehavior behavior && behavior.AssociatedObject != null)
            {
                bool isVisible = (bool)e.NewValue;
                behavior.ToggleColumnVisibility(isVisible);
            }
        }

        /// <summary>
        /// 根据可见性切换宽度，展开到 TargetWidth，收起到 lastWidth。
        /// </summary>
        /// <param name="isVisible">是否可见</param>
        private void ToggleColumnVisibility(bool isVisible)
        {
            double targetWidth = isVisible ? TargetWidth : lastWidth;
            AnimateColumnWidth(targetWidth);
        }

        /// <summary>
        /// 执行宽度缓动动画。
        /// </summary>
        /// <param name="targetWidth">目标宽度</param>
        private void AnimateColumnWidth(double targetWidth)
        {
            var widthAnimation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            AssociatedObject.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
        }

        /// <summary>
        /// 行为附加到控件时，记录初始宽度。
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            lastWidth = AssociatedObject.Width;
        }
    }
}
