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

        /// <summary>
        /// 获取临时存储服务
        /// </summary>
        ITemporarilyService TemporarilyService { get; }

        /// <summary>
        /// 获取刷新服务接口。
        /// </summary>
        /// <value>
        /// 返回刷新服务接口的实例。
        /// </value>
        IRefreshService RefreshService { get; }

        /// <summary>
        /// 获取日志服务。
        /// </summary>
        /// <returns>返回日志服务接口。</returns>
        ILogingService LoggingService { get; }
    }
}
