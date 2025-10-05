
namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 用于安全关闭/释放 <see cref="IServiceProvider"/> 的包装器。
    /// 在不改变原有实现的前提下，按其实际实现（IDisposable/IAsyncDisposable）调用相应释放方法。
    /// </summary>
    /// <remarks>
    /// - 使用 <see cref="WeakReference{T}"/> 避免延长被包装的 IServiceProvider 生命周期。<br/>
    /// - 线程安全，幂等（多次调用只生效一次）。<br/>
    /// - 同步 <see cref="Dispose"/> 在仅实现 <see cref="IAsyncDisposable"/> 的情况下会同步等待 <c>DisposeAsync()</c> 完成。<br/>
    /// - 不假设 <see cref="IServiceProvider"/> 必然实现释放接口，运行时检查后再调用。
    /// </remarks>
    /// <example>
    /// var provider = BuildProvider();
    /// using var closer = new ServiceProviderCloser(provider); // 同步释放
    ///
    /// // 或
    /// await using var asyncCloser = new ServiceProviderCloser(provider); // 异步释放
    /// </example>
    internal class ServiceProviderCloser : IServiceProviderCloser
    {
        /// <summary>
        /// 以弱引用形式保存外部传入的 IServiceProvider，避免强引用导致的生命周期延长。
        /// </summary>
        private readonly WeakReference<IServiceProvider> _serviceProvider;

        /// <summary>
        /// 释放标志：0 = 未释放，1 = 已释放。使用 <see cref="Interlocked.Exchange(ref int, int)"/> 保证线程安全与幂等。
        /// </summary>
        private int _disposed; // 0 = 未释放, 1 = 已释放

        /// <summary>
        /// 使用给定的 <see cref="IServiceProvider"/> 创建关闭器。
        /// </summary>
        /// <param name="services">待关闭/释放的服务提供者实例。</param>
        public ServiceProviderCloser(IServiceProvider services)
        {
            _serviceProvider = new(services);
        }

        /// <summary>
        /// 同步释放：优先调用 <see cref="IDisposable.Dispose"/>；
        /// 若仅实现 <see cref="IAsyncDisposable"/>，则同步等待其 <c>DisposeAsync()</c> 完成。
        /// </summary>
        public void Dispose()
        {
            // 保证只释放一次（线程安全、幂等）
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            if (_serviceProvider.TryGetTarget(out var sp))
            {
                switch (sp)
                {
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        // 同步上下文中也确保释放到位
                        asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        break;
                }
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 异步释放：优先调用 <see cref="IAsyncDisposable.DisposeAsync"/>；
        /// 若仅实现 <see cref="IDisposable"/>，则执行其同步释放。
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            // 保证只释放一次（线程安全、幂等）
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            if (_serviceProvider.TryGetTarget(out var sp))
            {
                if (sp is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (sp is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }
    }
}
