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
        /// 获取当前视图的窗口对象。 获取的窗口不包含主窗口，只能是新建的窗口
        /// </summary>
        /// <returns>返回窗口对象，如果窗口不存在则返回null。</returns>
        IWindow? Window { get; }

        /// <summary>
        /// 注入窗口
        /// </summary>
        /// <param name="window">需要注入的窗口对象</param>
        void InjectWindow(IWindow window);

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