namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="ScopeOptions"/> 构建器。
    /// </summary>
    /// <remarks>
    /// 通过流式设置选项后调用 <see cref="Build"/> 生成不可变的 <see cref="ScopeOptions"/> 实例。
    /// </remarks>
    public class ScopeOptionsBuilder
    {
        /// <summary>
        /// 要设置的作用域名（可选）。
        /// </summary>
        public string? ScopeName { get; set; }

        /// <summary>
        /// 构建并返回 <see cref="ScopeOptions"/> 实例。
        /// </summary>
        /// <returns>生成的 <see cref="ScopeOptions"/>。</returns>
        /// <example>
        /// var options = new ScopeOptionsBuilder { ScopeName = "MyScope" }.Build();
        /// </example>
        public ScopeOptions Build()
        {
            // 使用内部构造函数创建不可变选项对象
            return new ScopeOptions(ScopeName);
        }

        /// <summary>
        /// 重置构建器状态。
        /// </summary>
        internal void Rest()
        {
            ScopeName = string.Empty;
        }
    }
}
