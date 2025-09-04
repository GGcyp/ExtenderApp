using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace ExtenderApp.Views.Behaviors
{
    /// <summary>
    /// HeightBehavior 用于为 FrameworkElement 控件实现高度缓动动画切换。
    /// 通过绑定 IsVisible 属性控制展开/收起时的动画，TargetHeight 指定展开目标高度，AnimationDuration 指定动画时长（毫秒）。
    /// </summary>
    public class HeightBehavior : Behavior<FrameworkElement>
    {
        #region 依赖属性

        /// <summary>
        /// 目标高度，展开时动画到此高度。
        /// </summary>
        public double TargetHeight
        {
            get { return (double)GetValue(TargetHeightProperty); }
            set { SetValue(TargetHeightProperty, value); }
        }

        public static readonly DependencyProperty TargetHeightProperty =
            DependencyProperty.Register("TargetHeight",
                typeof(double),
                typeof(HeightBehavior));

        /// <summary>
        /// 动画时长（毫秒），控制高度变化的缓动速度。
        /// </summary>
        public int AnimationDuration
        {
            get { return (int)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration",
                typeof(int),
                typeof(HeightBehavior));

        /// <summary>
        /// 控制是否可见，切换时触发高度动画。
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
                typeof(HeightBehavior),
                new PropertyMetadata(false, OnIsVisibleChanged));

        #endregion

        /// <summary>
        /// 记录收起时的高度，用于动画回退。
        /// </summary>
        private double lastHeight;

        /// <summary>
        /// IsVisible 属性变更时触发，执行高度动画。
        /// </summary>
        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HeightBehavior behavior && behavior.AssociatedObject != null)
            {
                bool isVisible = (bool)e.NewValue;
                behavior.ToggleColumnVisibility(isVisible);
            }
        }

        /// <summary>
        /// 根据可见性切换高度，展开到 TargetHeight，收起到 lastHeight。
        /// </summary>
        private void ToggleColumnVisibility(bool isVisible)
        {
            double targetHeight = isVisible ? TargetHeight : lastHeight;
            AnimateColumnWidth(targetHeight);
        }

        /// <summary>
        /// 执行高度缓动动画。
        /// </summary>
        /// <param name="targetHeight">目标高度</param>
        private void AnimateColumnWidth(double targetHeight)
        {
            var heightAnimation = new DoubleAnimation
            {
                To = targetHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            AssociatedObject.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
        }

        /// <summary>
        /// 行为附加到控件时，记录初始高度。
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            lastHeight = AssociatedObject.Height;
        }
    }
}
