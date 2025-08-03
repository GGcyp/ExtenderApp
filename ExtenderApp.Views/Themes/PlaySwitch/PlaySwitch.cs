using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ExtenderApp.Views.Themes
{
    public class PlaySwitch : ToggleButton
    {
        static PlaySwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PlaySwitch),
                new FrameworkPropertyMetadata(typeof(PlaySwitch))
            );
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Fill",
                typeof(Brush),
                typeof(PlaySwitch));

        public Brush Stroke
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Stroke",
                typeof(Brush),
                typeof(PlaySwitch));
    }
}
