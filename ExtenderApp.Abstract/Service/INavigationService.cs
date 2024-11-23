
namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 导航服务接口
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// 导航到目标视图
        /// </summary>
        /// <param name="targetViewType">目标视图</param>
        /// <param name="oldView">旧试图</param>
        /// <returns>返回目标试图实例</returns>
        IView NavigateTo(Type targetViewType, IView oldView);
    }
}
