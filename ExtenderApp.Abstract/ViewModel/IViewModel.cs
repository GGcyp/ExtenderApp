using ExtenderApp.Data;

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 视图模型接口
    /// </summary>
    public interface IViewModel
    {
        /// <summary>
        /// 将视图注入到视图模型中
        /// </summary>
        /// <param name="viewModel">视图对象</param>
        void InjectView(IView view);

        /// <summary>
        /// 进入视图时调用此方法。
        /// </summary>
        /// <param name="oldViewInfo">旧的视图信息</param>
        void Enter(ViewInfo oldViewInfo);

        /// <summary>
        /// 退出视图时调用此方法。
        /// </summary>
        /// <param name="newViewInfo">新的视图信息</param>
        void Exit(ViewInfo newViewInfo);
    }
}