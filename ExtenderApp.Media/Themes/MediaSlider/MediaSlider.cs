using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace ExtenderApp.Media.Themes
{
    /// <summary>
    /// 可定制的媒体进度条控件，继承自 <see cref="Slider"/>，提供悬停放大、样式化-thumb、以及拖拽/点击命令支持。
    /// </summary>
    public class MediaSlider : Slider
    {
        static MediaSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaSlider),
                new FrameworkPropertyMetadata(typeof(MediaSlider)));
        }

        #region Properties

        /// <summary>
        /// 鼠标进入控件时使用的 TrackHeight（值为 0 表示不启用悬停高度）。
        /// </summary>
        public double HoverTrackHeight
        {
            get => (double)GetValue(HoverTrackHeightProperty);
            set => SetValue(HoverTrackHeightProperty, value);
        }

        /// <summary>
        /// 注册 <see cref="HoverTrackHeight"/> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty HoverTrackHeightProperty =
            DependencyProperty.Register(nameof(HoverTrackHeight),
                typeof(double),
                typeof(MediaSlider),
                new PropertyMetadata(0d));

        private double _normalTrackHeight;

        /// <summary>
        /// Thumb 的尺寸（像素），用于模板中绑定 thumb 大小。
        /// </summary>
        public double ThumbSize
        {
            get => (double)GetValue(ThumbSizeProperty);
            set => SetValue(ThumbSizeProperty, value);
        }

        /// <summary>
        /// 注册 <see cref="ThumbSize"/> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(nameof(ThumbSize),
                typeof(double),
                typeof(MediaSlider),
                new PropertyMetadata(10d));

        /// <summary>
        /// 滑块（整个轨道）背景画刷（可用于模板绑定）。
        /// </summary>
        public Brush TrackBackground
        {
            get => (Brush)GetValue(TrackBackgroundProperty);
            set => SetValue(TrackBackgroundProperty, value);
        }

        /// <summary>
        /// 注册 <see cref="TrackBackground"/> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty TrackBackgroundProperty =
            DependencyProperty.Register(nameof(TrackBackground),
                typeof(Brush),
                typeof(MediaSlider),
                new PropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// 已播放区域的背景画刷（左侧）。
        /// </summary>
        public Brush LeftBackground
        {
            get { return (Brush)GetValue(LeftBackgroundProperty); }
            set { SetValue(LeftBackgroundProperty, value); }
        }

        /// <summary>
        /// 注册 <see cref="LeftBackground"/> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty LeftBackgroundProperty =
            DependencyProperty.Register(nameof(LeftBackground),
                typeof(Brush),
                typeof(MediaSlider),
                new PropertyMetadata(null));

        /// <summary>
        /// 未播放区域的背景画刷（右侧）。
        /// </summary>
        public Brush RightBackground
        {
            get { return (Brush)GetValue(RightBackgroundProperty); }
            set { SetValue(RightBackgroundProperty, value); }
        }

        /// <summary>
        /// 注册 <see cref="RightBackground"/> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty RightBackgroundProperty =
            DependencyProperty.Register(nameof(RightBackground),
                typeof(Brush),
                typeof(MediaSlider),
                new PropertyMetadata(null));

        /// <summary>
        /// Thumb 的填充颜色（用于模板中 Thumb 的 Fill）。
        /// </summary>
        public Brush ThumbColor
        {
            get { return (Brush)GetValue(ThumbColorProperty); }
            set { SetValue(ThumbColorProperty, value); }
        }

        /// <summary>
        /// 注册 <see cref="ThumbColor"/> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty ThumbColorProperty =
            DependencyProperty.Register(nameof(ThumbColor),
                typeof(Brush),
                typeof(MediaSlider),
                new PropertyMetadata(null));

        /// <summary>
        /// 轨道高度（像素），用于模板绑定 Track 的高度。
        /// </summary>
        public double TrackHeight
        {
            get => (double)GetValue(TrackHeightProperty);
            set => SetValue(TrackHeightProperty, value);
        }

        /// <summary>
        /// 注册 <see cref="TrackHeight"/> 依赖属性。
        /// </summary>
        public static readonly DependencyProperty TrackHeightProperty =
            DependencyProperty.Register(nameof(TrackHeight),
                typeof(double),
                typeof(MediaSlider),
                new PropertyMetadata(5d));

        #endregion Properties

        #region Command

        /// <summary>
        /// 拖拽开始时要执行的命令（CommandParameter 为当前 Value）。
        /// </summary>
        public static readonly DependencyProperty DragStartedCommandProperty =
            DependencyProperty.Register(nameof(DragStartedCommand),
                typeof(ICommand),
                typeof(MediaSlider),
                new PropertyMetadata(null));

        /// <summary>
        /// 绑定用于处理拖拽开始的命令。
        /// </summary>
        public ICommand? DragStartedCommand
        {
            get => (ICommand?)GetValue(DragStartedCommandProperty);
            set => SetValue(DragStartedCommandProperty, value);
        }

        /// <summary>
        /// 拖拽过程中要执行的命令（CommandParameter 为当前 Value）。
        /// </summary>
        public static readonly DependencyProperty DragDeltaCommandProperty =
            DependencyProperty.Register(nameof(DragDeltaCommand),
                typeof(ICommand),
                typeof(MediaSlider),
                new PropertyMetadata(null));

        /// <summary>
        /// 绑定用于处理拖拽过程的命令。
        /// </summary>
        public ICommand? DragDeltaCommand
        {
            get => (ICommand?)GetValue(DragDeltaCommandProperty);
            set => SetValue(DragDeltaCommandProperty, value);
        }

        /// <summary>
        /// 拖拽完成时要执行的命令（CommandParameter 为当前 Value）。
        /// </summary>
        public static readonly DependencyProperty DragCompletedCommandProperty =
            DependencyProperty.Register(nameof(DragCompletedCommand),
                typeof(ICommand),
                typeof(MediaSlider),
                new PropertyMetadata(null));

        /// <summary>
        /// 绑定用于处理拖拽完成的命令。
        /// </summary>
        public ICommand? DragCompletedCommand
        {
            get => (ICommand?)GetValue(DragCompletedCommandProperty);
            set => SetValue(DragCompletedCommandProperty, value);
        }

        /// <summary>
        /// 点击轨道时要执行的命令（CommandParameter 为当前 Value）。
        /// </summary>
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(nameof(ClickCommand),
                typeof(ICommand),
                typeof(MediaSlider),
                new PropertyMetadata(null));

        /// <summary>
        /// 绑定用于处理点击定位（Seek）的命令。
        /// </summary>
        public ICommand? ClickCommand
        {
            get => (ICommand?)GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
        }

        #endregion Command

        /// <summary>
        /// 创建一个 <see cref="MediaSlider"/> 实例并启用 MoveToPoint。
        /// </summary>
        public MediaSlider()
        {
            IsMoveToPointEnabled = true;
            _normalTrackHeight = TrackHeight;
        }

        /// <summary>
        /// 鼠标进入时将轨道高度切换为 <see cref="HoverTrackHeight"/>（当其大于 0 时）。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            _normalTrackHeight = TrackHeight;

            if (HoverTrackHeight > 0 && TrackHeight != HoverTrackHeight)
            {
                TrackHeight = HoverTrackHeight;
            }
        }

        /// <summary>
        /// 鼠标离开时恢复先前记录的轨道高度。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (HoverTrackHeight > 0 && TrackHeight != _normalTrackHeight)
            {
                TrackHeight = _normalTrackHeight;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            var point = e.GetPosition(this);
            var newValue = Minimum + (Maximum - Minimum) * (point.X / ActualWidth);
            Value = newValue;

            CallCommand(ClickCommand);
        }

        /// <summary>
        /// 重写：thumb 拖拽完成时触发事件并执行 <see cref="DragCompletedCommand"/>（若绑定）。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
            CallCommand(DragCompletedCommand);
        }

        /// <summary>
        /// 重写：thumb 开始拖拽时执行 <see cref="DragStartedCommand"/>（若绑定）。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            CallCommand(DragStartedCommand);
        }

        /// <summary>
        /// 重写：thumb 拖拽过程中执行 <see cref="DragDeltaCommand"/>（若绑定）。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            CallCommand(DragDeltaCommand);
        }

        /// <summary>
        /// 将指定命令与当前 Value 一起调用。
        /// </summary>
        /// <param name="cmd">指定命令</param>
        private void CallCommand(ICommand? cmd)
        {
            if (cmd == null)
                return;
            if (cmd is RelayCommand<double> dCommand && dCommand.CanExecute(Value))
            {
                dCommand.Execute(Value);
            }
            else if (cmd.CanExecute(null))
            {
                cmd.Execute(Value);
            }
        }
    }
}