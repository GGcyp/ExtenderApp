namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端插件管理器接口。
    /// 用于注册、托管与分发多个 <see cref="ILinkClientPlugin{TLinker}"/> 实例的回调。
    /// 实现者应将自身作为一个聚合的 <see cref="ILinkClientPlugin{TLinker}"/>，在对应生命周期与收发回调发生时
    /// 按注册顺序（或实现约定的顺序）将事件逐一分发到已注册的插件。
    /// </summary>
    /// <typeparam name="TLinkClient">关联的链路实现类型。</typeparam>
    /// <remarks>
    /// - 实现应保证线程安全（可在并发上下文中安全注册/移除/查询插件）。  
    /// - 在分发回调给各插件时，推荐捕获单个插件抛出的异常并记录/聚合，避免单个插件异常阻断其它插件的执行；具体异常传播策略由实现或上层决定。  
    /// - 对于 OnSend/OnReceive 等涉及 <c>ref struct</c> 的回调，管理器必须在同一调用栈内依次调用插件，确保不将栈上类型跨异步边界或存储到堆上。
    /// </remarks>
    public interface ILinkClientPluginManager<TLinkClient> : ILinkClientPlugin<TLinkClient>
        where TLinkClient : ILinkClientAwareSender<TLinkClient>
    {
        /// <summary>
        /// 注册一个插件实例到管理器中。
        /// 注册后该插件将参与后续的连接生命周期与收发事件处理，并按实现约定的顺序参与事件分发。
        /// </summary>
        /// <typeparam name="T">插件类型，必须实现 <see cref="ILinkClientPlugin{TLinker}"/>。</typeparam>
        /// <param name="plugin">要注册的插件实例，不能为空。实现应在收到 null 时抛出 <see cref="ArgumentNullException"/>。</param>
        /// <remarks>
        /// - 是否允许重复注册同一实例或同一类型的多个实例由实现决定；使用者应参考具体实现文档。  
        /// - 注册操作应尽可能轻量，不应阻塞较长时间；若需要初始化工作，可由插件在 <see cref="ILinkClientPlugin{TLinkClient}.OnAttach"/> 中完成。
        /// </remarks>
        void AddPlugin<T>(T plugin) where T : ILinkClientPlugin<TLinkClient>;

        /// <summary>
        /// 按类型移除已注册的插件。
        /// </summary>
        /// <typeparam name="T">要移除的插件类型。</typeparam>
        /// <returns>
        /// 若成功移除至少一个匹配类型的插件则返回 true；若未找到匹配项则返回 false。  
        /// 如果存在多个同类型实例，默认行为为移除第一个匹配项（或由实现选择移除全部，具体契约以实现为准）。
        /// </returns>
        /// <remarks>
        /// - 移除成功后，管理器应在适当时机调用被移除插件的 <see cref="ILinkClientPlugin{TLinkClient}.OnDetach"/>（若尚未调用）。  
        /// - 移除操作应为线程安全。
        /// </remarks>
        bool RemovePlugin<T>() where T : ILinkClientPlugin<TLinkClient>;

        /// <summary>
        /// 尝试按类型获取已注册的插件实例（引用快照）。
        /// </summary>
        /// <typeparam name="T">插件类型。</typeparam>
        /// <param name="plugin">当返回 true 时包含找到的插件实例（通常为首次匹配项或实现约定的匹配项），否则为 null。</param>
        /// <returns>若找到对应类型的插件则为 true；否则返回 false。</returns>
        /// <remarks>
        /// - 返回的实例为当前管理器内部引用的其中一个插件实例（若存在），调用方不得假定其为不可变副本。  
        /// - 为避免竞态，调用方在并发场景中应当在合适的同步策略下使用返回的插件引用或再次通过 GetPlugins() 获取快照。
        /// </remarks>
        bool TryGetPlugin<T>(out T? plugin) where T : class, ILinkClientPlugin<TLinkClient>;

        /// <summary>
        /// 替换已存在的插件实例（按类型 T）。
        /// </summary>
        /// <typeparam name="T">插件类型。</typeparam>
        /// <param name="plugin">新的插件实例，不能为空。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="plugin"/> 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">若指定类型的插件当前不存在则抛出（实现可改为返回 bool，但此接口语义为抛出）。</exception>
        /// <remarks>
        /// - 替换操作应保证原插件的 <see cref="ILinkClientPlugin{TLinkClient}.OnDetach"/> 在适当时机被调用，且新插件的 <see cref="ILinkClientPlugin{TLinkClient}.OnAttach"/> 被调用（如果语义需要）。  
        /// - 实现应保证替换为原子操作，以避免在并发环境中出现中间不一致状态。
        /// </remarks>
        void ReplacePlugin<T>(T plugin) where T : ILinkClientPlugin<TLinkClient>;

        /// <summary>
        /// 返回当前已注册插件的一个只读快照列表，供查询或调试使用。
        /// </summary>
        /// <returns>
        /// 插件快照的只读列表（调用者不应依赖返回集合在后续操作中保持实时同步；若需最新状态请再次调用本方法）。
        /// </returns>
        /// <remarks>
        /// - 为避免并发问题，该方法通常返回内部集合的浅拷贝或不可变视图。  
        /// - 不应将此列表用于决定并发修改策略；修改插件集合应通过 AddPlugin/RemovePlugin/ReplacePlugin 等方法进行。
        /// </remarks>
        IReadOnlyList<ILinkClientPlugin<TLinkClient>> GetPlugins();
    }
}
