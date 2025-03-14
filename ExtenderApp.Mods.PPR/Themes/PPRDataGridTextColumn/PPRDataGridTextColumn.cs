﻿using System;
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

namespace ExtenderApp.Mod.PPR
{
    public class PPRDataGridTextColumn : Control
    {
        static PPRDataGridTextColumn()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PPRDataGridTextColumn),
                new FrameworkPropertyMetadata(typeof(PPRDataGridTextColumn))
            );
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(PPRDataGridTextColumn),
            new(true)
        );

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(PPRDataGridTextColumn),
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
                typeof(PPRDataGridTextColumn),
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
                typeof(PPRDataGridTextColumn),
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
                typeof(PPRDataGridTextColumn),
                new PropertyMetadata(new Thickness(1))
            );
    }
}
