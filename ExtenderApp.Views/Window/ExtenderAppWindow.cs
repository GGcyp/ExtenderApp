using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    public class ExtenderAppWindow : Window, IWindow
    {
        private readonly Lazy<FullScreenManager> _fullManagerLazy;
        private readonly IMessageService _messageService;
        public ViewInfo ViewInfo { get; }

        public virtual IView? CurrentView
        {
            get
            {
                if (DataContext is not IWindowViewModel viewModel)
                    return null;
                return viewModel.CurrentView;
            }
        }

        IWindow? IWindow.Owner
        {
            get => Owner as IWindow;
            set => Owner = value as Window;
        }

        public IWindow? Window => throw new NotImplementedException($"无法重复获取Window的Window:{Title}");

        int IWindow.WindowStartupLocation
        {
            get
            {
                return (int)WindowStartupLocation;
            }
            set
            {
                WindowStartupLocation = (WindowStartupLocation)value;
            }
        }

        public bool IsFullScreen => _fullManagerLazy.Value.IsFullScreen;

        protected T? ViewModel<T>() where T : class, IViewModel
            => DataContext as T;

        public ExtenderAppWindow(IMessageService messageService, IViewModel? dataContext = null)
        {
            ViewInfo = new ViewInfo(GetType().Name);
            DataContext = dataContext;
            _fullManagerLazy = new(() => new FullScreenManager(this));
            _messageService = messageService;
        }

        public virtual void Enter(ViewInfo oldViewInfo)
        {
            var viewModel = DataContext as IViewModel;
            viewModel?.InjectView(this);
            viewModel?.Enter(oldViewInfo);
        }

        public virtual void Exit(ViewInfo newViewInfo)
        {
            var viewModel = DataContext as IViewModel;
            viewModel?.Exit(newViewInfo);
        }

        public virtual void ShowView(IView view)
        {
            if (DataContext is not IWindowViewModel viewModel)
                return;

            viewModel.ShowView(view);
        }

        public void InjectWindow(IWindow window)
        {
        }

        public void EnterFullScreen(bool coverTaskbar = false)
        {
            _fullManagerLazy.Value.Enter(coverTaskbar);
        }

        public void ExitFullScreen()
        {
            _fullManagerLazy.Value.Exit();
        }

        public void ToggleFullScreen(bool coverTaskbar = false)
        {
            _fullManagerLazy.Value.Toggle(coverTaskbar);
        }

        #region KeyCapture

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            Data.Key key = (Data.Key)e.Key;
            Data.ModifierKeys modifiers = (Data.ModifierKeys)Keyboard.Modifiers;
            bool isRepeat = e.IsRepeat;
            _messageService.Publish(this, new KeyEvent(key, modifiers, isRepeat, false));

            if (IsTextInputFocused())
                return;

            if (OnGlobalPreviewKeyDown(e))
            {
                e.Handled = true;
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            Data.Key key = (Data.Key)e.Key;
            Data.ModifierKeys modifiers = (Data.ModifierKeys)Keyboard.Modifiers;
            bool isRepeat = e.IsRepeat;
            _messageService.Publish(this, new KeyEvent(key, modifiers, isRepeat, false));

            if (IsTextInputFocused())
                return;

            if (OnGlobalPreviewKeyUp(e))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 全局按键预览按下事件的回调方法，供派生类重写以实现自定义逻辑。
        /// </summary>
        /// <param name="e">按键事件参数。</param>
        /// <returns>如果事件已被处理，则返回 true；否则返回 false。</returns>
        protected virtual bool OnGlobalPreviewKeyDown(KeyEventArgs e)
            => false;

        /// <summary>
        /// 全局按键预览抬起事件的回调方法，供派生类重写以实现自定义逻辑。
        /// </summary>
        /// <param name="e">按键事件参数。</param>
        /// <returns>如果事件已被处理，则返回 true；否则返回 false。</returns>
        protected virtual bool OnGlobalPreviewKeyUp(KeyEventArgs e)
            => false;

        /// <summary>
        /// 检查当前拥有键盘焦点的元素是否为文本输入控件。
        /// </summary>
        /// <returns>如果焦点在文本输入控件上，则返回 true；否则返回 false。</returns>
        /// <remarks>
        /// 此方法用于区分用户是在与全局快捷键交互还是在输入文本。 它会检查焦点元素是否为常见的文本输入控件（如 TextBox, PasswordBox, RichTextBox），
        /// 或者是否为继承自 TextBoxBase 的任何控件的子元素。
        /// </remarks>
        private static bool IsTextInputFocused()
        {
            var focused = Keyboard.FocusedElement as DependencyObject;
            if (focused == null)
                return false;

            // 常见文本输入控件直接判断
            if (focused is System.Windows.Controls.TextBox
                || focused is System.Windows.Controls.PasswordBox
                || focused is System.Windows.Controls.RichTextBox)
                return true;

            // 向上搜索父级，判断是否属于 TextBoxBase（包括一些文本相关控件）
            var current = focused;
            while (current != null)
            {
                if (current is System.Windows.Controls.Primitives.TextBoxBase)
                    return true;
                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        #endregion KeyCapture
    }
}