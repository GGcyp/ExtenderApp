using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks.LinkClients;

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
    public class LinkClientBuilder
    {
        /// <summary>
        /// 服务提供者，用于解析依赖。
        /// </summary>
        private readonly IServiceProvider _provider;

    }
}