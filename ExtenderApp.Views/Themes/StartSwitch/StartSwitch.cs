
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ExtenderApp.Views.Themes
{
    public class StartSwitch : ToggleButton
    {
        static StartSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(StartSwitch),
                new FrameworkPropertyMetadata(typeof(StartSwitch))
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
                typeof(StartSwitch));

        public Brush Stroke
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Stroke",
                typeof(Brush),
                typeof(StartSwitch));



        public bool IsStart
        {
            get { return (bool)GetValue(IsStartProperty); }
            set { SetValue(IsStartProperty, value); }
        }

        public static readonly DependencyProperty IsStartProperty =
            DependencyProperty.Register(
                "IsStart", 
                typeof(bool), 
                typeof(StartSwitch), 
                new PropertyMetadata(false));
    }
}
