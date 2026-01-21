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
        /// 尝试从 <see cref="FrameworkElement.DataContext"/> 获取指定类型的 ViewModel。
        /// <para>
        /// 获取失败返回 <see langword="null"/>。
        /// </para>
        /// </summary>
        public T? GetViewModel<T>() where T : class, IViewModel
            => DataContext as T;

        public object? GetViewModel()
            => DataContext;

        /// <summary>
        /// 初始化视图实例，并绑定 WPF 生命周期事件（Loaded/Unloaded）。
        /// </summary>
        /// <param name="dataContext">可选 ViewModel；若传入则会设置为 <see cref="FrameworkElement.DataContext"/>。</param>
        public ExtenderAppView()
        {
        }

        public void InjectViewModel(IViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}