using System.Net.Sockets;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// 链路客户端构建器基类。
    /// 提供用于配置并创建 <typeparamref name="TLinkClient"/> 实例的便捷方法：
    /// - 通过 <see cref="ClientFactory"/> 创建客户端实例；
    /// - 可选地设置 <see cref="FormatterManager"/> 与 <see cref="PluginManager"/> 并将其注入到目标客户端。
    /// </summary>
    /// <typeparam name="TLinkClient">要构建的链路客户端类型，必须实现 <see cref="ILinkClientAwareSender{TLinkClient}"/>。</typeparam>
    /// <remarks>
    /// - 本构建器以“先配置后构建”的方式工作；通常在单线程/启动时使用。若在并发环境中共享使用，应由调用方负责同步。  
    /// - 构建出的客户端不会被本类释放或管理；调用方对客户端的生命周期负责。  
    /// - FormatterManager 与 PluginManager 为可选依赖：若需要按类型序列化/反序列化或插件管线，请在构建前设置相应管理器。
    /// </remarks>
    public class LinkClientBuilder<TLinkClient>
        where TLinkClient : ILinkClientAwareSender<TLinkClient>
    {
        /// <summary>
        /// 服务提供者，用于解析依赖。
        /// </summary>
        private readonly IServiceProvider _provider;

        /// <summary>
        /// 用于创建 <typeparamref name="TLinkClient"/> 实例的工厂。可在构建前替换（若替换为 null 则无法构造客户端）。
        /// </summary>
        public ILinkClientFactory<TLinkClient>? ClientFactory { get; set; }

        /// <summary>
        /// 可选的插件管理器；若设置，将在构建时通过 <see cref="ILinkClientAwareSender{TLinkClient}.SetClientPluginManager"/> 注入到客户端。
        /// </summary>
        public ILinkClientPluginManager<TLinkClient>? PluginManager { get; set; }

        /// <summary>
        /// 可选的格式化器管理器；若设置，将在构建时通过 <see cref="ILinkClientAwareSender{TLinkClient}.SetClientFormatterManager"/> 注入到客户端。
        /// </summary>
        public ILinkClientFormatterManager? FormatterManager { get; set; }

        /// <summary>
        /// 使用运行时服务提供者与默认客户端工厂初始化构建器。
        /// </summary>
        /// <param name="provider">用于构建时解析依赖的 <see cref="IServiceProvider"/>，不得为 null。</param>
        /// <param name="factory">创建客户端实例的工厂，实现不得为 null。</param>
        public LinkClientBuilder(IServiceProvider provider, ILinkClientFactory<TLinkClient> factory)
        {
            ClientFactory = factory;
            _provider = provider;
        }

        /// <summary>
        /// 初始化并设置一个 <see cref="ILinkClientFormatterManager"/> 实例，并通过回调允许调用方注册格式化器。
        /// </summary>
        /// <param name="action">用于配置 <see cref="FormatterManagerBuilder"/> 的委托；可为 null（表示仅初始化空管理器）。</param>
        /// <returns>当前构建器实例（便于链式调用）。</returns>
        /// <remarks>
        /// - 本方法会创建一个内部 <see cref="LinkClientFormatterManager"/> 实例并赋值给 <see cref="FormatterManager"/>。  
        /// - 回调中可使用提供的 <see cref="FormatterManagerBuilder"/> 注册格式化器；回调异常会直接向上抛出。
        /// </remarks>
        public LinkClientBuilder<TLinkClient> SetFormatterManager(Action<FormatterManagerBuilder> action)
        {
            LinkClientFormatterManager manager = new();
            FormatterManager = manager;
            action?.Invoke(new FormatterManagerBuilder(_provider, FormatterManager));
            return this;
        }

        /// <summary>
        /// 使用默认地址族（IPv4）构建并返回客户端实例。
        /// </summary>
        /// <returns>构建并注入可选管理器后的 <typeparamref name="TLinkClient"/> 实例。</returns>
        public TLinkClient Build()
        {
            return Build(AddressFamily.InterNetwork);
        }

        /// <summary>
        /// 按指定地址族创建客户端实例并返回。依赖于 <see cref="ClientFactory"/> 提供对指定地址族的创建支持。
        /// </summary>
        /// <param name="addressFamily">用于创建底层套接字的地址族（例如 <see cref="AddressFamily.InterNetwork"/> 或 <see cref="AddressFamily.InterNetworkV6"/>）。</param>
        /// <returns>构建并注入可选管理器后的 <typeparamref name="TLinkClient"/> 实例。</returns>
        /// <exception cref="InvalidOperationException">当 <see cref="ClientFactory"/> 未设置时抛出。</exception>
        public TLinkClient Build(AddressFamily addressFamily)
        {
            if (ClientFactory is null)
                throw new InvalidOperationException("ClientFactory 未设置，无法创建 Linker 实例。");

            return Build(ClientFactory.CreateLinkClient(addressFamily));
        }

        /// <summary>
        /// 使用现有 <see cref="Socket"/> 创建并返回客户端实例。
        /// </summary>
        /// <param name="socket">已创建的 <see cref="Socket"/>；由工厂将其包装为目标客户端实例。</param>
        /// <returns>构建并注入可选管理器后的 <typeparamref name="TLinkClient"/> 实例。</returns>
        /// <exception cref="InvalidOperationException">当 <see cref="ClientFactory"/> 未设置时抛出。</exception>
        public TLinkClient Build(Socket socket)
        {
            if (ClientFactory is null)
                throw new InvalidOperationException("ClientFactory 未设置，无法创建 Linker 实例。");

            return Build(ClientFactory.CreateLinkClient(socket));
        }

        /// <summary>
        /// 使用现有的 <see cref="ILinker"/> 实例创建并返回客户端实例。
        /// </summary>
        /// <param name="linker">已有<see cref="ILinker"/> 实例。</param>
        /// <returns><see cref="TLinkClient"/>实例。</returns>
        /// <exception cref="InvalidOperationException">当 <see cref="ClientFactory"/> 未设置时抛出。</exception>
        public TLinkClient Build(ILinker linker)
        {
            if (ClientFactory is null)
                throw new InvalidOperationException("ClientFactory 未设置，无法创建 Linker 实例。");

            return Build(ClientFactory.CreateLinkClient(linker));
        }

        /// <summary>
        /// 将传入的客户端实例注入已配置的管理器并返回该实例。
        /// </summary>
        /// <param name="client">目标客户端实例，不能为 null。</param>
        /// <returns>已注入 <see cref="FormatterManager"/> 与 <see cref="PluginManager"/>（如果已设置）的客户端实例。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
        /// <remarks>
        /// - 如果 <see cref="FormatterManager"/> 或 <see cref="PluginManager"/> 非空，方法会调用客户端的相应设置方法进行注入。  
        /// - 方法不会对客户端做进一步初始化或启动连接；仅完成依赖注入式的配置。
        /// </remarks>
        public TLinkClient Build(TLinkClient client)
        {
            ArgumentNullException.ThrowIfNull(client, nameof(client));

            if (FormatterManager is not null)
                client.SetClientFormatterManager(FormatterManager);

            if (PluginManager is not null)
                client.SetClientPluginManager(PluginManager);

            return client;
        }
    }
}