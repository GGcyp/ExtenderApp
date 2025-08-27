using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace ExtenderApp.MainViews.Behaviors
{
    internal class ShowMessageBehavior : Behavior<Border>
    {
        #region 依赖属性
        public int AnimationDuration
        {
            get => (int)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(int), typeof(ShowMessageBehavior), new PropertyMetadata(300));

        public Color VisibleBackgroundColor
        {
            get => (Color)GetValue(VisibleBackgroundColorProperty);
            set => SetValue(VisibleBackgroundColorProperty, value);
        }

        public static readonly DependencyProperty VisibleBackgroundColorProperty =
            DependencyProperty.Register("VisibleBackgroundColor", typeof(Color), typeof(ShowMessageBehavior), new PropertyMetadata(Color.FromArgb(180, 60, 60, 60)));

        public double OffsetDistance
        {
            get => (double)GetValue(OffsetDistanceProperty);
            set => SetValue(OffsetDistanceProperty, value);
        }

        public static readonly DependencyProperty OffsetDistanceProperty =
            DependencyProperty.Register("OffsetDistance", typeof(double), typeof(ShowMessageBehavior), new PropertyMetadata(20.0));

        public int ShowDuration
        {
            get => (int)GetValue(ShowDurationProperty);
            set => SetValue(ShowDurationProperty, value);
        }

        public static readonly DependencyProperty ShowDurationProperty =
            DependencyProperty.Register("ShowDuration",
                typeof(int),
                typeof(ShowMessageBehavior),
                new PropertyMetadata(1000));

        #endregion

        private TranslateTransform translateTransform;
        private SolidColorBrush backgroundBrush;
        private Color targetTransparentColor;
        private bool isAttached; // 标记是否已附加到控件

        protected override void OnAttached()
        {
            base.OnAttached();
            isAttached = true;

            // 初始化前先检查控件是否有效
            if (AssociatedObject == null) return;

            // 确保背景画刷未被冻结（关键：冻结的画刷不能动画）
            backgroundBrush = new SolidColorBrush(Colors.Transparent);
            AssociatedObject.Background = backgroundBrush;

            translateTransform = new TranslateTransform();
            AssociatedObject.RenderTransform = translateTransform;

            targetTransparentColor = Color.FromArgb(0,
                VisibleBackgroundColor.R,
                VisibleBackgroundColor.G,
                VisibleBackgroundColor.B);

            ResetToHiddenState();
        }

        private void ResetToHiddenState()
        {
            translateTransform.Y = -OffsetDistance;
            backgroundBrush.Color = targetTransparentColor;
        }

        public void ToggleVisibility()
        {
            ResetToHiddenState();

            // 显示动画
            Animate(0, VisibleBackgroundColor);

            // 延迟后执行隐藏动画（增加状态检查）
            Task.Run(async () =>
            {
                await Task.Delay(AnimationDuration + ShowDuration);

                AssociatedObject.Dispatcher.Invoke(() =>
                {
                    Animate(OffsetDistance, targetTransparentColor);
                });
            });
        }

        private void Animate(double targetOffset, Color targetColor)
        {
            // 验证动画时间（避免无效值）
            if (AnimationDuration <= 0)
                AnimationDuration = 300; // 强制默认值

            // 停止当前动画（防止冲突）
            translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);

            // 位置动画
            var positionAnim = new DoubleAnimation
            {
                To = targetOffset,
                Duration = TimeSpan.FromMilliseconds(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // 颜色动画
            var colorAnim = new ColorAnimation
            {
                To = targetColor,
                Duration = TimeSpan.FromMilliseconds(AnimationDuration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // 启动动画
            translateTransform.BeginAnimation(TranslateTransform.YProperty, positionAnim);
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            isAttached = false;

            // 清理动画和资源
            if (translateTransform != null)
                translateTransform.BeginAnimation(TranslateTransform.YProperty, null);

            if (backgroundBrush != null)
                backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);

            translateTransform = null;
            backgroundBrush = null;
        }
    }
}
