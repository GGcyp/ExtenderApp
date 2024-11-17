using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MainApp.Mod.PPR
{
    public class PPRDataGridLabelColumn : Control
    {
        static PPRDataGridLabelColumn()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PPRDataGridLabelColumn),
                new FrameworkPropertyMetadata(typeof(PPRDataGridLabelColumn))
            );
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(PPRDataGridLabelColumn),
            new PropertyMetadata(string.Empty)
        );

        public HorizontalAlignment HorizontalMessageAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalMessageAlignmentProperty); }
            set { SetValue(HorizontalMessageAlignmentProperty, value); }
        }

        public static readonly DependencyProperty HorizontalMessageAlignmentProperty =
            DependencyProperty.Register(
                nameof(HorizontalMessageAlignment),
                typeof(HorizontalAlignment),
                typeof(PPRDataGridLabelColumn),
                new PropertyMetadata(HorizontalAlignment.Center)
            );

        public VerticalAlignment VerticalMessageAlignment
        {
            get { return (VerticalAlignment)GetValue(VerticalMessageAlignmentProperty); }
            set { SetValue(VerticalMessageAlignmentProperty, value); }
        }

        public static readonly DependencyProperty VerticalMessageAlignmentProperty =
            DependencyProperty.Register(
                nameof(VerticalMessageAlignment),
                typeof(VerticalAlignment),
                typeof(PPRDataGridLabelColumn),
                new PropertyMetadata(VerticalAlignment.Center)
            );

        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public new static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
                nameof(BorderThickness),
                typeof(Thickness),
                typeof(PPRDataGridLabelColumn),
                new PropertyMetadata(new Thickness(1))
            );
    }
}
