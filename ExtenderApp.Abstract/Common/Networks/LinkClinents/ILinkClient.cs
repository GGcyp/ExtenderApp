namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端链路的基础接口。
    /// <para>定义了客户端最核心的资源释放行为以及帧器（Framer）的管理能力。</para>
    /// </summary>
    public interface ILinkClient : IDisposable, ILinkInfo
    {
    }

    /// <summary>
    /// 表示一个支持插件化管理的客户端链路实体。
    /// <para>通过泛型约束确保插件管理器与具体的客户端实现类型匹配，从而提供更强的类型安全。</para>
    /// </summary>
    /// <typeparam name="TLinkClient">具体的客户端实现类型，必须继承自 <see cref="ILinkClient"/>。</typeparam>
    public interface ILinkClient<TLinkClient> : ILinkClient
        where TLinkClient : ILinkClient
    {
    }
}