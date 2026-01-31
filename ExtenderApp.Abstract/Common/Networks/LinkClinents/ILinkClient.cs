namespace ExtenderApp.Abstract
{
    /// <summary>
    /// 表示一个可发起和管理网络连接的客户端实体。
    /// <para>组合了帧器（Framer）与插件管理器（PluginManager）的能力，并继承自 <see cref="ILinker"/>。</para>
    /// </summary>
    public interface ILinkClient : ILinker
    {
        /// <summary>
        /// 获取当前客户端使用的帧器实例；若未设置则返回 <c>null</c>。
        /// </summary>
        ILinkClientFramer? Framer { get; }

        /// <summary>
        /// 获取当前客户端使用的插件管理器实例；若未设置则返回 <c>null</c>。
        /// </summary>
        ILinkClientPluginManager? PluginManager { get; }

        /// <summary>
        /// 设置客户端使用的帧器实例。
        /// </summary>
        /// <param name="framer">要设置的 <see cref="ILinkClientFramer"/> 实例；不得为 <c>null</c>。</param>
        void SetClientFramer(ILinkClientFramer framer);

        /// <summary>
        /// 设置客户端使用的插件管理器实例。
        /// </summary>
        /// <param name="pluginManager">要设置的 <see cref="ILinkClientPluginManager"/> 实例；不得为 <c>null</c>。</param>
        void SetClientPluginManager(ILinkClientPluginManager pluginManager);
    }
}