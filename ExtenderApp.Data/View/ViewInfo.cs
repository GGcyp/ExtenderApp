namespace ExtenderApp.Data
{
    /// <summary>
    /// 包含视图名称和视图模型的结构体。
    /// </summary>
    public struct ViewInfo
    {
        /// <summary>
        /// 获取视图名称。
        /// </summary>
        public string ViewName { get; }

        /// <summary>
        /// 获取视图模型。
        /// </summary>
        public object? ViewModel { get; }

        /// <summary>
        /// 使用视图名称初始化 <see cref="ViewInfo"/> 结构体的新实例。
        /// </summary>
        /// <param name="viewName">视图名称。</param>
        /// <param name="viewModel">视图模型。</param>
        public ViewInfo(string viewName, object? viewModel = null)
        {
            ViewName = viewName;
            ViewModel = viewModel;
        }
    }
}