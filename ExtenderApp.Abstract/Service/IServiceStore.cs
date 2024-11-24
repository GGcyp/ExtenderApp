namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 服务仓库
    /// </summary>
    public interface IServiceStore
    {
        /// <summary>
        /// UI线程通讯服务
        /// </summary>
        IDispatcherService DispatcherService { get; }

        /// <summary>
        /// 显示导航服务
        /// </summary>
        INavigationService NavigationService { get; }
    }
}
