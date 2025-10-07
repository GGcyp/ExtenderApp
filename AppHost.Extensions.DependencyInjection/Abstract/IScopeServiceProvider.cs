

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 表示一个服务提供者接口，它继承自 <see cref="IServiceProvider"/> 和 <see cref="IDisposable"/> 接口，
    /// 并提供了一个名为 ScopeName 的属性。
    /// </summary>
    public interface IScopeServiceProvider : IServiceProvider, IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// 获取作用域选项
        /// </summary>
        ScopeOptions ScopeOptions { get; }
    }
}
