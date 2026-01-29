namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端插件管理器接口 — 聚合并分发多个 <see cref="ILinkClientPlugin"/> 的回调。
    /// 实现负责注册、托管并按约定顺序将生命周期与收发事件逐一分发到已注册插件。
    /// </summary>
    /// <typeparam name="TLinkClient">关联的链路实现类型（仅用于实现约定文档，接口不直接使用该类型参数）。</typeparam>
    /// <remarks>
    /// - 管理器应保证在并发场景下安全注册/移除/查询插件（线程安全）。
    /// - 在分发回调时，建议捕获单个插件抛出的异常并记录或聚合，避免单个插件异常阻断其它插件执行；具体异常传播策略由实现决定。
    /// - 对于涉及 <c>ref struct</c>（如 <see cref="FrameContext"/>）的方法（OnSend/OnReceive），管理器必须在同一调用栈内按顺序调用插件，避免将栈上类型跨异步边界或存储到堆上。
    /// </remarks>
    public interface ILinkClientPluginManager : ILinkClientPlugin
    {
        ILinkClientPlugin? this[int index] { get; }

        /// <summary>
        /// 向管理器注册一个插件实例，使其参与后续生命周期与收发事件分发。
        /// </summary>
        /// <typeparam name="T">插件类型，必须实现 <see cref="ILinkClientPlugin"/>。</typeparam>
        /// <param name="plugin">要注册的插件实例；不得为 null。</param>
        /// <exception cref="ArgumentNullException"><paramref name="plugin"/> 为 null 时抛出。</exception>
        /// <remarks>
        /// - 是否允许重复注册同一实例或同一类型由具体实现决定；调用方应参照实现文档。
        /// - 注册应尽量轻量；若插件需要较重的初始化操作，请在插件的 <see cref="ILinkClientPlugin.OnAttach"/> 中完成。
        /// </remarks>
        void AddPlugin(int priority, ILinkClientPlugin plugin);

        void AddPlugin<T>(int priority) where T : class, ILinkClientPlugin;

        /// <summary>
        /// 按类型移除已注册插件（默认移除第一个匹配项，或由实现决定移除策略）。
        /// </summary>
        /// <typeparam name="T">要移除的插件类型。</typeparam>
        /// <returns>若成功移除至少一个匹配类型的插件则返回 true；否则返回 false。</returns>
        /// <remarks>
        /// - 移除成功后，管理器应在适当时机调用被移除插件的 <see cref="ILinkClientPlugin.OnDetach"/>（若尚未调用）。
        /// - 移除操作应为线程安全，并对并发注册/移除保持一致性。
        /// </remarks>
        bool RemovePlugin<T>() where T : class, ILinkClientPlugin;

        /// <summary>
        /// 尝试按类型获取已注册的插件实例（返回当前快照中的一个匹配项引用）。
        /// </summary>
        /// <typeparam name="T">插件类型。</typeparam>
        /// <param name="plugin">当返回 true 时包含找到的插件实例；否则为 null。</param>
        /// <returns>找到对应类型的插件则返回 true；否则返回 false。</returns>
        /// <remarks>
        /// - 返回的是管理器当前持有的实例引用（调用方不得假定为不可变副本）。
        /// - 在高度并发场景下，若需确保一致性请在调用方侧采用适当同步或再次获取快照。
        /// </remarks>
        bool TryGetPlugin<T>(out T plugin) where T : class, ILinkClientPlugin;

        /// <summary>
        /// 获取当前注册插件的只读快照列表，供查询或调试使用。
        /// </summary>
        /// <returns>插件快照的只读集合；调用者不应依赖该集合在后续操作中保持实时同步。</returns>
        /// <remarks>
        /// - 若需最新状态，请再次调用此方法；对快照的迭代应在调用方侧考虑并发变更的可能性。
        /// - 返回集合中插件实例的释放/生命周期由管理器或其上层持有者负责（按项目约定）。
        /// </remarks>
        IEnumerable<ILinkClientPlugin> GetPlugins();
    }
}