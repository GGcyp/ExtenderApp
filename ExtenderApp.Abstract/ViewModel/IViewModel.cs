

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

        /// <summary>
        /// 关闭资源。
        /// </summary>
        void Close();
    }

    /// <summary>
    /// 视图模型接口
    /// </summary>
    public interface IViewModel<TView> : IViewModel where TView : class, IView
    {
        /// <summary>
        /// 向视图容器中注入视图
        /// </summary>
        /// <param name="view">需要注入的视图</param>
        void InjectView(TView view);
    }

    /// <summary>
    /// 表示一个窗口视图模型接口，继承自 IViewModel 接口。
    /// </summary>
    /// <typeparam name="TView">视图类型，必须实现 IView 接口。</typeparam>
    public interface IWindowViewModel : IViewModel
    {
        /// <summary>
        /// 获取当前视图接口。
        /// </summary>
        /// <value>返回当前视图接口。</value>
        IView? CurrentView { get; }

        /// <summary>
        /// 显示视图。
        /// </summary>
        /// <param name="view">要显示的视图。</param>
        void ShowView(IView view);
    }
}
