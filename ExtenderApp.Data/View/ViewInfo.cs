
namespace ExtenderApp.Data
{
    /// <summary>
    /// 表示视图信息的结构体。
    /// </summary>
    public struct ViewInfo
    {
        /// <summary>
        /// 获取视图的哈希码。
        /// </summary>
        public int ViewHashCode { get; }

        /// <summary>
        /// 使用视图名称初始化 <see cref="ViewInfo"/> 结构体的新实例。
        /// </summary>
        /// <param name="viewName">视图名称。</param>
        public ViewInfo(string viewName) : this(viewName.GetHashCode())
        {

        }

        /// <summary>
        /// 使用视图名称初始化 <see cref="ViewInfo"/> 结构体的新实例。
        /// </summary>
        /// <param name="viewName">视图名称哈希值。</param>
        public ViewInfo(int viewHashCode)
        {
            ViewHashCode = viewHashCode;
        }
    }
}
