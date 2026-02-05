namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端链路的基础接口。
    /// <para>定义了客户端最核心的资源释放行为以及帧器（Framer）的管理能力。</para>
    /// </summary>
    public interface ILinkClient : IDisposable, ILinkInfo, ILinkClientMagic
    {
        /// <summary>
        /// 为客户端配置帧器实例。
        /// </summary>
        /// <param name="framer">要设置的 <see cref="ILinkClientFramer"/> 实例；通常不得为 <c>null</c>。</param>
        void SetClientFramer(ILinkClientFramer framer);
    }

    /// <summary>
    /// 表示一个支持插件化管理的客户端链路实体。
    /// <para>通过泛型约束确保插件管理器与具体的客户端实现类型匹配，从而提供更强的类型安全。</para>
    /// </summary>
    /// <typeparam name="TLinkClient">具体的客户端实现类型，必须继承自 <see cref="ILinkClient"/>。</typeparam>
    public interface ILinkClient<TLinkClient> : ILinkClient
        where TLinkClient : ILinkClient
    {
        /// <summary>
        /// 获取当前客户端挂载的插件管理器。
        /// </summary>
        /// <value>返回 <see cref="ILinkClientPluginManager{TLinkClient}"/> 实例；未挂载时返回 <c>null</c>。</value>
        ILinkClientPluginManager<TLinkClient>? PluginManager { get; }

        /// <summary>
        /// 设置并启用客户端的插件管理器。
        /// </summary>
        /// <param name="pluginManager">要设置的插件管理器实例；通常由 DI 容器或初始化程序提供。</param>
        void SetClientPluginManager(ILinkClientPluginManager<TLinkClient> pluginManager);
    }
}