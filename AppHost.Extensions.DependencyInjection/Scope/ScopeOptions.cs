namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 作用域选项。
    /// </summary>
    /// <remarks>
    /// 用于在创建作用域（Scope）时传递可选的命名信息，便于记录、诊断或区分不同的作用域实例。
    /// 建议通过 <see cref="ScopeOptionsBuilder"/> 构建本类型。
    /// </remarks>
    public class ScopeOptions
    {
        /// <summary>
        /// 作用域名（可选）。
        /// 用于标识或区分子容器/命名 Scope；为 <c>null</c> 表示未命名作用域。
        /// </summary>
        public string? ScopeName { get; }

        /// <summary>
        /// 使用指定的作用域名创建 <see cref="ScopeOptions"/>。
        /// </summary>
        /// <param name="scopeName">作用域名；可为 <c>null</c> 表示未命名。</param>
        internal ScopeOptions(string? scopeName)
        {
            ScopeName = scopeName;
        }
    }
}
