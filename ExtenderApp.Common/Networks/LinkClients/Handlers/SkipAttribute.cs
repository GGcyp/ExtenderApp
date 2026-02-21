namespace ExtenderApp.Common.Networks.LinkClients
{
    /// <summary>
    /// 标记处理器方法在未重写时可被管线跳过。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    internal sealed class SkipAttribute : Attribute
    {
        /// <summary>
        /// 获取跳过标记的类型。
        /// </summary>
        public SkipFlags Flags { get; }

        /// <summary>
        /// 初始化 <see cref="SkipAttribute"/> 的新实例。
        /// </summary>
        /// <param name="flags">要应用的跳过标记。</param>
        public SkipAttribute(SkipFlags flags)
        {
            Flags = flags;
        }
    }
}