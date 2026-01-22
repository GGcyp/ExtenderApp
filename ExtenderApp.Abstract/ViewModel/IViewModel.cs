namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 视图模型接口
    /// </summary>
    public interface IViewModel
    {
        /// <summary>
        /// 将服务提供器注入到视图模型中
        /// </summary>
        /// <param name="serviceProvider">服务提供器实例</param>
        void Inject(IServiceProvider serviceProvider);
    }
}