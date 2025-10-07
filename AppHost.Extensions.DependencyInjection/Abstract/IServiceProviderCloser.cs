namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 定义用于关闭/释放 <see cref="IServiceProvider"/> 的抽象接口。
    /// 统一提供同步与异步释放入口。
    /// </summary>
    /// <remarks>
    /// 实现需满足：
    /// - 线程安全与幂等（多次调用只释放一次）；<br/>
    /// - 根据被包装对象的实际实现选择调用 <see cref="IDisposable"/> 或 <see cref="IAsyncDisposable"/>。
    /// </remarks>
    public interface IServiceProviderCloser : IDisposable, IAsyncDisposable
    {
    }
}
