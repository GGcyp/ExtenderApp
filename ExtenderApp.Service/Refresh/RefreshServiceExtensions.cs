using ExtenderApp.Abstract;
using ExtenderApp.Data;

namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 提供 <see cref="IRefreshService"/> 的扩展方法。
    /// </summary>
    public static class RefreshServiceExtensions
    {
        /// <summary>
        /// 为 IRefreshService 服务添加或更新一个动作，类型为更新。
        /// </summary>
        /// <param name="services">IRefreshService 服务实例。</param>
        /// <param name="action">要添加或更新的动作。</param>
        public static void AddUpdate(this IRefreshService services, Action action)
        {
            services.AddAction(action, RefreshType.Update);
        }

        /// <summary>
        /// 为 IRefreshService 服务添加或更新一个动作，类型为修复更新。
        /// </summary>
        /// <param name="services">IRefreshService 服务实例。</param>
        /// <param name="action">要添加或更新的动作。</param>
        public static void AddFixUpdate(this IRefreshService services, Action action)
        {
            services.AddAction(action, RefreshType.FixUpdate);
        }

        /// <summary>
        /// 从 IRefreshService 服务中移除一个更新类型的动作。
        /// </summary>
        /// <param name="services">IRefreshService 服务实例。</param>
        /// <param name="action">要移除的动作。</param>
        public static void RemoveUpdate(this IRefreshService services, Action action)
        {
            services.RemoveAction(action, RefreshType.Update);
        }

        /// <summary>
        /// 从 IRefreshService 服务中移除一个修复更新类型的动作。
        /// </summary>
        /// <param name="services">IRefreshService 服务实例。</param>
        /// <param name="action">要移除的动作。</param>
        public static void RemoveFixUpdate(this IRefreshService services, Action action)
        {
            services.RemoveAction(action, RefreshType.FixUpdate);
        }
    }
}
