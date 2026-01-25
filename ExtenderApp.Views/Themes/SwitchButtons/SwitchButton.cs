using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ExtenderApp.Views.Themes
{
    public class SwitchButton : ButtonBase
    {
        static SwitchButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SwitchButton),
                new FrameworkPropertyMetadata(typeof(SwitchButton))
            );
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Fill),
                typeof(Brush),
                typeof(SwitchButton));

        public Brush Stroke
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(nameof(Stroke),
                typeof(Brush),
                typeof(SwitchButton));

        public bool IsSwitch
        {
            get { return (bool)GetValue(IsSwitchProperty); }
            set { SetValue(IsSwitchProperty, value); }
        }

        public static readonly DependencyProperty IsSwitchProperty =
            DependencyProperty.Register(nameof(IsSwitch),
                typeof(bool),
                typeof(SwitchButton),
                new PropertyMetadata(false));

        public Geometry DefaultGeometry
        {
            get { return (Geometry)GetValue(DefaultGeometryProperty); }
            set { SetValue(DefaultGeometryProperty, value); }
        }

        public static readonly DependencyProperty DefaultGeometryProperty =
            DependencyProperty.Register(
                nameof(DefaultGeometry),
                typeof(Geometry),
                typeof(SwitchButton),
                new PropertyMetadata(null));

        public Geometry TargetGeometry
        {
            get { return (Geometry)GetValue(TargetGeometryProperty); }
            set { SetValue(TargetGeometryProperty, value); }
        }

        public static readonly DependencyProperty TargetGeometryProperty =
            DependencyProperty.Register(
                nameof(TargetGeometry),
                typeof(Geometry),
                typeof(SwitchButton),
                new PropertyMetadata(null));

        protected override void OnClick()
        {
            base.OnClick();
            IsSwitch = !IsSwitch;
        }
    }
}