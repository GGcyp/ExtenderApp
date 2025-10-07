using System.Reflection;

namespace AppHost.Extensions.Hosting
{
    /// <summary>
    /// 提供用于创建主机环境及其相关上下文的静态工厂方法。
    /// </summary>
    public static class HostEnvironmentBuilder
    {
        /// <summary>
        /// 创建一个新的主机环境实例。
        /// </summary>
        /// <returns>返回一个新的 <see cref="IHostEnvironment"/> 实例。</returns>
        public static IHostEnvironment CreateEnvironment()
        {
            return new HostEnvironment(Assembly.GetEntryAssembly()!.FullName!, Directory.GetCurrentDirectory(), string.Empty);
        }

        /// <summary>
        /// 创建主线程上下文访问器实例。
        /// </summary>
        /// <remarks>
        /// - 新实例创建后默认未捕获主线程与同步上下文；需在目标主线程（通常为 WPF UI 线程）的合适时机调用
        ///   <see cref="IMainThreadContext.InitMainThreadContext"/> 完成捕获（建议在 App.OnStartup 中调用）。<br/>
        /// - 完成初始化后，可通过 <see cref="IMainThreadContext.MainThread"/> 与 <see cref="IMainThreadContext.MainThreadContext"/>
        ///   进行线程检查与调度。<br/>
        /// - 该方法仅负责实例化，不涉及 Dispatcher 的创建或安装。
        /// </remarks>
        /// <returns>返回一个新的 <see cref="IMainThreadContext"/> 实例。</returns>
        public static IMainThreadContext CreateMainThreadContext()
        {
            return new MainThreadContext();
        }
    }
}
