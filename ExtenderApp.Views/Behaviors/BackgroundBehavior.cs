using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace ExtenderApp.Views.Behaviors
{
    public class BackgroundBehavior : Behavior<Panel>
    {
        #region Property

        public Color TargetColor
        {
            get { return (Color)GetValue(TargetColorProperty); }
            set { SetValue(TargetColorProperty, value); }
        }

        public static readonly DependencyProperty TargetColorProperty =
            DependencyProperty.Register(
                "TargetColor",
                typeof(Color),
                typeof(BackgroundBehavior));


        public int AnimationDuration
        {
            get { return (int)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(
                "AnimationDuration",
                typeof(int),
                typeof(BackgroundBehavior),
                new PropertyMetadata(500));

        #endregion

        private Color lastColor;

        public void ToggleVisibility(bool isVisible, EventHandler? eventHandler = null)
        {
            Color target = isVisible ? TargetColor : lastColor;
            AnimateColumnWidth(target, eventHandler);
        }

        private void AnimateColumnWidth(Color target, EventHandler? eventHandler)
        {
            // 创建宽度动画
            var animation = new ColorAnimation
            {
                To = target,
                Duration = new Duration(TimeSpan.FromMilliseconds(AnimationDuration)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            if (eventHandler != null)
            {
                animation.Completed += eventHandler;
            }

            AssociatedObject.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            var background = AssociatedObject.Background as SolidColorBrush;
            if (background != null && !background.IsFrozen)
            {
                lastColor = background.Color;
            }
            else
            {
                lastColor = background == null ? Color.FromArgb(0, 0, 0, 0) : background.Color;
                AssociatedObject.Background = new SolidColorBrush(lastColor);
            }
        }
    }
}
