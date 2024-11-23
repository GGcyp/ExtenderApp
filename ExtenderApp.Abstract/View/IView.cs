namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 视图接口
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// 视图模型
        /// </summary>
        IViewModel ViewModel { get; }

        /// <summary>
        /// 进入视图
        /// </summary>
        /// <param name="oldView">将要离开的视图</param>
        void Enter(IView oldView);

        /// <summary>
        /// 离开视图
        /// </summary>
        /// <param name="newView">将要进入的视图</param>
        void Exit(IView newView);
    }
}
