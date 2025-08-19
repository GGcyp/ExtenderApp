

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 导航服务接口
    /// </summary>
    public interface INavigationService
    {
        ///// <summary>
        ///// 导航到目标视图
        ///// </summary>
        ///// <param name="targetViewType">目标视图</param>
        ///// <param name="oldView">旧试图</param>
        ///// <returns>返回目标试图实例</returns>
        //IView NavigateTo(Type targetViewType, IView oldView);

        /// <summary>
        /// 导航到指定的视图类型。
        /// </summary>
        /// <param name="targetViewType">目标视图类型。</param>
        /// <param name="scope">导航的范围。</param>
        /// <param name="oldView">旧的视图。</param>
        /// <returns>导航后的视图。</returns>
        IView NavigateTo(Type targetViewType, string scope, IView? oldView);

        /// <summary>
        /// 导航到指定类型的窗口
        /// </summary>
        /// <param name="targetViewType">要导航到的目标视图类型，必须实现IView接口</param>
        /// <param name="scope">导航作用域标识，用于区分不同导航场景</param>
        /// <param name="oldView">当前正在显示的视图实例，可为null</param>
        /// <returns>返回新创建的窗口实例</returns>
        IWindow NavigateToWindow(Type targetViewType, string scope, IView? oldView);
    }
}
