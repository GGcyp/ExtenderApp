

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
}
