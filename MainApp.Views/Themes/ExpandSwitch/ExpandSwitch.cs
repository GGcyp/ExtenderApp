using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainApp.Views.Themes
{
    /// <summary>
    /// 自定义展开按钮
    /// </summary>
    public class ExpandSwitch : ToggleButton
    {
        static ExpandSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExpandSwitch),
                new FrameworkPropertyMetadata(typeof(ExpandSwitch))
            );
        }

        public double PathWidth
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty PathWidthProperty = DependencyProperty.Register(
            nameof(PathWidth),
            typeof(double),
            typeof(ExpandSwitch),
            new PropertyMetadata(10.0)
        );

        public double PathHeight
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly DependencyProperty PathHeightProperty = DependencyProperty.Register(
            nameof(PathHeight),
            typeof(double),
            typeof(ExpandSwitch),
            new PropertyMetadata(10.0)
        );
    }
}
