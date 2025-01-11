

namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 视图模型接口
    /// </summary>
    public interface IViewModel 
    {
        ///// <summary>
        ///// 向视图容器中注入视图
        ///// </summary>
        ///// <param name="view">需要注入的视图</param>
        //public void InjectView(IView view);
    }

    /// <summary>
    /// 视图模型接口
    /// </summary>
    public interface IViewModel<TView> : IViewModel where TView : IView
    {
        /// <summary>
        /// 向视图容器中注入视图
        /// </summary>
        /// <param name="view">需要注入的视图</param>
        public void InjectView(TView view);
    }
}
