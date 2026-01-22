namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 导航服务接口
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// 当前视图接口
        /// </summary>
        IView? CurrentView { get; }

        /// <summary>
        /// 导航到指定视图
        /// </summary>
        /// <param name="view">指定视图类型</param>
        /// <param name="scope">作用域</param>
        void NavigateTo(Type view, string scope);

        /// <summary>
        /// 当前视图更改时触发的事件
        /// </summary>
        event EventHandler? CurrentViewChanged;
    }
}