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
        /// 获取日志服务
        /// </summary>
        ILogingService LogingService { get; }

        /// <summary>
        /// 获取网络服务
        /// </summary>
        INetWorkService NetWorkService { get; }

        /// <summary>
        /// 获取模组服务
        /// </summary>
        IModService ModService { get; }
    }
}
