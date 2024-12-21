

using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 刷新服务接口
    /// </summary>
    public interface IRefreshService
    {
        /// <summary>
        /// 添加刷新动作
        /// </summary>
        /// <param name="action">要添加的动作</param>
        /// <param name="type">刷新类型</param>
        void AddAction(Action action, RefreshType type);

        /// <summary>
        /// 移除刷新动作
        /// </summary>
        /// <param name="action">要移除的动作</param>
        /// <param name="type">刷新类型</param>
        void RemoveAction(Action action, RefreshType type);
    }
}
