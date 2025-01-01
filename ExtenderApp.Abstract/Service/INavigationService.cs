

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
    }
}
