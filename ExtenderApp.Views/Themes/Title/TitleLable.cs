using System.Windows;
using System.Windows.Controls;

namespace ExtenderApp.Views.Themes
{
    public class TitleLable : Control
    {
        static TitleLable()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TitleLable),
                new FrameworkPropertyMetadata(typeof(TitleLable))
            );
        }

        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register(
                nameof(BorderThickness),
                typeof(Thickness),
                typeof(TitleLable),
                new PropertyMetadata(new Thickness(1))
            );
    }
}
