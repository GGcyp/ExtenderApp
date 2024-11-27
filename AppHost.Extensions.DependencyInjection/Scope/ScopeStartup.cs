

namespace AppHost.Extensions.DependencyInjection
{
    /// <summary>
    /// 定义了一个抽象类 ScopeStartup，用于在启动时配置作用域相关的服务和组件。
    /// </summary>
    public abstract class ScopeStartup
    {
        /// <summary>
        /// 获取 ScopeStartup 类型的 Type 对象。
        /// </summary>
        private readonly static Type _startupType = typeof(ScopeStartup);

        /// <summary>
        /// 获取 ScopeStartup 类型的 Type 对象。
        /// </summary>
        public static Type SatrtupType => _startupType;

        /// <summary>
        /// 获取作用域的名称。
        /// </summary>
        /// <returns>返回作用域的名称。</returns>
        public abstract string ScopeName { get; }

        /// <summary>
        /// 向服务集合中添加作用域相关的服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        public abstract void AddService(IServiceCollection services);
    }
}
