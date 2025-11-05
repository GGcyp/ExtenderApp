

namespace ExtenderApp.Common
{
    /// <summary>
    /// 定义了一个抽象类 ScopeStartup，用于在启动时配置作用域相关的服务和组件。
    /// </summary>
    public abstract class ScopeStartup : Startup
    {
        /// <summary>
        /// 当前作用域的名称
        /// </summary>
        public abstract string ScopeName { get; }
    }
}
