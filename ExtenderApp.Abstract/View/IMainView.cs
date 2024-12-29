
namespace ExtenderApp.Abstract
{
    /// <summary>
    /// IMainView接口
    /// </summary>
    public interface IMainView : IView
    {
        /// <summary>
        /// 显示视图
        /// </summary>
        /// <param name="view">要显示的视图</param>
        void ShowView(IView view);
    }
}
