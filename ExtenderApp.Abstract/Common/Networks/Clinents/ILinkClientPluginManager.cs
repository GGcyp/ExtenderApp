namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 客户端插件管理器接口：用于注册与托管多个 <see cref="ILinkClientPlugin{TLinker}"/> 实例，
    /// 并以聚合方式实现 <see cref="ILinkClientPlugin{TLinker}"/> 的回调契约（将事件分发给已注册插件）。
    /// </summary>
    /// <typeparam name="TLinker">关联的链路实现类型。</typeparam>
    public interface ILinkClientPluginManager<TLinker> : ILinkClientPlugin<TLinker>
        where TLinker : ILinker
    {
        /// <summary>
        /// 注册一个插件实例到管理器中。注册后该插件将参与后续的连接生命周期与收发事件处理。
        /// </summary>
        /// <typeparam name="T">插件类型，必须实现 <see cref="ILinkClientPlugin{TLinker}"/>。</typeparam>
        /// <param name="plugin">要注册的插件实例，不能为空。</param>
        void AddPlugin<T>(T plugin) where T : ILinkClientPlugin<TLinker>;

        /// <summary>
        /// 移除指定类型的插件（按类型 T），如果存在则移除并返回 true。
        /// </summary>
        /// <typeparam name="T">要移除的插件类型。</typeparam>
        /// <returns>若移除成功则为 true，否则 false。</returns>
        bool RemovePlugin<T>() where T : ILinkClientPlugin<TLinker>;

        /// <summary>
        /// 尝试按类型获取已注册的插件实例。
        /// </summary>
        /// <typeparam name="T">插件类型。</typeparam>
        /// <param name="plugin">当返回 true 时包含插件实例，否则为 null。</param>
        /// <returns>若找到对应类型的插件则为 true。</returns>
        bool TryGetPlugin<T>(out T? plugin) where T : class, ILinkClientPlugin<TLinker>;

        /// <summary>
        /// 替换已存在的插件实例（按类型 T）。若指定类型不存在将抛出异常。
        /// </summary>
        /// <typeparam name="T">插件类型。</typeparam>
        /// <param name="plugin">新的插件实例，不能为空。</param>
        void ReplacePlugin<T>(T plugin) where T : ILinkClientPlugin<TLinker>;

        /// <summary>
        /// 以只读列表形式获取当前已注册的插件快照（用于查询/调试）。
        /// </summary>
        IReadOnlyList<ILinkClientPlugin<TLinker>> GetPlugins();
    }
}
