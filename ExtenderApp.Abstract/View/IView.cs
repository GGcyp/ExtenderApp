namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 视图接口
    /// </summary>
    public interface IView : IInject<IViewModel>
    {
        /// <summary>
        /// 获取关联的 ViewModel 实例（弱类型）。
        /// </summary>
        /// <returns>如果视图包含 ViewModel 则返回该实例，否则返回 <c>null</c>。</returns>
        object? GetViewModel();

        /// <summary>
        /// 获取关联的强类型 ViewModel 实例。
        /// </summary>
        /// <typeparam name="T">期望的 ViewModel 类型，必须实现 <see cref="IViewModel"/>。</typeparam>
        /// <returns>如果视图的 ViewModel 可以转换为指定类型则返回该实例，否则返回 <c>null</c>。</returns>
        T? GetViewModel<T>() where T : class, IViewModel;
    }
}