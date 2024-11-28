namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// ScopeServiceCollectionFactory 类提供创建 ScopeServiceCollection 实例的静态方法。
    /// </summary>
    public static class ScopeServiceCollectionFactory
    {
        /// <summary>
        /// 创建一个新的 ScopeServiceCollection 实例。
        /// </summary>
        /// <returns>返回一个新的 ScopeServiceCollection 实例。</returns>
        public static ScopeServiceCollection CreateScopeServiceCollection()
        {
            return new ScopeServiceCollection();
        }
    }
}
