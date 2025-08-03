using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace ExtenderApp.Views.Behaviors
{
    public class ForegroundBegavior : Behavior<TextBlock>
    {
        #region Property

        public Color TargetColor
        {
            get { return (Color)GetValue(TargetForegroundProperty); }
            set { SetValue(TargetForegroundProperty, value); }
        }

        public static readonly DependencyProperty TargetForegroundProperty =
            DependencyProperty.Register(
                "TargetColor",
                typeof(Color),
                typeof(ForegroundBegavior));


        public int AnimationDuration
        {
            get { return (int)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(
                "AnimationDuration",
                typeof(int),
                typeof(ForegroundBegavior),
                new PropertyMetadata(500));

        #endregion

        private Color lastColor;

        public void ToggleVisibility(bool isVisible)
        {
            Color target = isVisible ? TargetColor : lastColor;
            AnimateColumnWidth(target);
        }

        private void AnimateColumnWidth(Color target)
        {
            // 创建宽度动画
            var animation = new ColorAnimation
            {
                To = target,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            AssociatedObject.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            var foreground = AssociatedObject.Foreground as SolidColorBrush;
            if (foreground != null && !foreground.IsFrozen)
            {
                lastColor = foreground.Color;
            }
            else
            {
                lastColor = foreground == null ? Color.FromArgb(0, 0, 0, 0) : foreground.Color;
                Brush brush = new SolidColorBrush(lastColor);
                AssociatedObject.Foreground = brush;
            }
        }
    }
}
