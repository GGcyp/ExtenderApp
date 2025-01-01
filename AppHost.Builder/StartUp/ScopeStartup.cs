using AppHost.Extensions.DependencyInjection;

namespace AppHost.Builder
{
    /// <summary>
    /// 定义了一个抽象类 ScopeStartup，用于在启动时配置作用域相关的服务和组件。
    /// </summary>
    public abstract class ScopeStartup : Startup
    {
        public sealed override void Start(IHostApplicationBuilder builder)
        {

        }

        /// <summary>
        /// 配置作用域选项。
        /// </summary>
        /// <param name="options">作用域选项实例。</param>
        public abstract void ConfigureScopeOptions(ScopeOptions options);
    }
}
