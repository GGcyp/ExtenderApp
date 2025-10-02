using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ExtenderApp.Media.Themes
{
    internal class MediaSlider : Slider
    {
        static MediaSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaSlider),
                new FrameworkPropertyMetadata(typeof(MediaSlider)));
        }

        public event Action? ThumbDragCompleted;

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
            ThumbDragCompleted?.Invoke();
        }

        ///// <summary>
        ///// 图标
        ///// </summary>
        //public Geometry Icon
        //{
        //    get => (Geometry)GetValue(IconProperty);
        //    set => SetValue(IconProperty, value);
        //}

        //public static readonly DependencyProperty IconProperty =
        //    DependencyProperty.Register(nameof(Icon), 
        //        typeof(Geometry), 
        //        typeof(MediaSlider), 
        //        new PropertyMetadata(null));


        /// <summary>
        /// 图标尺寸
        /// </summary>
        public double ThumbSize
        {
            get => (double)GetValue(ThumbSizeProperty);
            set => SetValue(ThumbSizeProperty, value);
        }

        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(nameof(ThumbSize),
                typeof(double),
                typeof(MediaSlider),
                new PropertyMetadata(10d));


        /// <summary>
        /// 滑块背景颜色
        /// </summary>
        public Brush TrackBackground
        {
            get => (Brush)GetValue(TrackBackgroundProperty);
            set => SetValue(TrackBackgroundProperty, value);
        }

        public static readonly DependencyProperty TrackBackgroundProperty =
            DependencyProperty.Register(nameof(TrackBackground),
                typeof(Brush),
                typeof(MediaSlider),
                new PropertyMetadata(Brushes.Transparent));


        /// <summary>
        /// 已看完显示颜色
        /// </summary>
        public Brush LeftBackground
        {
            get { return (Brush)GetValue(LeftBackgroundProperty); }
            set { SetValue(LeftBackgroundProperty, value); }
        }

        public static readonly DependencyProperty LeftBackgroundProperty =
            DependencyProperty.Register(nameof(LeftBackground),
                typeof(Brush),
                typeof(MediaSlider),
                new PropertyMetadata(null));


        /// <summary>
        /// 还没看的视频长度背景色
        /// </summary>
        public Brush RightBackground
        {
            get { return (Brush)GetValue(RightBackgroundProperty); }
            set { SetValue(RightBackgroundProperty, value); }
        }

        public static readonly DependencyProperty RightBackgroundProperty =
            DependencyProperty.Register(nameof(RightBackground),
                typeof(Brush),
                typeof(MediaSlider),
                new PropertyMetadata(null));




        /// <summary>
        /// 圆块的颜色
        /// </summary>
        public Brush ThumbColor
        {
            get { return (Brush)GetValue(ThumbColorProperty); }
            set { SetValue(ThumbColorProperty, value); }
        }

        public static readonly DependencyProperty ThumbColorProperty =
            DependencyProperty.Register(nameof(ThumbColor),
                typeof(Brush),
                typeof(MediaSlider),
                new PropertyMetadata(null));



        /// <summary>
        /// 滑动条高度
        /// </summary>
        public double TrackHeight
        {
            get => (double)GetValue(TrackHeightProperty);
            set => SetValue(TrackHeightProperty, value);
        }

        public static readonly DependencyProperty TrackHeightProperty =
            DependencyProperty.Register(nameof(TrackHeight),
                typeof(double),
                typeof(MediaSlider),
                new PropertyMetadata(5d));

        public MediaSlider()
        {
            IsMoveToPointEnabled = true;
            MouseLeftButtonUp += MediaSlider_MouseLeftButtonDown;
        }

        private void MediaSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(this);
            var newValue = Minimum + (Maximum - Minimum) * (point.X / ActualWidth);
            Value = newValue;
            ThumbDragCompleted?.Invoke();
        }
    }
}
