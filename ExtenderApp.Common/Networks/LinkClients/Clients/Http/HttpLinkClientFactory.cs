using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    /// <summary>
    /// HTTP 链路客户端工厂。
    /// 基于 <see cref="ILinkerFactory{TLinker}"/> 提供的 <see cref="ITcpLinker"/> 创建 <see cref="IHttpLinkClient"/> 实例。
    /// </summary>
    /// <remarks>
    /// - 该工厂继承自 <see cref="LinkClientFactory{TLinkClient, TLinker}"/>，复用其基于地址族或现有 Socket 构建链接器的能力；<br/>
    /// - 实际客户端由 <see cref="HttpLinkClient"/> 实现，通过覆盖 <see cref="CreateLinkClient(ITcpLinker)"/> 完成适配；<br/>
    /// - 可用于依赖注入场景中按需创建轻量 HTTP 客户端。
    /// </remarks>
    internal class HttpLinkClientFactory : LinkClientFactory<IHttpLinkClient, ITcpLinker>
    {
        /// <summary>
        /// 使用指定的 TCP 链接器工厂初始化 HTTP 客户端工厂。
        /// </summary>
        /// <param name="linkerFactory">用于创建 <see cref="ITcpLinker"/> 的链接器工厂实例。</param>
        public HttpLinkClientFactory(ILinkerFactory<ITcpLinker> linkerFactory) : base(linkerFactory)
        {
        }

        /// <summary>
        /// 基于给定的 <see cref="ITcpLinker"/> 创建一个 <see cref="IHttpLinkClient"/> 实例。
        /// </summary>
        /// <param name="linker">底层 TCP 链接器。</param>
        /// <returns>创建的 <see cref="IHttpLinkClient"/> 实例。</returns>
        protected override IHttpLinkClient CreateLinkClient(ITcpLinker linker)
        {
            return new HttpLinkClient(linker);
        }
    }
}
