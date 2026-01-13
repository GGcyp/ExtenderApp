using System.Windows;
using System.Windows.Controls;
using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace ExtenderApp.Views
{
    /// <summary>
    /// 通用视图基类（WPF <see cref="UserControl"/>），用于承载 <see cref="IViewModel"/> 并提供统一的进入/退出与生命周期事件桥接。
    /// <para>
    /// 主要职责：
    /// <list type="bullet">
    /// <item><description>保存并暴露当前视图的 <see cref="ViewInfo"/>（用于导航/切换时传递上下文）。</description></item>
    /// <item><description>在 <see cref="Enter(ViewInfo)"/> / <see cref="Exit(ViewInfo)"/> 时通知对应的 <see cref="IViewModel"/>。</description></item>
    /// <item><description>在 WPF 的 <see cref="FrameworkElement.Loaded"/> / <see cref="FrameworkElement.Unloaded"/> 事件中，调用可重写的 <see cref="OnLoaded"/> / <see cref="OnUnloaded"/> 并转发到 ViewModel。</description></item>
    /// <item><description>在注入窗口（<see cref="InjectWindow(IWindow)"/>）后，监听窗口关闭并触发 <see cref="Exit(ViewInfo)"/>。</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// 约定：
    /// <list type="bullet">
    /// <item><description><see cref="DataContext"/> 应为实现了 <see cref="IViewModel"/> 的实例，否则 Enter/Exit 仅执行视图侧逻辑。</description></item>
    /// <item><description>派生类需要自定义加载/卸载逻辑时，优先重写 <see cref="OnLoaded"/> / <see cref="OnUnloaded"/>，避免直接订阅/取消订阅事件导致重复绑定。</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class ExtenderAppView : UserControl, IView
    {
        /// <summary>
        /// 当前视图的描述信息（通常包含视图名称与 ViewModel 引用），用于导航与视图切换时传递上下文。
        /// </summary>
        public ViewInfo ViewInfo { get; }

        /// <summary>
        /// 当前视图被注入的窗口宿主。
        /// <para>
        /// 当视图作为窗口内容使用时，通过 <see cref="InjectWindow(IWindow)"/> 注入，以便在窗口关闭时触发退出逻辑。
        /// </para>
        /// </summary>
        public IWindow? Window { get; private set; }

        /// <summary>
        /// 尝试从 <see cref="FrameworkElement.DataContext"/> 获取指定类型的 ViewModel。
        /// <para>
        /// 获取失败返回 <see langword="null"/>。
        /// </para>
        /// </summary>
        public T? GetViewModel<T>() where T : class, IViewModel
            => DataContext as T;

        /// <summary>
        /// 初始化视图实例，并绑定 WPF 生命周期事件（Loaded/Unloaded）。
        /// </summary>
        /// <param name="dataContext">可选 ViewModel；若传入则会设置为 <see cref="FrameworkElement.DataContext"/>。</param>
        public ExtenderAppView(IViewModel? dataContext = null)
        {
            ViewInfo = new ViewInfo(GetType().Name, dataContext);
            DataContext = dataContext;
            Loaded += ViewLoaded;
            Unloaded += ViewUnloaded;
        }

        /// <summary>
        /// 注入窗口宿主，并在窗口关闭时触发退出流程。
        /// <para>
        /// 通常由导航/窗口管理器在创建窗口后调用。
        /// </para>
        /// </summary>
        /// <param name="window">窗口宿主。</param>
        public virtual void InjectWindow(IWindow window)
        {
            Window = window;
            Window.Closed += (s, e) =>
            {
                Exit(new());
            };
        }

        /// <summary>
        /// 进入当前视图。
        /// <para>
        /// 会将当前视图注入到 ViewModel（<see cref="IViewModel.InjectView(IView)"/>），并调用 ViewModel.Enter 以便实现导航初始化。
        /// </para>
        /// </summary>
        /// <param name="oldViewInfo">前一个视图的信息。</param>
        public virtual void Enter(ViewInfo oldViewInfo)
        {
            var viewModel = DataContext as IViewModel;
            viewModel?.InjectView(this);
            viewModel?.Enter(oldViewInfo);
        }

        /// <summary>
        /// 退出当前视图。
        /// <para>
        /// 由导航切换或窗口关闭时触发，转发到 ViewModel.Exit 以便释放资源、提交状态等。
        /// </para>
        /// </summary>
        /// <param name="newViewInfo">将要进入的下一个视图的信息。</param>
        public virtual void Exit(ViewInfo newViewInfo)
        {
            var viewModel = DataContext as IViewModel;
            viewModel?.Exit(newViewInfo);
        }

        /// <summary>
        /// 视图加载事件处理方法。
        /// <para>
        /// 当控件加载到可视树时触发：
        /// <list type="number">
        /// <item><description>先调用 <see cref="OnLoaded"/>（供派生类扩展）。</description></item>
        /// <item><description>再调用 ViewModel 的 <c>OnViewloaded</c>（用于初始化数据、注册事件等）。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as IViewModel;
            OnLoaded();
            viewModel?.OnViewloaded();
        }

        /// <summary>
        /// 视图加载时的扩展点。
        /// <para>
        /// 派生类可重写此方法以添加自定义加载逻辑（初始化控件、绑定命令、注册事件等）。
        /// </para>
        /// </summary>
        protected virtual void OnLoaded()
        {
        }

        /// <summary>
        /// 视图卸载事件处理方法。
        /// <para>
        /// 当控件从可视树移除时触发：
        /// <list type="number">
        /// <item><description>先调用 <see cref="OnUnloaded"/>（供派生类扩展）。</description></item>
        /// <item><description>再调用 ViewModel 的 <c>OnViewUnloaded</c>（用于释放资源、注销事件等）。</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void ViewUnloaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as IViewModel;
            OnUnloaded();
            viewModel?.OnViewUnloaded();
        }

        /// <summary>
        /// 视图卸载时的扩展点。
        /// <para>
        /// 派生类可重写此方法以添加自定义卸载逻辑（释放资源、注销事件等）。
        /// </para>
        /// </summary>
        protected virtual void OnUnloaded()
        {
        }
    }
}