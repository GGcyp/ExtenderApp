using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

public class ExpanderAutoWidthBehavior : Behavior<Expander>
{
    public int AnimationDuration { get; set; } = 300;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Expanded += OnExpanded;
        AssociatedObject.Collapsed += OnCollapsed;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Expanded -= OnExpanded;
        AssociatedObject.Collapsed -= OnCollapsed;
    }

    private void OnExpanded(object sender, RoutedEventArgs e)
    {
        AssociatedObject.UpdateLayout();
        if (AssociatedObject.Content is FrameworkElement content)
        {
            content.Measure(new Size(double.PositiveInfinity, AssociatedObject.ActualHeight));
            double targetWidth = content.DesiredSize.Width;
            AnimateWidth(targetWidth);
        }
    }

    private void OnCollapsed(object sender, RoutedEventArgs e)
    {
        AnimateWidth(0);
    }

    private void AnimateWidth(double toWidth)
    {
        var animation = new DoubleAnimation
        {
            To = toWidth,
            Duration = TimeSpan.FromMilliseconds(AnimationDuration),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        AssociatedObject.BeginAnimation(FrameworkElement.WidthProperty, animation);
    }
}