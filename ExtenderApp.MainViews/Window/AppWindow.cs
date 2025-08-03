using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.MainViews
{
    public class AppWindow : Window, IWindow
    {
        private readonly WindowChrome _chrome;

        public AppWindow()
        {
            _chrome = new WindowChrome
            {
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(4),
                CaptionHeight = 40,
                CornerRadius = new CornerRadius(20)
            };
            WindowChrome.SetWindowChrome(this, _chrome);

            RegisterWindowBinding();

            RegisterWindowCommand();
        }

        /// <summary>
        /// 注册窗口绑定。
        /// </summary>
        private void RegisterWindowBinding()
        {
            //将标题栏高度绑定给Chrome
            BindingOperations.SetBinding(_chrome, WindowChrome.CaptionHeightProperty,
                new Binding(CaptionHeightProperty.Name) { Source = this });
        }

        /// <summary>
        /// 注册窗口命令绑定
        /// </summary>
        private void RegisterWindowCommand()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (sender, e) =>
            {
                WindowState = WindowState.Minimized;
            }));

            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (sender, e) =>
            {
                WindowState = WindowState.Maximized;
            }));

            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (sender, e) =>
            {
                WindowState = WindowState.Normal;
            }));

            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (sender, e) =>
            {
                Close();
            }));
        }

        public void ShowView(IView view)
        {
            throw new NotImplementedException();
        }

        public void ShowView(IMainView mainView)
        {
            throw new NotImplementedException();
        }

        public void Enter(ViewInfo oldViewInfo)
        {
            throw new NotImplementedException();
        }

        public void Exit(ViewInfo newViewInfo)
        {
            throw new NotImplementedException();
        }

        #region 系统按钮

        /// <summary>
        /// 系统按钮背景色
        /// </summary>
        public Brush SystemButtonColor
        {
            get { return (Brush)GetValue(SystemButtonColorProperty); }
            set { SetValue(SystemButtonColorProperty, value); }
        }
        public static readonly DependencyProperty SystemButtonColorProperty =
            DependencyProperty.Register("SystemButtonColor",
                typeof(Brush),
                typeof(AppWindow),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0, 255, 255, 255))));


        /// <summary>
        /// 系统按钮大小
        /// </summary>
        public double SystemButtonSize
        {
            get { return (double)GetValue(SystemButtonSizeProperty); }
            set { SetValue(SystemButtonSizeProperty, value); }
        }
        public static readonly DependencyProperty SystemButtonSizeProperty =
            DependencyProperty.Register("SystemButtonSize",
                typeof(double),
                typeof(AppWindow),
                new PropertyMetadata(30.0));

        /// <summary>
        /// 系统按钮前景色
        /// </summary>
        public Brush SystemButtonForeground
        {
            get { return (Brush)GetValue(SystemButtonForegroundProperty); }
            set { SetValue(SystemButtonForegroundProperty, value); }
        }
        public static readonly DependencyProperty SystemButtonForegroundProperty =
            DependencyProperty.Register("SystemButtonForeground",
                typeof(Brush),
                typeof(AppWindow),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(88, 88, 88))));

        /// <summary>
        /// 系统按钮悬浮背景色
        /// </summary>
        public Brush SystemButtonOverColor
        {
            get { return (Brush)GetValue(SystemButtonOverColorProperty); }
            set { SetValue(SystemButtonOverColorProperty, value); }
        }
        public static readonly DependencyProperty SystemButtonOverColorProperty =
            DependencyProperty.Register("SystemButtonOverColor",
                typeof(Brush),
                typeof(AppWindow),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255))));

        /// <summary>
        /// 关闭按钮悬浮背景色
        /// </summary>
        public Brush SystemButtonCloseOverColor
        {
            get { return (Brush)GetValue(SystemButtonCloseOverColorProperty); }
            set { SetValue(SystemButtonCloseOverColorProperty, value); }
        }
        public static readonly DependencyProperty SystemButtonCloseOverColorProperty =
            DependencyProperty.Register("SystemButtonCloseOverColor",
                typeof(Brush),
                typeof(AppWindow),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 0, 0))));

        #endregion

        #region 窗口属性

        /// <summary>
        /// 标题栏高度
        /// </summary>
        public double CaptionHeight
        {
            get { return (double)GetValue(CaptionHeightProperty); }
            set { SetValue(CaptionHeightProperty, value); }
        }
        public static readonly DependencyProperty CaptionHeightProperty =
            DependencyProperty.Register("CaptionHeight",
                typeof(double),
                typeof(AppWindow),
                new PropertyMetadata(30.0));

        /// <summary>
        /// 标题栏背景色
        /// </summary>
        public Brush CaptionBackground
        {
            get { return (Brush)GetValue(CaptionBackgroundProperty); }
            set { SetValue(CaptionBackgroundProperty, value); }
        }
        public static readonly DependencyProperty CaptionBackgroundProperty =
            DependencyProperty.Register("CaptionBackground",
                typeof(Brush),
                typeof(AppWindow),
                new PropertyMetadata(default));

        /// <summary>
        /// 标题栏的内容,可以自定义
        /// </summary>
        public UIElement TitleContent
        {
            get { return (UIElement)GetValue(TitleContentProperty); }
            set { SetValue(TitleContentProperty, value); }
        }
        public static readonly DependencyProperty TitleContentProperty =
            DependencyProperty.Register("TitleContent",
                typeof(UIElement),
                typeof(AppWindow),
                new PropertyMetadata(default));

        /// <summary>
        /// 沉浸式标题栏
        /// </summary>
        public bool FitSystemWindow
        {
            get { return (bool)GetValue(FitSystemWindowProperty); }
            set { SetValue(FitSystemWindowProperty, value); }
        }

        public ViewInfo ViewInfo => throw new NotImplementedException();

        public static readonly DependencyProperty FitSystemWindowProperty =
            DependencyProperty.Register("FitSystemWindow",
                typeof(bool),
                typeof(AppWindow),
                new PropertyMetadata(default));


        #endregion
    }
}
