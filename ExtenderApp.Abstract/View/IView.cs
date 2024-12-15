using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 视图接口
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// 获取当前视图信息。
        /// </summary>
        ViewInfo ViewInfo { get; }

        /// <summary>
        /// 进入新视图时执行的操作。
        /// </summary>
        /// <param name="oldViewInfo">旧的视图信息。</param>
        void Enter(ViewInfo oldViewInfo);

        /// <summary>
        /// 退出当前视图时执行的操作。
        /// </summary>
        /// <param name="newViewInfo">新的视图信息。</param>
        void Exit(ViewInfo newViewInfo);
    }
}
