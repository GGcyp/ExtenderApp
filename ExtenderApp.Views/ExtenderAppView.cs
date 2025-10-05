using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// ExtenderAppView 类是一个用户控件类，实现了 IView 接口。
    /// </summary>
    public class ExtenderAppView : UserControl, IView
    {
        /// <summary>
        /// 获取当前视图的视图信息。
        /// </summary>
        /// <returns>返回当前视图的视图信息。</returns>
        public ViewInfo ViewInfo { get; }

        public IWindow? Window { get; private set; }

        public T? ViewModel<T>() where T : class, IViewModel
            => DataContext as T;

        public ExtenderAppView(IViewModel? dataContext = null)
        {
            ViewInfo = new ViewInfo(GetType().Name, dataContext);
            DataContext = dataContext;
            Loaded += ViewLoaded;
            Unloaded += ViewUnloaded;
        }

        public virtual void InjectWindow(IWindow window)
        {
            Window = window;
            Window.Closed += (s, e) =>
            {
                Exit(new());
            };
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

        /// <summary>
        /// 视图加载事件处理方法。
        /// 当控件加载到可视树时触发，先调用 OnLoaded（可供派生类重写），
        /// 再调用视图模型的 OnViewloaded 方法，通常用于初始化数据、注册事件等。
        /// </summary>
        /// <param name="sender">事件源对象。</param>
        /// <param name="e">事件参数。</param>
        private void ViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as IViewModel;
            OnLoaded();
            viewModel?.OnViewloaded();
        }

        /// <summary>
        /// 视图加载时的虚方法。
        /// 可在派生类中重写以添加自定义加载逻辑，如初始化控件、资源等。
        /// </summary>
        protected virtual void OnLoaded()
        {
            // 可在派生类中重写以添加自定义加载逻辑
        }

        /// <summary>
        /// 视图卸载事件处理方法。
        /// 当控件从可视树移除时触发，调用视图模型的 OnViewUnloaded 方法，通常用于释放资源、注销事件等。
        /// </summary>
        /// <param name="sender">事件源对象。</param>
        /// <param name="e">事件参数。</param>
        private void ViewUnloaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as IViewModel;
            OnUnloaded();
            viewModel?.OnViewUnloaded();
        }

        /// <summary>
        /// 视图卸载时的虚方法。
        /// 可在派生类中重写以添加自定义卸载逻辑，如释放资源、注销事件等。
        /// </summary>
        protected virtual void OnUnloaded()
        {
            // 可在派生类中重写以添加自定义卸载逻辑
        }

        ///// <summary>
        ///// 向当前视图添加一个键盘输入绑定，将指定的命令与按键及修饰键关联。
        ///// </summary>
        ///// <param name="command">要绑定的命令，通常为视图模型中的 ICommand 实例。</param>
        ///// <param name="key">触发命令的主按键。</param>
        ///// <param name="modifierKeys">触发命令时需要的修饰键（如 Ctrl、Alt、Shift）。</param>
        //protected void AddKey(ICommand command, Key key, ModifierKeys modifierKeys)
        //{
        //    this.InputBindings.Add(new KeyBinding(
        //        command,        // 绑定到 ViewModel 的命令
        //        key,            // 按键
        //        modifierKeys    // 修饰键
        //    ));
        //}

        ///// <summary>
        ///// 向当前视图添加一个键盘输入绑定，将指定的命令与 KeyGesture（按键+修饰键组合）关联。
        ///// </summary>
        ///// <param name="command">要绑定的命令，通常为视图模型中的 ICommand 实例。</param>
        ///// <param name="keyGesture">包含按键和修饰键的组合手势。</param>
        //protected void AddKey(ICommand command, KeyGesture keyGesture)
        //{
        //    this.InputBindings.Add(new KeyBinding(
        //        command,        // 绑定到 ViewModel 的命令
        //        keyGesture      // 按键和修饰键
        //    ));
        //}

        ///// <summary>
        ///// 向当前视图添加一个键盘输入绑定，将指定的命令与单一按键关联，无需修饰键。
        ///// </summary>
        ///// <param name="command">要绑定的命令，通常为视图模型中的 ICommand 实例。</param>
        ///// <param name="key">触发命令的主按键。</param>
        //protected void AddKey(ICommand command, Key key)
        //{
        //    AddKey(command, key, ModifierKeys.None);
        //}
    }
}
