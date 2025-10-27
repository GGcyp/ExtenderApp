using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 用于在构建链路客户端时携带插件管理器与依赖注入服务提供器的轻量值类型。
    /// </summary>
    /// <typeparam name="TLinkClient">关联的链路客户端类型，必须实现 <see cref="ILinkClientAwareSender{TLinkClient}"/>。</typeparam>
    /// <remarks>
    /// 本结构体作为构建过程中的短生命周期载体，用以传递：
    /// - 一个 <see cref="IServiceProvider"/>（用于解析插件所需的依赖）；  
    /// - 一个 <see cref="ILinkClientPluginManager{TLinkClient}"/>（实际的插件管理器实例）。
    /// 当任一成员为 null 时，可通过 <see cref="IsEmpty"/> 判断表示未配置插件能力。
    /// </remarks>
    public struct PluginManagerBuilder<TLinkClient>
        where TLinkClient : ILinkClientAwareSender<TLinkClient>
    {
        /// <summary>
        /// 解析插件依赖的服务提供器（可为 null，表示未提供 DI 支持）。
        /// </summary>
        public IServiceProvider Provider { get; }

        /// <summary>
        /// 客户端插件管理器实例（可为 null，表示未设置插件管理器）。
        /// </summary>
        public ILinkClientPluginManager<TLinkClient> Manager { get; }

        /// <summary>
        /// 指示当前构建器是否有效（即 <see cref="Provider"/> 或 <see cref="Manager"/> 任一为 null 时返回 true）。
        /// </summary>
        public bool IsEmpty => Manager is null || Provider is null;

        /// <summary>
        /// 使用给定的服务提供器与插件管理器初始化一个 <see cref="PluginManagerBuilder{TLinkClient}"/> 实例。
        /// </summary>
        /// <param name="provider">用于解析插件依赖的 <see cref="IServiceProvider"/>（可为 null）。</param>
        /// <param name="manager">要使用的 <see cref="ILinkClientPluginManager{TLinkClient}"/> 实例（可为 null）。</param>
        public PluginManagerBuilder(IServiceProvider provider, ILinkClientPluginManager<TLinkClient> manager)
        {
            Provider = provider;
            Manager = manager;
        }
    }
}